using ThisIsATestNamespace;

namespace Engine.Vars.Test;

public class ContentTest {
  [Test]
  public void SimpleContentTest() {
    Assert.That("", Is.EqualTo("")); // dummy
  }
}
