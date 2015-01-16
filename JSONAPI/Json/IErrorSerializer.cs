using Newtonsoft.Json;
using System;
using System.IO;

namespace JSONAPI.Json
{
    internal interface IErrorSerializer
    {
        bool CanSerialize(Type type);
        void SerializeError(object error, Stream writeStream, JsonWriter writer, JsonSerializer serializer);
    }
}
