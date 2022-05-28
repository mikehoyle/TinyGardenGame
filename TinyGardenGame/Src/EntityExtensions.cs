using MonoGame.Extended.Entities;

namespace TinyGardenGame {
  public static class EntityExtensions {
    public static Entity AttachAnd<T>(this Entity entity, T component) where T : class {
      entity.Attach(component);
      return entity;
    }
  }
}