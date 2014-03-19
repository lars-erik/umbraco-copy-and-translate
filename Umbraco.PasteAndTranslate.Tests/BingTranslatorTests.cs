using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Umbraco.PasteAndTranslate.Tests
{
    [TestFixture]
    public class BingTranslatorTests
    {
        [Test]
        public void Translate_HelloWorld_Returns_HalloVerden()
        {
            var translator = new BingTranslator();
            var result = translator.Translate("Hello world!", "en", "no");
            Assert.AreEqual("Hallo, verden!", result);
        }
    }
}
