using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TinyGardenGame {
  public static class SpriteFontHelper {
    private static int SpriteFontCharWidth = 8;
    private static int SpriteFontCharHeight = 8;
    private static int SpriteFontWidthInChars = 12;

    // There are more chars, but they are symbols I don't anticipate needing right now
    private static string Chars =
        "~1234567890-" +
        "+!@#$%^&*()_" +
        "={}[]|\\:;\"'<" +
        ",>.?/ABCDEFG" +
        "HIJKLMNOPQRS" +
        "TUVWXYZabcde" +
        "fghijklmnopq" +
        "rstuvwxyz";


    public static SpriteFont BuildSpriteFont(Texture2D texture) {
      var chars = new List<char>(Chars.ToCharArray());
      var glyphBounds = new List<Rectangle>();
      for (int i = 0; i < chars.Count; i++) {
        int x = i / SpriteFontWidthInChars /* intentional decimal truncation */;
        int y = i % SpriteFontWidthInChars;
        glyphBounds.Add(new Rectangle(
            x * SpriteFontCharWidth,
            y * SpriteFontCharHeight,
            SpriteFontCharWidth,
            SpriteFontCharHeight));
      }
      var kerning = new List<Vector3>(Enumerable.Repeat(new Vector3(0, 0, 0), chars.Count));
      
      // Characters must be sorted by their int value for some ungodly reason
      glyphBounds = glyphBounds
          .Select((glyph, i) => new { i, glyph })
          .OrderBy(element => (int) chars[element.i])
          .Select(element => element.glyph)
          .ToList();
      chars.Sort();

      return new SpriteFont(
          texture,
          glyphBounds,
          glyphBounds /* no cropping */,
          chars,
          0 /* lineSpacing */,
          0 /* spacing */,
          kerning /* no kerning */,
          '?' /* defaultCharacter */);
    }
  }
}