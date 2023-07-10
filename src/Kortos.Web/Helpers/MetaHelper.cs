using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Kortos.Web.Helpers;

public static class MetaHelper
{
    public static ReadOnlyDictionary<string, string> MetaKeys => new Dictionary<string, string>
    {
        { "keywords", "MetaKeywords" },
        { "description", "MetaDescription" },
        { "generator", "MetaGenerator" },
        { "referrer", "MetaReferrer" },
        { "theme-color", "MetaThemeColor" },
        { "color-scheme", "MetaColorScheme" },
        { "robots", "MetaRobots" },
        { "publisher", "MetaPublisher" },
        { "googlebot", "MetaRobots" },
        { "creator", "MetaCreator" }
    }.AsReadOnly();
    
    public static string MetaTag(this IHtmlHelper helper, string name)
    {
        

        if (!MetaKeys.TryGetValue(name, out var key)
            || !helper.ViewData.TryGetValue(key, out var value))
        {
            return string.Empty;
        }

        if (value is not IEnumerable<object> collection)
        {
            return $"<meta name='{name}' content='{value}'>";
        }
        
        return string.Join(Environment.NewLine,
            collection.Select(val => $"<meta name='{name}' content='{val}'>")
            .ToArray());
    }
}