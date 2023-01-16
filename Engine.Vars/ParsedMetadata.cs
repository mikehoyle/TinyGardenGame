using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Tomlyn;
using Tomlyn.Model;

namespace Engine.Vars; 

/// <summary>
/// Handles the parsing and most of the validation of input files.
/// </summary>
internal class ParsedMetadata : IEquatable<ParsedMetadata> {
  public const string RefPrefix = "Ref-";
  private const string NameLabel = "Name";
  private ParsedMetadata() { }

  public string TableName { get; private set; } =  "";

  public string Namespace { get; set; } = "";
  
  public HashSet<string> Names { get; } = new();
  // Some duplication, oh well -- Maps names to their parsed Toml table
  public Dictionary<string, TomlTable> NameToTableMap { get; } = new();

  // Maps field name to string representation of type
  public Dictionary<string, FieldType> Fields { get; } = new();
  
  // Maps referred field to all attempted references, for validation.
  public Dictionary<string, HashSet<string>> RefFields { get; } = new();

  // Maps ref field names to the table to which they refer
  // TODO(P0) fill and use this
  public Dictionary<string, string> RefFieldNames { get; } = new();

  // Maps object name to field names and type
  public Dictionary<string, Dictionary<string, FieldType>> InnerObjects { get; } = new();
  
  // Maps array fields to the type they contain
  public Dictionary<string, FieldType> InnerArrays { get; } = new();

  public static ParsedMetadata ParseTomlFile(AdditionalText file) {
    if (Path.GetExtension(file.Path) != ".toml") {
      // Only support Toml files, and avoid breaking if something else is provided
      throw new VarsGenerationException($"Cannot generate sources for non-toml files: {file.Path}");
    }
    if (string.IsNullOrEmpty(file.GetText()?.ToString())) {
      throw new VarsGenerationException($"No content for provided file {file.Path}");
    }

    var result = new ParsedMetadata();
    var tomlModel = Toml.ToModel(file.GetText()?.ToString() ?? "");

    if (tomlModel.Keys.Count > 1) {
      throw new VarsGenerationException(
          $"Currently only support for one key per file (in file {file.Path})");
    }

    result.TableName = tomlModel.Keys.First();
    if (tomlModel[tomlModel.Keys.First()] is not TomlTableArray tomlTableArray) {
      throw new VarsGenerationException($"Type is not of a table array in {file.Path}");
    }
    
    foreach (var tomlTable in tomlTableArray) {
      if (tomlTable[NameLabel] is not string) {
        throw new VarsGenerationException(
            $"Item name not a valid string: {tomlTable[NameLabel]} in {file.Path}");
      }

      var name = (string)tomlTable[NameLabel];
      if (!result.Names.Add(name)) {
        throw new VarsGenerationException($"Duplicated name '{name}' in {file.Path}");
      }
      result.NameToTableMap[name] = tomlTable;
      foreach (var keyValuePair in tomlTable) {
        if (keyValuePair.Key.StartsWith(RefPrefix)) {
          if (keyValuePair.Value is not string value) {
            throw new VarsGenerationException(
                $"Ref value must be string: {keyValuePair.Key} in {file.Path}");
          }
          
          var refNameSplit = keyValuePair.Key.Split('-');
          if (refNameSplit.Length != 3) {
            throw new VarsGenerationException(
              $"Ref must contain target table and name (in {file.Path})");
          }
          var refTableName = refNameSplit[1];
          var refFieldName = refNameSplit[2];
          if (result.RefFields.TryGetValue(refTableName, out var list)) {
            list.Add(value);
          } else {
            result.RefFields.Add(refTableName, new HashSet<string> { value });
          }

          if (result.Fields.ContainsKey(refFieldName)) {
            throw new VarsGenerationException(
              $"Ref field can't have same name as normal field (in {file.Path})");
          }
          result.RefFieldNames[refFieldName] = refTableName;
        } else {
          var type = GetFieldType(keyValuePair.Value, keyValuePair.Key, result.TableName);
          if (result.Fields.TryGetValue(keyValuePair.Key, out var existingType)) {
            if (type != existingType) {
              throw new VarsGenerationException(
                  $"Cannot have two different types for the same field in entry [{name}]: " +
                  $"({type} and {existingType}), in table {result.TableName}");
            }
          } else {
            if (result.RefFieldNames.ContainsKey(keyValuePair.Key)) {
              throw new VarsGenerationException($"Cannot have field already used by ref field");
            }
            result.Fields.Add(keyValuePair.Key, type);  
          }

          if (type == FieldType.Array) {
            var innerArray = (TomlArray)keyValuePair.Value;
            if (innerArray.Count > 0) {
              var innerType = GetFieldType(
                  innerArray[0], keyValuePair.Key, result.TableName, false);
              if (result.InnerArrays.ContainsKey(keyValuePair.Key) &&
                  result.InnerArrays[keyValuePair.Key] != innerType) {
                throw new VarsGenerationException(
                    $"Arrays must all contain the same type across file, for field " +
                    $"({keyValuePair.Key}) in table {result.TableName}");
              }

              result.InnerArrays[keyValuePair.Key] = innerType;
            }
          }

          if (type == FieldType.Object) {
            if (!result.InnerObjects.ContainsKey(keyValuePair.Key)) {
              result.InnerObjects.Add(keyValuePair.Key, new());
            }

            var innerObject = result.InnerObjects[keyValuePair.Key];
            foreach (var innerTableEntry in (TomlTable)keyValuePair.Value) {
              var innerFieldType =
                  GetFieldType(innerTableEntry.Value, innerTableEntry.Key, result.TableName, false);
              if (innerObject.ContainsKey(innerTableEntry.Key) &&
                  innerObject[innerTableEntry.Key] != innerFieldType) {
                throw new VarsGenerationException(
                    $"Inconsistent types in field {keyValuePair.Key} in table {result.TableName}");
              }

              result.InnerObjects[keyValuePair.Key][innerTableEntry.Key] = innerFieldType;
            }
          }
        }
      }
    }

    return result;
  }

  public static FieldType GetFieldType(
      object? fieldValue, string field, string tableName, bool allowComplexFields = true) {
    switch (fieldValue) {
      case long x:
      case int y:
        return FieldType.Integer;
      case double x:
      case float y:
        return FieldType.Float;
      case string x:
        return FieldType.String;
      case TomlArray x:
        if (!allowComplexFields) {
          throw new VarsGenerationException(
              $"Cannot use complex (nested fields) for field {field} in table {tableName}");
        }
        if (x.Count > 1) {
          var innerType = GetFieldType(x[0], field, tableName, false);
          foreach (var arrayItem in x) {
            if (GetFieldType(arrayItem, field, tableName, false) != innerType) {
              throw new VarsGenerationException(
                  $"Arrays must contain all the same type " +
                  $"(in table {tableName} for field {field})");
            }
          }
        }

        return FieldType.Array;
      // TODO(P1): Handle objects gracefully
      case TomlTable x:
        if (!allowComplexFields) {
          throw new VarsGenerationException(
              $"Cannot use complex (nested fields) for field {field} in table {tableName}");
        }
        return FieldType.Object;
      default:
        throw new VarsGenerationException(
            $"Unknown type in table {tableName} for field {field}");
    }
  }

  private static string FormatClassName(string filePath) {
    var result = Path.GetFileNameWithoutExtension(filePath);
    result = string.Concat(result[0].ToString().ToUpper(), result.Substring(1));
    result = result.Replace("\\", "");
    result = result.Replace("_", "");
    result = result.Replace("-", "");
    return result;
  }

  /// <summary>
  /// Validates all refs are valid and internal types are consistent.
  /// </summary>
  public void Validate(Collection tableNames) {
    foreach (var refField in RefFields.Keys) {
      if (!tableNames.Contains(refField)) {
        throw new VarsGenerationException(
            $"No valid target for ref \"{refField}\" in table {TableName}");
      }

      var referencedTableMetadata = tableNames[refField];
      foreach (var referencedField in RefFields[refField]) {
        if (!referencedTableMetadata.Names.Contains(referencedField)) {
          throw new VarsGenerationException(
              $"Referenced entry [{referencedField}] in target table " +
              $"[{referencedTableMetadata.TableName}] not present " +
              $"(Reference found in table [{TableName}])");
        }
      }
    }
  }

  public enum FieldType {
    Integer,
    Float,
    String,
    Array,
    Object,
  }

  public class Collection : KeyedCollection<string, ParsedMetadata> {
    protected override string GetKeyForItem(ParsedMetadata item) {
      return item.TableName;
    }
  }

  public bool Equals(ParsedMetadata? other) {
    if (ReferenceEquals(null, other)) {
      return false;
    }

    if (ReferenceEquals(this, other)) {
      return true;
    }

    return TableName == other.TableName;
  }

  public override bool Equals(object? obj) {
    if (ReferenceEquals(null, obj)) {
      return false;
    }

    if (ReferenceEquals(this, obj)) {
      return true;
    }

    if (obj.GetType() != this.GetType()) {
      return false;
    }

    return Equals((ParsedMetadata)obj);
  }

  public override int GetHashCode() {
    return TableName.GetHashCode();
  }
}
