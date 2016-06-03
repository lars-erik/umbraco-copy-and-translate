using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.PasteAndTranslate
{
    [PluginController("CopyAndTranslate")]
    [UmbracoApplicationAuthorizeAttribute("content")]
    public class LanguagesController : UmbracoAuthorizedApiController
    {
        public string Initialize()
        {
            var guid = Guid.NewGuid().ToString("N");
            HttpContext.Current.Cache.Add(guid, "Starting", null, DateTime.Now.AddDays(1), TimeSpan.Zero, CacheItemPriority.Normal, null);
            return guid;
        }

        public string Status()
        {
            var guid = HttpContext.Current.Request.Headers["translateGuid"];
            var value = HttpContext.Current.Cache[guid] as string;
            Debug.WriteLine("Status for " + guid + ": " + value);
            return value;
        }

        public object PostCopy(MoveOrCopy moveOrCopy)
        {
            var guid = HttpContext.Current.Request.Headers["translateGuid"];
            try
            {
                var controller = new ContentController(UmbracoContext) {Request = Request};
                return controller.PostCopy(moveOrCopy);
            }
            finally
            {
                HttpContext.Current.Cache.Remove(guid);
            }
        }

        public Dictionary<int, IEnumerable<NodeLanguageDto>> GetLanguages(int fromId, int toId)
        {
            var content = ApplicationContext.Services.ContentService.GetByIds(new[] {fromId, toId}.Distinct());
            return content.ToDictionary(
                c => c.Id, 
                c => Languages.FirstLanguageSet(c) ?? new NodeLanguageDto[0]
            );
        }
    }
}
