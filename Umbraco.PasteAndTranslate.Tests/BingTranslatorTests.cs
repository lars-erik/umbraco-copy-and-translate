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
        public void A_Fixture_Must_Warmup()
        {
            var translator = new BingTranslator();
            Assert.IsTrue(true);
        }

        [Test]
        public void Translate_HelloWorld_Returns_HalloVerden()
        {
            var translator = new BingTranslator();
            var result = translator.Translate("Hello world!", "en", "no");
            Assert.AreEqual("Hallo, verden!", result);
        }

        [Test]
        public void TranslateAsync_Is_Awaitable()
        {
            var translator = new BingTranslator();
            var task = translator.TranslateAsync("Hello world!", "en", "no");
            task.Wait(TimeSpan.FromSeconds(10));
            Assert.AreEqual("Hallo, verden!", task.Result);
        }

        [Test]
        public void MultipleTranslate_Takes_Longer()
        {
            var translator = new BingTranslator();
            var result1 = translator.Translate("Hello world!", "en", "no");
            var result2 = translator.Translate("Hei verden!", "no", "en");

            Assert.AreEqual("Hallo, verden!", result1);
            Assert.AreEqual("Hello World!", result2);
        }

        [Test]
        public void MultipleTranslateAsync_Is_Awaitable()
        {
            var translator = new BingTranslator();
            var task1 = translator.TranslateAsync("Hello world!", "en", "no");
            var task2 = translator.TranslateAsync("Hei verden!", "no", "en");

            Task.WhenAll(task1, task2).Wait(TimeSpan.FromSeconds(10));
            
            Assert.AreEqual("Hallo, verden!", task1.Result);
            Assert.AreEqual("Hello World!", task2.Result);
        }
    }
}
