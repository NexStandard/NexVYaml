#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace VYaml.Serialization
{
    public interface IYamlFormatterResolver
    {
        IYamlFormatter<T>? GetFormatter<T>();
    }

    public static class YamlFormatterResolverExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IYamlFormatter<T> GetFormatterWithVerify<T>(this IYamlFormatterResolver resolver)
        {
            IYamlFormatter<T>? formatter;
            try
            {
                Type type = typeof(T);
                if (type.IsInterface ||type.IsAbstract)
                {
                    if(resolver is RedirectFormatter<T> redirector)
                    {
                        formatter = redirector;
                    }
                    else
                    {
                        formatter = new RedirectFormatter<T>();

                    }
                }
                else
                {
                    if(type.IsGenericType)
                    {
                        formatter = NexYamlSerializerRegistry.Default.GetGenericBufferedFormatter<T>();
                    }
                    else
                    {
                        formatter = NexYamlSerializerRegistry.Default.GetFormatter<T>();
                    }

                }
            }
            catch (TypeInitializationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException ?? ex).Throw();
                return default!; // not reachable
            }

            if (formatter != null)
            {
                return formatter;
            }
            Throw(typeof(T), resolver);
            return default!; // not reachable
        }

        static void Throw(Type t, IYamlFormatterResolver resolver)
        {
            throw new YamlSerializerException(t.FullName + $"{t} is not registered in resolver: {resolver.GetType()}");
        }
    }
}
