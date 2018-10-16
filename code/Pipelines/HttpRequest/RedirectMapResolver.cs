using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Text;
using Sitecore.Web;

namespace Sitecore.Feature.Redirects.Pipelines.HttpRequest
{
    public class RedirectMapResolver : HttpRequestProcessor
    {
        public int CacheExpiration { get; set; }

        static string ResolvedMappingsPrefix => $"{Constants.CachePrefix}ResolvedMappings-{Context.Database.Name}-{Context.Site.Name}";

        static string AllMappingsPrefix => $"{Constants.CachePrefix}AllMappings-{Context.Database.Name}-{Context.Site.Name}";

        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item != null || Context.Database == null || Context.Site == null || IsFile(Context.Request.FilePath) || HttpContext.Current == null)
            {
                return;
            }

            // Don't override any item resolving in the content/experience editors because this breaks things like the link picker
            if (Context.Site.Name?.Equals("shell", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                return;
            }

            var filePath = EnsureSlashes(Context.Request.FilePath.ToLower());
            var redirectMapping = GetCachedMapping(filePath) ?? FindMapping(filePath);

            if (redirectMapping != null)
            {
                var dictionary = (HttpRuntime.Cache[ResolvedMappingsPrefix] as Dictionary<string, RedirectMapping>) ?? new Dictionary<string, RedirectMapping>();

                dictionary[filePath] = redirectMapping;

                HttpRuntime.Cache.Add(ResolvedMappingsPrefix, dictionary, null, DateTime.UtcNow.AddMinutes(CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);

                var targetUrl = redirectMapping.GetTargetUrl(HttpContext.Current, filePath);
                switch (redirectMapping.RedirectType)
                {
                    case RedirectType.Redirect301:
                        Redirect301(HttpContext.Current.Response, targetUrl);
                        break;
                    case RedirectType.Redirect302:
                        HttpContext.Current.Response.Redirect(targetUrl, true);
                        break;
                    case RedirectType.ServerTransfer:
                        HttpContext.Current.Server.TransferRequest(targetUrl);
                        break;
                }
            }
        }

        bool IsFile(string filePath) => string.IsNullOrEmpty(filePath) || WebUtil.IsExternalUrl(filePath) || File.Exists(HttpContext.Current.Server.MapPath(filePath));

        RedirectMapping GetCachedMapping(string filePath)
        {
            if (HttpRuntime.Cache[ResolvedMappingsPrefix] is Dictionary<string, RedirectMapping> dictionary && dictionary.ContainsKey(filePath))
            {
                return dictionary[filePath];
            }

            return null;
        }

        RedirectMapping FindMapping(string filePath)
        {
            try
            {
                var redirectMap = (HttpRuntime.Cache[AllMappingsPrefix] as IEnumerable<RedirectMapping>) ?? BuildMap();

                if (CacheExpiration > 0)
                {
                    HttpRuntime.Cache.Add(AllMappingsPrefix, redirectMap, null, DateTime.UtcNow.AddMinutes(CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
                }

                return redirectMap.FirstOrDefault(ent => ((!ent.IsRegex && ent.Pattern == filePath) || (ent.IsRegex && ent.Regex.IsMatch(filePath))));
            }
            catch (NullReferenceException)
            {
                Log.Error($"NullReferenceException looking up redirect for '{filePath}'", this);

                return null;
            }
        }

        void Redirect301(HttpResponse response, string url)
        {
            response.ClearContent();

            response.Status = "301 Moved Permanently";
            response.AddHeader("Location", url);
            response.End();
        }

        string EnsureSlashes(string text) => StringUtil.EnsurePostfix('/', StringUtil.EnsurePrefix('/', text));

        IEnumerable<RedirectMapping> BuildMap()
        {
            var siteRoot = Context.Site.GetRootItem();
            if (siteRoot != null)
            {
                var item = siteRoot.Children.FirstOrDefault(ent => ent.IsDerived(Templates.RedirectMapGrouping.ID));
                if (item != null)
                {
                    return item.Axes.GetDescendants()
                        .Where(i => i.IsDerived(Templates.RedirectMap.ID))
                        .SelectMany(ent =>
                        {
                            if (!Enum.TryParse<RedirectType>(ent[Templates.RedirectMap.Fields.RedirectType], out var redirectType))
                            {
                                Log.Info(string.Format("Redirect map '{0}' does not specify redirect type.", ent.Paths.FullPath), this);

                                return null;
                            }

                            var urlString = new UrlString
                            {
                                Query = ent[Templates.RedirectMap.Fields.UrlMapping]
                            };

                            return urlString.Parameters.Keys
                                    .Cast<string>()
                                    .Where(k => !string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(urlString.Parameters[k]))
                                    .ToDictionary(k => k, k => urlString.Parameters[k])
                                    .Select(kvp =>
                                    {
                                        var pattern = kvp.Key.ToLower();

                                        var isRegex = pattern.StartsWith("^", StringComparison.Ordinal) && pattern.EndsWith("$", StringComparison.Ordinal);
                                        if (!isRegex)
                                        {
                                            pattern = EnsureSlashes(pattern);
                                        }

                                        var target = (HttpUtility.UrlDecode(kvp.Value.ToLower()) ?? string.Empty)
                                            .TrimStart(new char[] { '^' })
                                            .TrimEnd(new char[] { '$' });

                                        return new RedirectMapping
                                        {
                                            RedirectType = redirectType,
                                            PreserveQueryString = MainUtil.GetBool(ent[Templates.RedirectMap.Fields.PreserveQueryString], false),
                                            Pattern = pattern,
                                            Target = target,
                                            IsRegex = isRegex
                                        };
                                    });
                        })
                        .Where(ent => ent != null)
                        .ToList();
                }
            }

            return Enumerable.Empty<RedirectMapping>();
        }
    }
}
