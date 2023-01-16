using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Engine.Vars;

[Generator]
public class VarsSourceGenerator : ISourceGenerator {
  private const string DefaultNamespace = "Engine.Vars.Generated";
  
  public void Initialize(GeneratorInitializationContext context) {}

  public void Execute(GeneratorExecutionContext context) {
    ParsedMetadata.Collection metadataMap = new();
    try {
      foreach (var file in context.AdditionalFiles) {
        var metadata = ParsedMetadata.ParseTomlFile(file);
        if (metadataMap.Contains(metadata.TableName)) {
          throw new VarsGenerationException(
              $"File contains already-used table name {metadata.TableName}, {file.Path}");
        }

        var fileNamespace = DefaultNamespace;
        if (context.AnalyzerConfigOptions.GetOptions(file)
            .TryGetValue("build_metadata.AdditionalFiles.GeneratedNamespace", out var customNamespace)) {
          fileNamespace = customNamespace;
        }

        metadata.Namespace = fileNamespace;
        metadataMap.Add(metadata);
      }

      foreach (var metadata in metadataMap) {
        metadata.Validate(metadataMap);
        ClassWriter.WriteClass(metadata, ref context);
      }
    } catch (VarsGenerationException e) {
      context.ReportDiagnostic(DiagnosticForMessage(e.Message, DiagnosticSeverity.Error));
    } catch (Exception e) {
      context.ReportDiagnostic(DiagnosticForMessage(
          "Fatal error generating source:", DiagnosticSeverity.Error));
      context.ReportDiagnostic(DiagnosticForMessage(e.Message, DiagnosticSeverity.Error)); 
      foreach (var stackTraceLine in e.StackTrace.Split('\n')) {
        context.ReportDiagnostic(DiagnosticForMessage(stackTraceLine, DiagnosticSeverity.Error)); 
      }
    }
  }

  public static Diagnostic DiagnosticForMessage(string message, DiagnosticSeverity severity) {
    return Diagnostic.Create(
        new DiagnosticDescriptor(
            id: "VARGEN001",
            title: message,
            messageFormat: message,
            category: "Engine.Vars.Generator",
            severity,
            isEnabledByDefault: true),
        Location.None);
  }
}