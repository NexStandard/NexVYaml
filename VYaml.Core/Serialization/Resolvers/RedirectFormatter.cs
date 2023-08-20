using System;
using System.Collections.Generic;
using System.Text;
using VYaml.Emitter;
using VYaml.Parser;

namespace VYaml.Serialization.Resolvers
{
    internal class RedirectFormatter<T> : IYamlFormatter<T>
    {
        public T Deserialize(ref YamlParser parser, YamlDeserializationContext context)
        {
            throw new NotImplementedException();
        }

        public void Serialize(ref Utf8YamlEmitter emitter, T value, YamlSerializationContext context)
        {
            Type type = typeof(T);
            if(type.IsInterface)
            {
                NexYamlSerializerRegistry.Default.Get
            }
        }
    }
}
