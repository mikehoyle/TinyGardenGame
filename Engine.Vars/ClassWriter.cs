using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Tomlyn.Model;

namespace Engine.Vars; 

/// <summary>
/// Handles writing the .cs class files for the inputs 
/// </summary>
internal static class ClassWriter {
  private const string EnumIdName = "Type";
  private const string DictName = "Items";
  private const string InnerStructContainerName = "Inner";
  private const string IdFieldName = "Id";
  private const int TabWidth = 2;
  
  public static void WriteClass(ParsedMetadata metadata, ref GeneratorExecutionContext context) {
    var code = new StringBuilder();
    WriteClassPrefix(code, metadata.TableName, metadata.Namespace);
    WriteFields(code, metadata);
    WriteNamesEnum(code, metadata);
    WriteInnerStructs(code, metadata);
    WriteStaticDict(code, metadata);
    WriteClassPostfix(code);

    context.AddSource(
        $"{metadata.TableName}.g.cs", SourceText.From(code.ToString(), Encoding.UTF8));
  }

  private static void WriteNamesEnum(StringBuilder code, ParsedMetadata metadata) {
    code.AppendLine($"{Tab(2)}public enum {EnumIdName} {{");
    foreach (var id in metadata.Names) {
      code.AppendLine($"{Tab(3)}{id},");
    }
    code.AppendLine($"{Tab(2)}}}\n");
  }

  private static void WriteInnerStructs(StringBuilder code, ParsedMetadata metadata) {
    if (metadata.InnerObjects.Count < 1) {
      return;
    }

    code.AppendLine($"{Tab(2)}public partial struct {InnerStructContainerName} {{");
    foreach (var innerObject in metadata.InnerObjects) {
      code.AppendLine($"{Tab(3)}public partial struct {innerObject.Key} {{");
      foreach (var innerField in innerObject.Value) {
        code.AppendLine(
            $"{Tab(4)}public {GetTypeString(innerField.Key, innerField.Value, metadata)} " +
            $"{innerField.Key} {{ get; init; }}");
      }
      code.AppendLine($"{Tab(3)}}}\n");
    }
    code.AppendLine($"{Tab(2)}}}\n");
  }

  private static void WriteStaticDict(StringBuilder code, ParsedMetadata metadata) {
    code.AppendLine($"{Tab(2)}public static readonly " +
        $"Dictionary<{EnumIdName}, {metadata.TableName}> {DictName} = " +
        $"new Dictionary<{EnumIdName}, {metadata.TableName}>() {{");

    foreach (var name in metadata.Names) {
      var tomlTable = metadata.NameToTableMap[name];
      code.AppendLine($"{Tab(3)}{{");

      code.AppendLine($"{Tab(4)}{EnumIdName}.{name},");
      code.AppendLine($"{Tab(4)}new {metadata.TableName}() {{");

      code.AppendLine($"{Tab(5)}{IdFieldName} = {EnumIdName}.{name},");
      foreach (var field in metadata.Fields) {
        if (tomlTable.TryGetValue(field.Key, out var tomlTableValue)) {
          code.AppendLine($"{Tab(5)}{field.Key} = " +
              $"{FormatItem(field.Value, tomlTableValue, field.Key, metadata.TableName)},");
        }
      }

      foreach (var refFieldName in metadata.RefFieldNames.Keys) {
        var referencedTableName = metadata.RefFieldNames[refFieldName];
        var tomlFieldName = ParsedMetadata.RefPrefix + referencedTableName + "-" + refFieldName;
        if (tomlTable.TryGetValue(tomlFieldName, out var tomlTableValue)) {
          code.AppendLine($"{Tab(5)}{refFieldName} = " +
              $"{metadata.Namespace}.{referencedTableName}.{DictName}[" +
              $"{metadata.Namespace}.{referencedTableName}.{EnumIdName}.{(string)tomlTableValue}],");
        }
      }
      
      code.AppendLine($"{Tab(4)}}}");
      code.AppendLine($"{Tab(3)}}},");
    }
    
    code.AppendLine($"{Tab(2)}}};\n");
  }

  private static string FormatItem(
      ParsedMetadata.FieldType type, object item, string fieldName, string tableName) {
    switch (type) {
      case ParsedMetadata.FieldType.Float:
        return $"{Convert.ToSingle(item)}f";
      case ParsedMetadata.FieldType.Integer:
        return $"{Convert.ToInt32(item)}";
      case ParsedMetadata.FieldType.String:
        return $"\"{(string)item}\"";
      case ParsedMetadata.FieldType.Array:
        var array = (TomlArray)item;
        if (array.Count == 0) {
          return "new()";
        }
        var innerType = ParsedMetadata.GetFieldType(array[0], fieldName, tableName);
        var result = new StringBuilder("new() { ");
        foreach (var innerItem in array) {
          result.Append(FormatItem(innerType, innerItem!, fieldName, tableName));
          result.Append(", ");
        }
        result.Append("}");
        return result.ToString();
      case ParsedMetadata.FieldType.Object:
        // TODO(P0): properly handle object types
        var table = (TomlTable)item;
        var objectResult = new StringBuilder($"new {InnerStructContainerName}.{fieldName}() {{ ");
        foreach (var tableEntry in table) {
          var entryType = ParsedMetadata.GetFieldType(tableEntry.Value, fieldName, tableName);
          objectResult.Append(
              $"{tableEntry.Key} = " +
              $"{FormatItem(entryType, tableEntry.Value, fieldName, tableName)}, ");
        }
        objectResult.Append("}");
        return objectResult.ToString();
      default:
        throw new VarsGenerationException("Unknown type");
    }
  }

  private static void WriteFields(StringBuilder code, ParsedMetadata metadata) {
    code.AppendLine($"{Tab(2)}// Fields");
    code.AppendLine($"{Tab(2)}public {EnumIdName} " + 
          $"{IdFieldName} {{ get; init; }}\n");
    foreach (var field in metadata.Fields) {
      code.AppendLine($"{Tab(2)}public {GetTypeString(field.Key, field.Value, metadata)} " +
          $"{field.Key} {{ get; init; }}\n");
    }
    
    code.AppendLine($"{Tab(2)}// Reference fields");
    foreach (var field in metadata.RefFieldNames.Keys) {
      code.AppendLine(
        $"{Tab(2)}public {metadata.RefFieldNames[field]} {field} {{ get; init; }}\n");
    }

    code.AppendLine("");
  }

  private static void WriteClassPrefix(StringBuilder builder, string className, string fileNamespace) {
    builder.AppendLine("// <auto-generated/>\n");
    builder.AppendLine("using System;");
    builder.AppendLine("using System.Collections.Generic;");
    builder.AppendLine("");
    builder.AppendLine($"namespace {fileNamespace} {{");
    builder.AppendLine($"{Tab(1)}public partial class {className} {{");
  }

  private static void WriteClassPostfix(StringBuilder builder) {
    builder.AppendLine($"{Tab(1)}}}\n}}\n");
  }

  private static string Tab(int num) {
    return new StringBuilder().Append(' ', TabWidth * num).ToString();
  }
  
  private static string GetTypeString(
      string fieldName, ParsedMetadata.FieldType fieldType, ParsedMetadata metadata) {
    switch (fieldType) {
      case ParsedMetadata.FieldType.Integer:
        return "int";
      case ParsedMetadata.FieldType.Float:
        return "float";
      case ParsedMetadata.FieldType.String:
        return "string";
      case ParsedMetadata.FieldType.Array:
        return $"List<{GetTypeString(fieldName, metadata.InnerArrays[fieldName], metadata)}>";
      case ParsedMetadata.FieldType.Object:
        return $"{InnerStructContainerName}.{fieldName}";
      default:
        throw new VarsGenerationException("Unhandled type in generation");
    }
  }
}
