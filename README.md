JSONAPI.NET
===========
[![jsonapi MyGet Build Status](https://www.myget.org/BuildSource/Badge/jsonapi?identifier=caf48269-c15b-4850-a29e-b41a23d9854d)](https://www.myget.org/)

News
----

The NuGet packages are out!
* [JSONAPI](https://www.nuget.org/packages/JSONAPI/)
* [JSONAPI.EntityFramework](https://www.nuget.org/packages/JSONAPI.EntityFramework/)

JSON API Compliance
----------------------

The master branch is roughly compatible with the RC3 version of JSON API. The major missing feature is inclusion of related resources. Many changes made to the spec since RC3 are not yet available in this library. Full 1.0 compliance is planned, so stay tuned!

What is JSONAPI.NET?
====================

JSONAPI.NET is a set of utility classes that aim to make it possible to implement [JSON API spec](http://jsonapi.org/) compliant RESTful web services quickly and easily using ASP.NET MVC WebAPI. In practice, the primary target is to build backend web services that work with the Ember Data library and [Ember.js](http://emberjs.com). Since the JSON API spec project originated from the Ember Data and Active Model Serializers project, Ember Data's included RESTAdapter is very close to JSON API spec compliant--but not quite. You will need to use the separately available [JSON API Adapter](https://github.com/kurko/ember-json-api) to connect with Ember Data, and it is currently a primary goal of this project to work well with this adapter (coverage of the whole JSON API spec is far from complete).

How do I use it?
----------------

At its most basic level, JSONAPI includes a Json.NET `JsonMediaTypeFormatter`, which can be used to transform WebAPI JSON output into JSON API spec compliant messages. You can use the media formatter by adding an instance of it to the Json.NET Formatters collection, probably in your WebApiConfig class, like this:

```C#
public static void Register(HttpConfiguration config)
{
  //...
  JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
  formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
  GlobalConfiguration.Configuration.Formatters.Add(formatter);
  //...
}
```

In connection with the media formatter, a `PluralizationService` is required to translate between singular and plural object names, since the spec (usually?) uses plural class/type names, and your class names are likely singular. The `JSONAPI.Core.PluralizationService` is provided as a horribly naive sample implementation (it simply adds or strips "s" from the end of the class name), but at least lets you manually add explicit mappings with the `AddMapping(string singular, string plural)` method. 

> :information_source: HINT: the `JSONAPI.EntityFramework` library includes a `PluralizationService` that uses EntityFramework's much more sophisticated inflector!

That will configure a working formatter. Note that the formatter (by default) will only handle requests having an `Accept` or `Content-type` header of `application/vnd.api+json`, the media type defined for the JSON API spec.

Essentially, that's it! At this point your WebAPI methods should be able to produce and consume JSON that conforms to (a subset of) the JSON API spec! There is, of course, much more that you can configure.

Serializing Relationships
-------------------------

One of the benefits of the JSON API spec is that it provides several ways to [serialize relationships](http://jsonapi.org/format/#document-structure-resource-relationships). This gives you a lot of flexibility to improve your API performance for both the server and client: If related objects are expensive to retrieve or compute, you can represent the relationship as a URL to avoid that cost if the client never accesses it. If it is not expensive, or you know the client will need them, you can bundle them in the same response to reduce the number of network requests. JSONAPI.NET provides you three attributes you can use to decorate your models to specify how a relationship property will be serialized.

* `SerializeAs`: Decorate a property with this attribute to specify that the related objects should be serialized as ids, a link, or with the serialized object(s) embedded within the parent object (embedding is NOT part of the spec, but is supported by Ember Data!)
* `IncludeInPayload`: Decorate a property with this attribute to cause the related objects to be included with the parent object in the same response message. This is called a Compound Document in the spec. This only makes sense to use with `[SerializeAs(SerializeAsOptions.Ids)]`.
* `LinkTemplate`: If you use `[SerializeAs(SerializeAsOptions.Link)]`, you must also use this attribute to specify the format of the link sent to the client. Two template parameters are supported, `{0}` will be replaced with the Id(s) of the related object, and `{1}` will be replaced with the Id of the parent object.

> :information_source: As a side note, the RelationAggregator class handles the work of pulling any included related objects into the document--which may be a recursive process: for example, if an object has a relationship of its own type and that relationship property is annotated to be included, then it will recursively include all related objects of the same type! However, those related objects will all be included in a flat array for that type, according to the spec.

# Great! But what's all this other stuff?


JSONAPI.Net intends to be so much more than just a media formatter. It provides subclasses of WebAPI's `ApiController` that automate some of the boilerplate that goes into building a web service, especially one that can comply with the JSON API spec. While `JSONAPI.ApiController` doesn't do much, it can be subclassed to make default actions for the HTTP methods that WebAPI supports.

A `JSONAPI.IMaterializer` object can be added to that `ApiController` to broker between the WebAPI layer and your persistence layer. If you create an implementation of `IMaterializer`, you can begin to use the base `ApiController`'s handler methods, such as the Get() handler--which already handles the JSON API spec for requesting multiple objects with a list of comma-separated Ids!

# Didn't I read something about using Entity Framework?

The classes in the `JSONAPI.EntityFramework` namespace take great advantage of the patterns set out in the `JSONAPI` namespace. The `EntityFrameworkMaterializer` is an `IMaterializer` that can operate with your own `DbContext` class to retrieve objects by Id/Primary Key, and can retrieve and update existing objects from your context in a way that Entity Framework expects for change tracking…that means, in theory, you can use the provided `JSONAPI.EntityFramework.ApiController` base class to handle GET, PUT, POST, and DELETE without writing any additional code! You will still almost certainly subclass `ApiController` to implement your business logic, but that means you only have to worry about your business logic--not implementing the JSON API spec or messing with your persistence layer.
