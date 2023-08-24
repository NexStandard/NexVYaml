using System;
using System.Collections.Generic;
using System.Text;
using VYaml.Parser;

namespace VYaml.Serialization
{
    public class NexYamlSerializerRegistry : IYamlFormatterResolver
    {
        public static NexYamlSerializerRegistry Default = new NexYamlSerializerRegistry();
        Dictionary<Type, IYamlFormatter> DefinedFormatters = new Dictionary<Type, IYamlFormatter>()
        {
            // Primitive
            { typeof(short), Int16Formatter.Instance },
            { typeof(int), Int32Formatter.Instance },
            { typeof(long), Int64Formatter.Instance },
            { typeof(ushort), UInt16Formatter.Instance },
            { typeof(uint), UInt32Formatter.Instance },
            { typeof(ulong), UInt64Formatter.Instance },
            { typeof(float), Float32Formatter.Instance },
            { typeof(double), Float64Formatter.Instance },
            { typeof(bool), BooleanFormatter.Instance },
            { typeof(byte), ByteFormatter.Instance },
            { typeof(sbyte), SByteFormatter.Instance },
            { typeof(DateTime), DateTimeFormatter.Instance },
            { typeof(char), CharFormatter.Instance },
            { typeof(byte[]), ByteArrayFormatter.Instance },

            // Nullable Primitive
            { typeof(short?), NullableInt16Formatter.Instance },
            { typeof(int?), NullableInt32Formatter.Instance },
            { typeof(long?), NullableInt64Formatter.Instance },
            { typeof(ushort?), NullableUInt16Formatter.Instance },
            { typeof(uint?), NullableUInt32Formatter.Instance },
            { typeof(ulong?), NullableUInt64Formatter.Instance },
            { typeof(float?), NullableFloat32Formatter.Instance },
            { typeof(double?), NullableFloat64Formatter.Instance },
            { typeof(bool?), NullableBooleanFormatter.Instance },
            { typeof(byte?), NullableByteFormatter.Instance },
            { typeof(sbyte?), NullableSByteFormatter.Instance },
            { typeof(DateTime?), NullableDateTimeFormatter.Instance },
            { typeof(char?), NullableCharFormatter.Instance },

            // StandardClassLibraryFormatter
            { typeof(string), NullableStringFormatter.Instance },
            { typeof(decimal), DecimalFormatter.Instance },
            { typeof(decimal?), new StaticNullableFormatter<decimal>(DecimalFormatter.Instance) },
            { typeof(TimeSpan), TimeSpanFormatter.Instance },
            { typeof(TimeSpan?), new StaticNullableFormatter<TimeSpan>(TimeSpanFormatter.Instance) },
            { typeof(DateTimeOffset), DateTimeOffsetFormatter.Instance },
            { typeof(DateTimeOffset?), new StaticNullableFormatter<DateTimeOffset>(DateTimeOffsetFormatter.Instance) },
            { typeof(Guid), GuidFormatter.Instance },
            { typeof(Guid?), new StaticNullableFormatter<Guid>(GuidFormatter.Instance) },
            { typeof(Uri), UriFormatter.Instance },

            // well known collections
            { typeof(List<short>), new ListFormatter<short>() },
            { typeof(List<int>), new ListFormatter<int>() },
            { typeof(List<long>), new ListFormatter<long>() },
            { typeof(List<ushort>), new ListFormatter<ushort>() },
            { typeof(List<uint>), new ListFormatter<uint>() },
            { typeof(List<ulong>), new ListFormatter<ulong>() },
            { typeof(List<float>), new ListFormatter<float>() },
            { typeof(List<double>), new ListFormatter<double>() },
            { typeof(List<bool>), new ListFormatter<bool>() },
            { typeof(List<byte>), new ListFormatter<byte>() },
            { typeof(List<sbyte>), new ListFormatter<sbyte>() },
            { typeof(List<DateTime>), new ListFormatter<DateTime>() },
            { typeof(List<char>), new ListFormatter<char>() },
            { typeof(List<string>), new ListFormatter<string>() },

            { typeof(object[]), new ArrayFormatter<object>() },
            { typeof(List<object>), new ListFormatter<object>() },
        };
        Dictionary<Type, Dictionary<Type, IYamlFormatter>> InterfaceBuffer = new();
        Dictionary<Type, Dictionary<Type, IYamlFormatter>> AbstractClassesBuffer = new();
        public IYamlFormatter<T>? GetFormatter<T>()
        {
            if (DefinedFormatters.ContainsKey(typeof(T)))
            {
                return (IYamlFormatter<T>)DefinedFormatters[typeof(T)];
            }

            return null;
        }
        public void RegisterFormatter<T>(IYamlFormatter<T> formatter)
        {
            Type keyType = typeof(T);
            DefinedFormatters[keyType] = formatter;
        }
        public void RegisterInterface<T>(IYamlFormatter<T> formatter, Type interfaceType)
        {
            Type keyType = typeof(T);
            if (!InterfaceBuffer.ContainsKey(interfaceType))
            {
                InterfaceBuffer.Add(interfaceType, new());
            }
            if (!InterfaceBuffer[interfaceType].ContainsKey(keyType))
            {
                InterfaceBuffer[interfaceType].Add(keyType, formatter);
            }
            else
            {
                InterfaceBuffer[interfaceType][keyType] = formatter;
            }
        }
        public void RegisterAbstractClass<T>(IYamlFormatter<T> formatter, Type interfaceType)
        {
            Type keyType = typeof(T);
            AbstractClassesBuffer[interfaceType][keyType] = formatter;
        }
        public IYamlFormatter FindInterfaceFormatter<T>(Tag tag)
        {
            Type type = Type.GetType(tag.Handle);
            return InterfaceBuffer[typeof(T)][type];
        }
        public IYamlFormatter FindInterfaceTypeBased<T>(Type target)
        {
            return InterfaceBuffer[typeof(T)][target];
        }
        public IYamlFormatter FindAbstractTypeBased<T>(Type target)
        {
            return InterfaceBuffer[typeof(T)][target];
        }

        public IYamlFormatter FindAbstractFormatter<T>(Tag tag)
        {
            Type type = Type.GetType(tag.Handle);
            return AbstractClassesBuffer[typeof(T)][type];
        }
    }
}
