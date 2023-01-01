using System;

namespace Engine.Vars; 

/// <summary>
/// Reports exceptions that occur during generation.
/// </summary>
public class VarsGenerationException : Exception {
  public VarsGenerationException(string? message) : base(message) {}
  public VarsGenerationException(string? message, Exception inner) : base(message, inner) {}
}
