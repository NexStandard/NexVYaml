using System;
using System.Collections.Generic;
using System.Text;
using VYaml.Serialization;

namespace VYaml
{
    public class NexYamlSerializerRegistry : IYamlFormatterResolver
    {
        public static NexYamlSerializerRegistry Default = new NexYamlSerializerRegistry();
        Dictionary<Type, IYamlFormatter> TypeFormatter = new Dictionary<Type, IYamlFormatter>()
        {
            // Primitive
            { typeof(Int16), Int16Formatter.Instance },
            { typeof(Int32), Int32Formatter.Instance },
            { typeof(Int64), Int64Formatter.Instance },
            { typeof(UInt16), UInt16Formatter.Instance },
            { typeof(UInt32), UInt32Formatter.Instance },
            { typeof(UInt64), UInt64Formatter.Instance },
            { typeof(Single), Float32Formatter.Instance },
            { typeof(Double), Float64Formatter.Instance },
            { typeof(bool), BooleanFormatter.Instance },
            { typeof(byte), ByteFormatter.Instance },
            { typeof(sbyte), SByteFormatter.Instance },
            { typeof(DateTime), DateTimeFormatter.Instance },
            { typeof(char), CharFormatter.Instance },
            { typeof(byte[]), ByteArrayFormatter.Instance },

            // Nullable Primitive
            { typeof(Int16?), NullableInt16Formatter.Instance },
            { typeof(Int32?), NullableInt32Formatter.Instance },
            { typeof(Int64?), NullableInt64Formatter.Instance },
            { typeof(UInt16?), NullableUInt16Formatter.Instance },
            { typeof(UInt32?), NullableUInt32Formatter.Instance },
            { typeof(UInt64?), NullableUInt64Formatter.Instance },
            { typeof(Single?), NullableFloat32Formatter.Instance },
            { typeof(Double?), NullableFloat64Formatter.Instance },
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
            { typeof(List<Int16>), new ListFormatter<Int16>() },
            { typeof(List<Int32>), new ListFormatter<Int32>() },
            { typeof(List<Int64>), new ListFormatter<Int64>() },
            { typeof(List<UInt16>), new ListFormatter<UInt16>() },
            { typeof(List<UInt32>), new ListFormatter<UInt32>() },
            { typeof(List<UInt64>), new ListFormatter<UInt64>() },
            { typeof(List<Single>), new ListFormatter<Single>() },
            { typeof(List<Double>), new ListFormatter<Double>() },
            { typeof(List<Boolean>), new ListFormatter<Boolean>() },
            { typeof(List<byte>), new ListFormatter<byte>() },
            { typeof(List<SByte>), new ListFormatter<SByte>() },
            { typeof(List<DateTime>), new ListFormatter<DateTime>() },
            { typeof(List<Char>), new ListFormatter<Char>() },
            { typeof(List<string>), new ListFormatter<string>() },

            { typeof(object[]), new ArrayFormatter<object>() },
            { typeof(List<object>), new ListFormatter<object>() },
        };

        public IYamlFormatter<T>? GetFormatter<T>()
        {
            if(TypeFormatter.ContainsKey(typeof(T)))
            {
                return (IYamlFormatter<T>)TypeFormatter[typeof(T)];
            }
            return null;
        }
        public void RegisterFormatter<T>(IYamlFormatter<T> formatter)
        {
            Type keyType = typeof(T);
            TypeFormatter[keyType] = formatter;
            
        }

    }
}
