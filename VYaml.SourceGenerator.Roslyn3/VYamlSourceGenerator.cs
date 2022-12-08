﻿using Microsoft.CodeAnalysis;

namespace VYaml.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class VYamlSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxContextReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        try
        {
            var moduleName = context.Compilation.SourceModule.Name;
            if (moduleName.StartsWith("UnityEngine.")) return;
            if (moduleName.StartsWith("UnityEditor.")) return;
            if (moduleName.StartsWith("Unity.")) return;

            var references = ReferenceSymbols.Create(context.Compilation);
            if (references is null) return;

            var codeWriter = new CodeWriter();
            if (context.SyntaxContextReceiver! is not SyntaxContextReceiver syntaxCollector) return;

            var l = new List<TypeMeta>();
            foreach (var workItem in syntaxCollector.GetWorkItems())
            {
                var typeMeta = workItem.Analyze(in context, references);
                if (typeMeta is null) continue;

                Emit(typeMeta, codeWriter, references, in context);
                codeWriter.Clear();
                l.Add(typeMeta);
            }
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnexpectedErrorDescriptor,
                Location.None,
                ex.ToString().Replace(Environment.NewLine, " ")));
        }
    }

    static void Emit(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        ReferenceSymbols references,
        in GeneratorExecutionContext context)
    {
        try
        {
            // verify is partial
            if (!typeMeta.IsPartial())
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.MustBePartial,
                    typeMeta.Syntax.Identifier.GetLocation(),
                    typeMeta.Symbol.Name));
                return;
            }

            // nested is not allowed
            if (typeMeta.IsNested())
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticDescriptors.NestedNotAllow,
                    typeMeta.Syntax.Identifier.GetLocation(),
                    typeMeta.Symbol.Name));
                return;
            }

            codeWriter.AppendLine("// <auto-generated />");
            codeWriter.AppendLine("#nullable enable");
            codeWriter.AppendLine("#pragma warning disable CS0162 // Unreachable code");
            codeWriter.AppendLine("#pragma warning disable CS0219 // Variable assigned but never used");
            codeWriter.AppendLine("#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.");
            codeWriter.AppendLine("#pragma warning disable CS8601 // Possible null reference assignment");
            codeWriter.AppendLine("#pragma warning disable CS8602 // Possible null return");
            codeWriter.AppendLine("#pragma warning disable CS8604 // Possible null reference argument for parameter");
            codeWriter.AppendLine("#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method");
            codeWriter.AppendLine();
            codeWriter.AppendLine("using System;");
            codeWriter.AppendLine("using VYaml.Parser;");
            codeWriter.AppendLine("using VYaml.Serialization;");
            codeWriter.AppendLine();

            var ns = typeMeta.Symbol.ContainingNamespace;
            if (!ns.IsGlobalNamespace)
            {
                codeWriter.AppendLine($"namespace {ns}");
                codeWriter.BeginBlock();
            }

            var fullType = typeMeta.FullTypeName
                .Replace("global::", "")
                .Replace("<", "_")
                .Replace(">", "_");

            var typeDecralationKeyword = (typeMeta.Symbol.IsRecord, typeMeta.Symbol.IsValueType) switch
            {
                (true, true) => "record struct",
                (true, false) => "record",
                (false, true) => "struct",
                (false, false) => "class",
            };

            using (codeWriter.BeginBlockScope($"partial {typeDecralationKeyword} {typeMeta.TypeName}"))
            {
                EmitRegisterMethod(typeMeta, codeWriter, in context);
                EmitFormatter(typeMeta, codeWriter, in context);
            }

            if (!ns.IsGlobalNamespace)
            {
                codeWriter.EndBlock();
            }

            codeWriter.AppendLine("#pragma warning restore CS0162 // Unreachable code");
            codeWriter.AppendLine("#pragma warning restore CS0219 // Variable assigned but never used");
            codeWriter.AppendLine("#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.");
            codeWriter.AppendLine("#pragma warning restore CS8601 // Possible null reference assignment");
            codeWriter.AppendLine("#pragma warning restore CS8602 // Possible null return");
            codeWriter.AppendLine("#pragma warning restore CS8604 // Possible null reference argument for parameter");
            codeWriter.AppendLine("#pragma warning restore CS8631 // The type cannot be used as type parameter in the generic type or method");

            context.AddSource($"{fullType}.YamlFormatter.g.cs", codeWriter.ToString());
        }
        catch (Exception ex)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticDescriptors.UnexpectedErrorDescriptor,
                Location.None,
                ex.ToString().Replace(Environment.NewLine, " ")));
        }
    }

    static void EmitRegisterMethod(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        in GeneratorExecutionContext context)
    {
        codeWriter.AppendLine("[Preserve]");
        using var _ = codeWriter.BeginBlockScope("public static void __RegisterVYamlFormatter()");
        codeWriter.AppendLine($"global::VYaml.Serialization.GeneratedResolver.Register(new {typeMeta.TypeName}GeneratedFormatter());");
    }

    static void EmitFormatter(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        in GeneratorExecutionContext context)
    {
        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";

        codeWriter.AppendLine("[Preserve]");
        using var _ = codeWriter.BeginBlockScope($"public class {typeMeta.TypeName}GeneratedFormatter : IYamlFormatter<{returnType}>");

        EmitDeserializeMethod(typeMeta, codeWriter, in context);
    }

    static void EmitDeserializeMethod(
        TypeMeta typeMeta,
        CodeWriter codeWriter,
        in GeneratorExecutionContext context)
    {
        var memberMetas = typeMeta.GetSerializeMembers();

        foreach (var memberMeta in memberMetas)
        {
            codeWriter.Append($"static readonly byte[] {memberMeta.Name}KeyUtf8Bytes = ");
            codeWriter.AppendByteArrayString(memberMeta.KeyNameUtf8Bytes);
            codeWriter.AppendLine($"; // {memberMeta.KeyName}", false);
            codeWriter.AppendLine();
        }

        var returnType = typeMeta.Symbol.IsValueType
            ? typeMeta.FullTypeName
            : $"{typeMeta.FullTypeName}?";
        codeWriter.AppendLine("[Preserve]");
        using var methodScope = codeWriter.BeginBlockScope(
            $"public {returnType} Deserialize(ref YamlParser parser, YamlDeserializationContext context)");

        using (codeWriter.BeginBlockScope("if (parser.IsNullScalar())"))
        {
            codeWriter.AppendLine("parser.Read();");
            codeWriter.AppendLine("return default;");
        }

        if (memberMetas.Length <= 0)
        {
            codeWriter.AppendLine("parser.SkipCurrentNode();");
            codeWriter.AppendLine($"return new {typeMeta.TypeName}();");
            return;
        }

        codeWriter.AppendLine("parser.ReadWithVerify(ParseEventType.MappingStart);");
        codeWriter.AppendLine();
        foreach (var memberMeta in memberMetas)
        {
            codeWriter.AppendLine($"var __{memberMeta.Name}__ = default({memberMeta.FullTypeName});");
        }

        using (codeWriter.BeginBlockScope("while (!parser.End && parser.CurrentEventType != ParseEventType.MappingEnd)"))
        {
            using (codeWriter.BeginBlockScope("if (parser.CurrentEventType != ParseEventType.Scalar)"))
            {
                codeWriter.AppendLine("throw new YamlSerializerException(parser.CurrentMark, \"Custom type deserialization supports only string key\");");
            }
            codeWriter.AppendLine();
            using (codeWriter.BeginBlockScope("if (!parser.TryGetScalarAsSpan(out var key))"))
            {
                codeWriter.AppendLine("throw new YamlSerializerException(parser.CurrentMark, \"Custom type deserialization supports only string key\");");
            }
            codeWriter.AppendLine();
            using (codeWriter.BeginBlockScope("switch (key.Length)"))
            {
                var membersByNameLength = memberMetas.GroupBy(x => x.KeyNameUtf8Bytes.Length);
                foreach (var group in membersByNameLength)
                {
                    using (codeWriter.BeginIndentScope($"case {group.Key}:"))
                    {
                        var branching = "if";
                        foreach (var memberMeta in group)
                        {
                            using (codeWriter.BeginBlockScope($"{branching} (key.SequenceEqual({memberMeta.Name}KeyUtf8Bytes))"))
                            {
                                codeWriter.AppendLine("parser.Read(); // skip key");
                                codeWriter.AppendLine(
                                    $"__{memberMeta.Name}__ = context.DeserializeWithAlias<{memberMeta.FullTypeName}>(ref parser);");
                            }
                            branching = "else if";
                        }
                        using (codeWriter.BeginBlockScope("else"))
                        {
                            codeWriter.AppendLine("parser.Read(); // skip key");
                            codeWriter.AppendLine("parser.SkipCurrentNode(); // skip value");
                        }
                        codeWriter.AppendLine("continue;");
                    }
                }

                using (codeWriter.BeginIndentScope("default:"))
                {
                    codeWriter.AppendLine("parser.Read(); // skip key");
                    codeWriter.AppendLine("parser.SkipCurrentNode(); // skip value");
                    codeWriter.AppendLine("continue;");
                }
            }
        }
        codeWriter.AppendLine("parser.ReadWithVerify(ParseEventType.MappingEnd);");
        using (codeWriter.BeginBlockScope($"return new {typeMeta.TypeName}"))
        {
            foreach (var memberMeta in memberMetas)
            {
                codeWriter.AppendLine($"{memberMeta.Name} = __{memberMeta.Name}__,");
            }
        }
        codeWriter.AppendLine(";");
    }
}
