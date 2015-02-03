using JSONAPI.Attributes;
using JSONAPI.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JSONAPI.Extensions;

namespace JSONAPI.Json
{
    public class JsonApiFormatter : JsonMediaTypeFormatter
    {
        public JsonApiFormatter(IModelManager modelManager) :
            this(modelManager, new ErrorSerializer())
        {
        }

        public JsonApiFormatter(IPluralizationService pluralizationService) :
            this(new ModelManager(pluralizationService))
        {
        }

        // Currently for tests only.
        internal JsonApiFormatter(IErrorSerializer errorSerializer)
            : this(new ModelManager(new PluralizationService()), errorSerializer)
        {
            
        }

        internal JsonApiFormatter(IModelManager modelManager, IErrorSerializer errorSerializer)
        {
            _modelManager = modelManager;
            _errorSerializer = errorSerializer;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.api+json"));
            ValidateRawJsonStrings = true;
        }

        public bool ValidateRawJsonStrings { get; set; }

        [Obsolete("Use ModelManager.PluralizationService instead")]
        public IPluralizationService PluralizationService  //FIXME: Deprecated, will be removed shortly
        {
            get
            {
                return _modelManager.PluralizationService;
            }
        }

        private readonly IErrorSerializer _errorSerializer;

        private readonly IModelManager _modelManager;
        public IModelManager ModelManager
        {
            get
            {
                return _modelManager;
            }
        }

        private Lazy<Dictionary<Stream, RelationAggregator>> _relationAggregators
            = new Lazy<Dictionary<Stream, RelationAggregator>>(
                () => new Dictionary<Stream, RelationAggregator>()
            );
        public Dictionary<Stream, RelationAggregator> RelationAggregators
        {
            get { return _relationAggregators.Value; }
        }

        public override bool CanReadType(Type t)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        #region Serialization

        public override Task WriteToStreamAsync(System.Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            RelationAggregator aggregator;
            lock (this.RelationAggregators)
            {
                if (this.RelationAggregators.ContainsKey(writeStream))
                    aggregator = this.RelationAggregators[writeStream];
                else
                {
                    aggregator = new RelationAggregator();
                    this.RelationAggregators[writeStream] = aggregator;
                }
            }
            
            var contentHeaders = content == null ? null : content.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            JsonWriter writer = this.CreateJsonWriter(typeof(object), writeStream, effectiveEncoding);
            JsonSerializer serializer = this.CreateJsonSerializer();
            if (_errorSerializer.CanSerialize(type))
            {
                // `value` is an error
                _errorSerializer.SerializeError(value, writeStream, writer, serializer);
            }
            else
            {
                Type valtype = GetSingleType(value.GetType());
                if (_modelManager.IsSerializedAsMany(value.GetType()))
                    aggregator.AddPrimary(valtype, (IEnumerable<object>) value);
                else
                    aggregator.AddPrimary(valtype, value);

                //writer.Formatting = Formatting.Indented;

                var root = _modelManager.GetJsonKeyForType(type);

                writer.WriteStartObject();
                writer.WritePropertyName(root);
                if (_modelManager.IsSerializedAsMany(value.GetType()))
                    this.SerializeMany(value, writeStream, writer, serializer, aggregator);
                else
                    this.Serialize(value, writeStream, writer, serializer, aggregator);

                // Include links from aggregator
                SerializeLinkedResources(writeStream, writer, serializer, aggregator);

                writer.WriteEndObject();
            }
            writer.Flush();

            lock (this.RelationAggregators)
            {
                this.RelationAggregators.Remove(writeStream);
            }

            //return base.WriteToStreamAsync(type, obj as object, writeStream, content, transportContext);

            //TODO: For now we won't worry about optimizing this down into smaller async parts, we'll just do it all synchronous. So...
            // Just return a completed task... from http://stackoverflow.com/a/17527551/489116
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(null);
            return tcs.Task;
        }

        protected void SerializeMany(object value, Stream writeStream, JsonWriter writer, JsonSerializer serializer, RelationAggregator aggregator)
        {
            writer.WriteStartArray();
            foreach (object singleVal in (IEnumerable)value)
            {
                this.Serialize(singleVal, writeStream, writer, serializer, aggregator);
            }
            writer.WriteEndArray();
        }

        protected void Serialize(object value, Stream writeStream, JsonWriter writer, JsonSerializer serializer, RelationAggregator aggregator)
        {
            writer.WriteStartObject();

            // The spec no longer requires that the ID key be "id":
            //     "An ID SHOULD be represented by an 'id' key..." :-/
            // But Ember Data does. So, we'll add "id" to the document
            // always, and also serialize the property under its given
            // name, for now at least.
            //TODO: Partly because of this, we should probably disallow updates to Id properties where practical.

            // Do the Id now...
            writer.WritePropertyName("id");
            var idProp = _modelManager.GetIdProperty(value.GetType());
            writer.WriteValue(GetValueForIdProperty(idProp, value));

            // Leverage the cached map to avoid another costly call to System.Type.GetProperties()
            PropertyInfo[] props = _modelManager.GetProperties(value.GetType());

            // Do non-model properties first, everything else goes in "links"
            //TODO: Unless embedded???
            IList<PropertyInfo> modelProps = new List<PropertyInfo>();

            foreach (PropertyInfo prop in props)
            {
                string propKey = _modelManager.GetJsonKeyForProperty(prop);
                if (propKey == "id") continue; // Don't write the "id" property twice, see above!

                if (prop.PropertyType.CanWriteAsJsonApiAttribute())
                {
                    if (prop.GetCustomAttributes().Any(attr => attr is JsonIgnoreAttribute))
                        continue;

                    // numbers, strings, dates...
                    writer.WritePropertyName(propKey);

                    var propertyValue = prop.GetValue(value, null);

                    if (prop.PropertyType == typeof (Decimal) || prop.PropertyType == typeof (Decimal?))
                    {
                        if (propertyValue == null)
                            writer.WriteNull();
                        else
                            writer.WriteValue(propertyValue.ToString());
                    }
                    else if (prop.PropertyType == typeof (string) &&
                        prop.GetCustomAttributes().Any(attr => attr is SerializeStringAsRawJsonAttribute))
                    {
                        if (propertyValue == null)
                        {
                            writer.WriteNull();
                        }
                        else
                        {
                            var json = (string) propertyValue;
                            if (ValidateRawJsonStrings)
                            {
                                try
                                {
                                    var token = JToken.Parse(json);
                                    json = token.ToString();
                                }
                                catch (Exception)
                                {
                                    json = "{}";
                                }
                            }
                            var valueToSerialize = JsonHelpers.MinifyJson(json);
                            writer.WriteRawValue(valueToSerialize);
                        }
                    }
                    else
                    {
                        serializer.Serialize(writer, propertyValue);
                    }
                }
                else
                {
                    modelProps.Add(prop);
                    continue;
                }
            }

            // Now do other stuff
            if (modelProps.Count() > 0)
            {
                writer.WritePropertyName("links");
                writer.WriteStartObject();
            }
            foreach (PropertyInfo prop in modelProps)
            {
                bool skip = false, iip = false;
                string lt = null;
                SerializeAsOptions sa = SerializeAsOptions.Ids;

                object[] attrs = prop.GetCustomAttributes(true);

                foreach (object attr in attrs)
                {
                    Type attrType = attr.GetType();
                    if (typeof(JsonIgnoreAttribute).IsAssignableFrom(attrType))
                    {
                        skip = true;
                        continue;
                    }
                    if (typeof(IncludeInPayload).IsAssignableFrom(attrType))
                        iip = ((IncludeInPayload)attr).Include;
                    if (typeof(SerializeAs).IsAssignableFrom(attrType))
                        sa = ((SerializeAs)attr).How;
                    if (typeof(LinkTemplate).IsAssignableFrom(attrType))
                        lt = ((LinkTemplate)attr).TemplateString;
                }
                if (skip) continue;

                writer.WritePropertyName(_modelManager.GetJsonKeyForProperty(prop));

                // Look out! If we want to SerializeAs a link, computing the property is probably 
                // expensive...so don't force it just to check for null early!
                if (sa != SerializeAsOptions.Link && prop.GetValue(value, null) == null)
                {
                    writer.WriteNull();
                    continue;
                }

                // Now look for enumerable-ness:
                if (typeof(IEnumerable<Object>).IsAssignableFrom(prop.PropertyType))
                {
                    switch (sa)
                    {
                        case SerializeAsOptions.Ids:
                            //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
                            IEnumerable<object> items = (IEnumerable<object>)prop.GetValue(value, null);
                            if (items == null)
                            {
                                writer.WriteValue((IEnumerable<object>)null); //TODO: Is it okay with the spec and Ember Data to return null for an empty array?
                                break; // LOOK OUT! Ending this case block early here!!!
                            }
                            this.WriteIdsArrayJson(writer, items, serializer);
                            if (iip)
                            {
                                Type itemType;
                                if (prop.PropertyType.IsGenericType)
                                {
                                    itemType = prop.PropertyType.GetGenericArguments()[0];
                                }
                                else
                                {
                                    // Must be an array at this point, right??
                                    itemType = prop.PropertyType.GetElementType();
                                }
                                if (aggregator != null) aggregator.Add(itemType, items); // should call the IEnumerable one...right?
                            }
                            break;
                        case SerializeAsOptions.Link:
                            if (lt == null) throw new JsonSerializationException("A property was decorated with SerializeAs(SerializeAsOptions.Link) but no LinkTemplate attribute was provided.");
                            //TODO: Check for "{0}" in linkTemplate and (only) if it's there, get the Ids of all objects and "implode" them.
                            string href = String.Format(lt, null, GetIdFor(value));
                            //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
                            //TODO: Support ids and type properties in "link" object
                            writer.WriteStartObject();
                            writer.WritePropertyName("href");
                            writer.WriteValue(href);
                            writer.WriteEndObject();
                            break;
                        case SerializeAsOptions.Embedded:
                            // Not really supported by Ember Data yet, incidentally...but easy to implement here.
                            //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
                            //serializer.Serialize(writer, prop.GetValue(value, null));
                            this.Serialize(prop.GetValue(value, null), writeStream, writer, serializer, aggregator);
                            break;
                    }
                }
                else
                {
                    var propertyValue = prop.GetValue(value, null);
                    if (propertyValue == null)
                    {
                        writer.WriteNull();
                    }
                    else
                    {
                        string objId = GetIdFor(propertyValue);

                        switch (sa)
                        {
                            case SerializeAsOptions.Ids:
                                //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
                                serializer.Serialize(writer, objId);
                                if (iip)
                                    if (aggregator != null)
                                        aggregator.Add(prop.PropertyType, prop.GetValue(value, null));
                                break;
                            case SerializeAsOptions.Link:
                                if (lt == null)
                                    throw new JsonSerializationException(
                                        "A property was decorated with SerializeAs(SerializeAsOptions.Link) but no LinkTemplate attribute was provided.");
                                string link = String.Format(lt, objId,  
                                    GetIdFor(value)); //value.GetType().GetProperty("Id").GetValue(value, null));
                                    
                                //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
                                writer.WriteStartObject();
                                writer.WritePropertyName("href");
                                writer.WriteValue(link);
                                writer.WriteEndObject();
                                break;
                            case SerializeAsOptions.Embedded:
                                // Not really supported by Ember Data yet, incidentally...but easy to implement here.
                                //writer.WritePropertyName(ContractResolver._modelManager.GetJsonKeyForProperty(prop));
                                //serializer.Serialize(writer, prop.GetValue(value, null));
                                this.Serialize(prop.GetValue(value, null), writeStream, writer, serializer, aggregator);
                                break;
                        }
                    }
                }

            }
            if (modelProps.Count() > 0)
            {
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        protected void SerializeLinkedResources(Stream writeStream, JsonWriter writer, JsonSerializer serializer, RelationAggregator aggregator)
        {
            /* This is a bit messy, because we may add items of a given type to the
             * set we are currently processing. Not only is this an issue because you
             * can't modify a set while you're enumerating it (hence why we make a
             * copy first), but we need to catch the newly added objects and process
             * them as well. So, we have to keep making passes until we detect that
             * we haven't added any new objects to any of the appendices.
             */
            Dictionary<Type, ISet<object>>
                processed = new Dictionary<Type, ISet<object>>(),
                toBeProcessed = new Dictionary<Type, ISet<object>>(); // is this actually necessary?
            /* On top of that, we need a new JsonWriter for each appendix--because we
             * may write objects of type A, then while processing type B find that
             * we need to write more objects of type A! So we can't keep appending
             * to the same writer.
             */
            /* Oh, and we have to keep a reference to the TextWriter of the JsonWriter
             * because there's no member to get it back out again. ?!?
             * */
            Dictionary<Type, KeyValuePair<JsonWriter, StringWriter>> writers = new Dictionary<Type, KeyValuePair<JsonWriter, StringWriter>>();

            int numAdditions;
            do
            {
                numAdditions = 0;
                Dictionary<Type, ISet<object>> appxs = new Dictionary<Type, ISet<object>>(aggregator.Appendices); // shallow clone, in case we add a new type during enumeration!
                foreach (KeyValuePair<Type, ISet<object>> apair in appxs)
                {
                    Type type = apair.Key;
                    ISet<object> appendix = apair.Value;
                    JsonWriter jw;
                    if (writers.ContainsKey(type))
                    {
                        jw = writers[type].Key;
                    }
                    else
                    {
                        // Setup and start the writer for this type...
                        StringWriter sw = new StringWriter();
                        jw = new JsonTextWriter(sw);
                        writers[type] = new KeyValuePair<JsonWriter, StringWriter>(jw, sw);
                        jw.WriteStartArray();
                    }

                    HashSet<object> tbp;
                    if (processed.ContainsKey(type))
                    {
                        toBeProcessed[type] = tbp = new HashSet<object>(appendix.Except(processed[type]));
                    }
                    else
                    {
                        toBeProcessed[type] = tbp = new HashSet<object>(appendix);
                        processed[type] = new HashSet<object>();
                    }

                    if (tbp.Count > 0)
                    {
                        numAdditions += tbp.Count;
                        foreach (object obj in tbp)
                        {
                            Serialize(obj, writeStream, jw, serializer, aggregator); // Note, not writer, but jw--we write each type to its own JsonWriter and combine them later.
                        }
                        processed[type].UnionWith(tbp);
                    }

                    //TODO: Add traversal depth limit??
                }
            } while (numAdditions > 0);

            if (aggregator.Appendices.Count > 0)
            {
                writer.WritePropertyName("linked");
                writer.WriteStartObject();

                // Okay, we should have captured everything now. Now combine the type writers into the main writer...
                foreach (KeyValuePair<Type, KeyValuePair<JsonWriter, StringWriter>> apair in writers)
                {
                    apair.Value.Key.WriteEnd(); // close off the array
                    writer.WritePropertyName(_modelManager.GetJsonKeyForType(apair.Key));
                    writer.WriteRawValue(apair.Value.Value.ToString()); // write the contents of the type JsonWriter's StringWriter to the main JsonWriter
                }

                writer.WriteEndObject();
            }


        }

        #endregion Serialization

        #region Deserialization

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.FromResult(ReadFromStream(type, readStream, content, formatterLogger)); ;
        }

        private object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            object retval = null;
            Type singleType = GetSingleType(type);
            var pripropname = _modelManager.GetJsonKeyForType(type);
            var contentHeaders = content == null ? null : content.Headers;

            // If content length is 0 then return default value for this type
            if (contentHeaders != null && contentHeaders.ContentLength == 0)
                return GetDefaultValueForType(type);


            try
            {

                var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
                JsonReader reader = this.CreateJsonReader(typeof(IDictionary<string, object>), readStream, effectiveEncoding);
                JsonSerializer serializer = this.CreateJsonSerializer();

                reader.Read();
                if (reader.TokenType != JsonToken.StartObject) throw new JsonSerializationException("Document root is not an object!");

                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string value = (string)reader.Value;
                        reader.Read(); // burn the PropertyName token
                        switch (value)
                        {
                            case "linked":
                                //TODO: If we want to capture linked/related objects in a compound document when deserializing, do it here...do we?
                                reader.Skip();
                                break;
                            case "links":
                                // ignore this, is it even meaningful in a PUT/POST body?
                                reader.Skip();
                                break;
                            default:
                                if (value == pripropname)
                                {
                                    // Could be a single resource or multiple, according to spec!
                                    if (reader.TokenType == JsonToken.StartArray)
                                    {
                                        Type listType = (typeof(List<>)).MakeGenericType(singleType);
                                        retval = (IList)Activator.CreateInstance(listType);
                                        reader.Read(); // Burn off StartArray token
                                        while (reader.TokenType == JsonToken.StartObject)
                                        {
                                            ((IList)retval).Add(Deserialize(singleType, readStream, reader, serializer));
                                        }
                                        // burn EndArray token...
                                        if (reader.TokenType != JsonToken.EndArray) throw new JsonReaderException(String.Format("Expected JsonToken.EndArray but got {0}", reader.TokenType));
                                        reader.Read();
                                    }
                                    else
                                    {
                                        // Because we choose what to deserialize based on the ApiController method signature
                                        // (not choose the method signature based on what we're deserializing), the `type`
                                        // parameter will always be `IList<Model>` even if a single model is sent!
                                        retval = Deserialize(singleType, readStream, reader, serializer);
                                    }
                                }
                                else
                                    reader.Skip();
                                break;
                        }
                    }
                    else
                        reader.Skip();
                }

                /* WARNING: May transform a single object into a list of objects!!!
                 * This is a necessary workaround to support POST and PUT of multiple 
                 * resoruces as per the spec, because WebAPI does NOT allow you to overload 
                 * a POST or PUT method based on the input parameter...i.e., you cannot 
                 * have a method that responds to POST of a single resource and a second 
                 * method that responds to the POST of a collection (list) of resources!
                 */
                if (retval != null)
                {
                    if (!type.IsAssignableFrom(retval.GetType()) && _modelManager.IsSerializedAsMany(type))
                    {
                        IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(singleType));
                        list.Add(retval);
                        return list;
                    }
                    else
                    {
                        return retval;
                    }
                }

            }
            catch (Exception e)
            {
                if (formatterLogger == null)
                {
                    throw;
                }
                formatterLogger.LogError(String.Empty, e);
                return GetDefaultValueForType(type);
            }

            /*
            try
            {
                using (var reader = (new StreamReader(readStream, effectiveEncoding)))
                {
                    var json = reader.ReadToEnd();
                    var jo = JObject.Parse(json);
                    return jo.SelectToken(root, false).ToObject(type);
                }
            }
            catch (Exception e)
            {
                if (formatterLogger == null)
                {
                    throw;
                }
                formatterLogger.LogError(String.Empty, e);
                return GetDefaultValueForType(type);
            }
             */

            return GetDefaultValueForType(type);
        }

        public object Deserialize(Type objectType, Stream readStream, JsonReader reader, JsonSerializer serializer)
        {
            object retval = Activator.CreateInstance(objectType);

            if (reader.TokenType != JsonToken.StartObject) throw new JsonReaderException(String.Format("Expected JsonToken.StartObject, got {0}", reader.TokenType.ToString()));
            reader.Read(); // Burn the StartObject token
            do
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string value = (string)reader.Value;
                    PropertyInfo prop = _modelManager.GetPropertyForJsonKey(objectType, value);
                    // If the model object has a non-standard Id property, but the "id" key is being used...
                    if (prop == null && value == "id") prop = _modelManager.GetIdProperty(objectType);

                    if (value == "links")
                    {
                        reader.Read(); // burn the PropertyName token
                        //TODO: linked resources (Done??)
                        DeserializeLinkedResources(retval, readStream, reader, serializer);
                    }
                    else if (prop != null)
                    {
                        if (!prop.PropertyType.CanWriteAsJsonApiAttribute())
                        {
                            reader.Read(); // burn the PropertyName token
                            //TODO: Embedded would be dropped here!
                            continue; // These aren't supposed to be here, they're supposed to be in "links"!
                        }

                        object propVal;
                        Type enumType;
                        if (prop.PropertyType == typeof(string) &&
                            prop.GetCustomAttributes().Any(attr => attr is SerializeStringAsRawJsonAttribute))
                        {
                            reader.Read();
                            if (reader.TokenType == JsonToken.Null)
                            {
                                propVal = null;
                            }
                            else
                            {
                                var token = JToken.Load(reader);
                                var rawPropVal = token.ToString();
                                propVal = JsonHelpers.MinifyJson(rawPropVal);
                            }
                        }
                        else if (prop.PropertyType.IsGenericType &&
                                 prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                                 (enumType = prop.PropertyType.GetGenericArguments()[0]).IsEnum)
                        {
                            // Nullable enums need special handling
                            reader.Read();
                            propVal = reader.TokenType == JsonToken.Null
                                ? null
                                : Enum.Parse(enumType, reader.Value.ToString());
                        }
                        else if (prop.PropertyType == typeof(DateTimeOffset) ||
                                 prop.PropertyType == typeof(DateTimeOffset?))
                        {
                            // For some reason 
                            reader.ReadAsString();
                            propVal = reader.TokenType == JsonToken.Null
                                ? (object) null
                                : DateTimeOffset.Parse(reader.Value.ToString());
                        }
                        else
                        {
                            reader.Read();
                            propVal = DeserializeAttribute(prop.PropertyType, reader);
                        }


                        prop.SetValue(retval, propVal, null);

                        // Tell the MetadataManager that we deserialized this property
                        MetadataManager.Instance.SetMetaForProperty(retval, prop, true);

                        // pop the value off the reader, so we catch the EndObject token below!.
                        reader.Read();
                    }
                    else
                    {
                        // Unexpected/unknown property--Skip the propertyname and its value
                        reader.Skip();
                        if (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.StartObject) reader.Skip();
                        else reader.Read();
                    }
                }

            } while (reader.TokenType != JsonToken.EndObject);
            reader.Read(); // burn the EndObject token before returning back up the call stack

            /*
            // Suss out all the relationship members, and which ones have what cardinality...
            IEnumerable<PropertyInfo> relations = (
                from prop in objectType.GetProperties()
                where !CanWriteTypeAsJsonApiAttribute(prop.PropertyType)
                && prop.GetCustomAttributes(true).Any(attribute => attribute is System.Runtime.Serialization.DataMemberAttribute)
                select prop
                );
            IEnumerable<PropertyInfo> hasManys = relations.Where(prop => typeof(IEnumerable<object>).IsAssignableFrom(prop.PropertyType));
            IEnumerable<PropertyInfo> belongsTos = relations.Where(prop => !typeof(IEnumerable<object>).IsAssignableFrom(prop.PropertyType));

            JObject links = (JObject)jo["links"];

            // Lets deal with belongsTos first, that should be simpler...
            foreach (PropertyInfo prop in belongsTos)
            {
                if (links == null) break; // Well, apparently we don't have any data for the relationships!

                string btId = (string)links[_modelManager.GetJsonKeyForProperty(prop)];
                if (btId == null)
                {
                    prop.SetValue(retval, null, null); // Important that we set--the value may have been cleared!
                    continue; // breaking early!
                }
                Type relType = prop.PropertyType;
                //if (typeof(EntityObject).IsAssignableFrom(relType))
                if (resolver.CanIncludeTypeAsObject(relType))
                {
                    prop.SetValue(retval, resolver.GetById(relType, btId), null);
                    //throw new ApplicationException(String.Format("Could not assign BelongsTo property \"{0}\" on object of type {1} by ID {2} because no object of type {3} could be retrieved by that ID.", prop.Name, objectType, btId, prop.PropertyType));
                }
            }
             */

            return retval;
        }

        private void DeserializeLinkedResources(object obj, Stream readStream, JsonReader reader, JsonSerializer serializer)
        {
            //reader.Read();
            if (reader.TokenType != JsonToken.StartObject) throw new JsonSerializationException("'links' property is not an object!");

            Type objectType = obj.GetType();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string value = (string)reader.Value;
                    reader.Read(); // burn the PropertyName token
                    PropertyInfo prop = _modelManager.GetPropertyForJsonKey(objectType, value);
                    if (prop != null && !prop.PropertyType.CanWriteAsJsonApiAttribute())
                    {
                        //FIXME: We're really assuming they're ICollections...but testing for that doesn't work for some reason. Break prone!
                        if (prop.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)) && prop.PropertyType.IsGenericType)
                        {
                            // Is a hasMany

                            //TODO: At present only supports an array of string IDs!
                            JArray ids = JArray.Load(reader);

                            Type relType;
                            if (prop.PropertyType.IsGenericType)
                            {
                                relType = prop.PropertyType.GetGenericArguments()[0];
                            }
                            else
                            {
                                // Must be an array at this point, right??
                                relType = prop.PropertyType.GetElementType();
                            }

                            IEnumerable<Object> hmrel = (IEnumerable<Object>)prop.GetValue(obj, null);
                            if (hmrel == null)
                            {
                                // Hmm...now we have to create an object that fits this property. This could get messy...
                                if (!prop.PropertyType.IsInterface && !prop.PropertyType.IsAbstract)
                                {
                                    // Whew...okay, just instantiate one of these...
                                    hmrel = (IEnumerable<Object>)Activator.CreateInstance(prop.PropertyType);
                                }
                                else
                                {
                                    // Ugh...now we're really in trouble...hopefully one of these will work:
                                    if (prop.PropertyType.IsGenericType)
                                    {
                                        if (prop.PropertyType.IsAssignableFrom(typeof(List<>).MakeGenericType(relType)))
                                        {
                                            hmrel = (IEnumerable<Object>)Activator.CreateInstance(typeof(List<>).MakeGenericType(relType));
                                        }
                                        else if (prop.PropertyType.IsAssignableFrom(typeof(HashSet<>).MakeGenericType(relType)))
                                        {
                                            hmrel = (IEnumerable<Object>)Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(relType));
                                        }
                                        //TODO: Other likely candidates??
                                        else
                                        {
                                            // punt!
                                            throw new JsonReaderException(String.Format("Could not create empty container for relationship property {0}!", prop));
                                        }
                                    }
                                    else
                                    {
                                        // erm...Array??!?
                                        hmrel = (IEnumerable<Object>)Array.CreateInstance(relType, ids.Count);
                                    }
                                }
                            }

                            // We're having problems with how to generalize/cast/generic-ize this code, so for the time
                            // being we'll brute-force it in super-dynamic language style...
                            Type hmtype = hmrel.GetType();
                            MethodInfo add = hmtype.GetMethod("Add");

                            foreach (JToken token in ids)
                            {
                                //((ICollection<object>)prop.GetValue(obj, null)).Add(Activator.CreateInstance(relType));
                                object dummyobj = Activator.CreateInstance(relType);
                                add.Invoke(hmrel, new object[] { this.GetById(relType, token.ToObject<string>()) });
                            }
                        }
                        else
                        {
                            // Is a belongsTo

                            //TODO: At present only supports a string ID!
                            Type relType = prop.PropertyType;

                            prop.SetValue(obj, GetById(relType, (string)reader.Value));
                        }

                        // Tell the MetadataManager that we deserialized this property
                        MetadataManager.Instance.SetMetaForProperty(obj, prop, true);
                    }
                    else
                        reader.Skip();
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    // Burn the EndObject token and get set to send back to the parent method in the call stack.
                    reader.Read();
                    break;
                }
                else
                    reader.Skip();
            }
        }

        private object DeserializeAttribute(Type type, JsonReader reader)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            var token = JToken.Load(reader);
            return token.ToObject(type);
        }

        #endregion

        private Type GetSingleType(Type type)//dynamic value = null)
        {
            return _modelManager.IsSerializedAsMany(type) ? _modelManager.GetElementType(type) : type;
        }

        protected object GetById(Type type, string id)
        {
            // Only good for creating dummy relationship objects...
            object retval = Activator.CreateInstance(type);
            PropertyInfo idprop = _modelManager.GetIdProperty(type);
            idprop.SetValue(retval, System.Convert.ChangeType(id, idprop.PropertyType));
            return retval;
        }

        protected string GetValueForIdProperty(PropertyInfo idprop, object obj)
        {
            if (idprop != null)
            {
                if (idprop.PropertyType == typeof(string))
                    return (string)idprop.GetValue(obj, null);
                if (idprop.PropertyType == typeof(Guid))
                    return idprop.GetValue(obj).ToString();
                if (idprop.PropertyType == typeof(int))
                    return ((int)idprop.GetValue(obj, null)).ToString();
            }
            return "NOIDCOMPUTABLE!";
        }

        protected string GetIdFor(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo idprop = _modelManager.GetIdProperty(type);
            return GetValueForIdProperty(idprop, obj);
        }

        private void WriteIdsArrayJson(Newtonsoft.Json.JsonWriter writer, IEnumerable<object> value, Newtonsoft.Json.JsonSerializer serializer)
        {
            IEnumerator<Object> collectionEnumerator = (value as IEnumerable<object>).GetEnumerator();
            writer.WriteStartArray();
            while (collectionEnumerator.MoveNext())
            {
                var serializable = collectionEnumerator.Current;
                writer.WriteValue(this.GetIdFor(serializable));
            }
            writer.WriteEndArray();
        }

    }
}
