using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Umbraco.PasteAndTranslate
{
    public class ApplicationEvents : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            base.ApplicationStarted(umbracoApplication, applicationContext);

            TreeControllerBase.MenuRendering += MenuRendering;
            ContentService.Copied += ContentCopied;
        }

        private void MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
        {
            if (e.NodeId.StartsWith("-"))
                return;

            var menuItem = new MenuItem("copyTranslate", "Copy and translate");
            menuItem.Icon = "documents";
            menuItem.LaunchDialogView("/App_Plugins/PasteAndTranslate/copy.html", "Copy and translate");

            var index = e.Menu.Items.IndexOf(e.Menu.Items.SingleOrDefault(m => m.Alias == "copy"));
            e.Menu.Items.Insert(index + 1, menuItem);
        }

        private void ContentCopied(IContentService sender, CopyEventArgs<IContent> e)
        {
            // TODO: Refactor, factory translator and configure ShouldTranslate

            if (HttpContext.Current == null ||
                String.IsNullOrWhiteSpace(HttpContext.Current.Request.Headers["Translate"]))
                return;

            var copy = e.Copy;

            var originalLanguageCode = HttpContext.Current.Request.Headers["translateFrom"];
            var copiedLanguageCode = HttpContext.Current.Request.Headers["translateTo"];

            if (EmptyLanguage(originalLanguageCode, copiedLanguageCode))
                return;

            var originalCulture = CultureInfo.GetCultureInfo(originalLanguageCode);
            var copiedCulture = CultureInfo.GetCultureInfo(copiedLanguageCode);

            var originalLanguage = originalCulture.TwoLetterISOLanguageName;
            var copiedLanguage = copiedCulture.TwoLetterISOLanguageName;

            var translator = new BingTranslator();

            var guid = HttpContext.Current.Request.Headers["translateGuid"];

            HttpContext.Current.Cache[guid] = "Translating " + copy.Name;

            copy.Name = translator.Translate(copy.Name, originalLanguage, copiedLanguage);

            foreach (var property in copy.Properties)
            {
                var type = copy.PropertyTypes.SingleOrDefault(t => String.Equals(t.Alias, property.Alias, StringComparison.InvariantCultureIgnoreCase));
                if (type == null)
                    continue;

                if (ShouldTranslate(type.PropertyEditorAlias))
                {
                    var text = property.Value as string;
                    if (!String.IsNullOrWhiteSpace(text))
                    {
                        property.Value = translator.Translate(text, originalLanguage, copiedLanguage);
                    }
                }
            }

            HttpContext.Current.Cache[guid] = "Translated " + copy.Name;

            sender.Save(copy, 0, false);
        }

        private bool ShouldTranslate(string propertyEditorAlias)
        {
            return true;
        }

        private static bool EmptyLanguage(string originalLanguage, string copiedLanguage)
        {
            return String.IsNullOrWhiteSpace(originalLanguage) ||
                   String.IsNullOrWhiteSpace(copiedLanguage);
        }
    }
}
