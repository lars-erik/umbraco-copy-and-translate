using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.PasteAndTranslate
{
    [PluginController("CopyAndTranslate")]
    public class LanguagesController : UmbracoApiController
    {
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
