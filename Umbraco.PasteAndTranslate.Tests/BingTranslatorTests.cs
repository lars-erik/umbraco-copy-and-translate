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

        //[Test]
        //public void TranslateAsync_Is_Awaitable()
        //{
        //    var translator = new BingTranslator();
        //    var task = translator.TranslateAsync("Hello world!", "en", "no");
        //    task.Wait(TimeSpan.FromSeconds(10));
        //    Assert.AreEqual("Hallo, verden!", task.Result);
        //}
    }
}
