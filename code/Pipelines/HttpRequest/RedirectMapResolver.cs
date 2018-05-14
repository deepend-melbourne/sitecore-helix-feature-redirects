using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Text;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;

namespace Sitecore.Feature.Redirects.Pipelines.HttpRequest
{
    public class RedirectMapResolver : HttpRequestProcessor
    {
        private string ResolvedMappingsPrefix
        {
            get
            {
                return string.Format("{0}ResolvedMappings-{1}-{2}", Constants.CachePrefix, Context.Database.Name, Context.Site.Name);
            }
        }

        private string AllMappingsPrefix
        {
            get
            {
                return string.Format("{0}AllMappings-{1}-{2}", Constants.CachePrefix, Context.Database.Name, Context.Site.Name);
            }
        }

        public int CacheExpiration { get; set; }

        public override void Process(HttpRequestArgs args)
        {
            if (Context.Item != null || Context.Database == null || Context.Site == null || this.IsFile(Context.Request.FilePath))
            {
                return;
            }

            var text = this.EnsureSlashes(Context.Request.FilePath.ToLower());
            var redirectMapping = this.GetResolvedMapping(text);
            var flag = redirectMapping != null;
            if (redirectMapping == null)
            {
                redirectMapping = this.FindMapping(text);
            }
            if (redirectMapping != null && !flag)
            {
                var dictionary = (HttpRuntime.Cache[this.ResolvedMappingsPrefix] as Dictionary<string, RedirectMapping>) ?? new Dictionary<string, RedirectMapping>();
                dictionary[text] = redirectMapping;
                HttpRuntime.Cache.Add(ResolvedMappingsPrefix, dictionary, null, DateTime.UtcNow.AddMinutes(CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
            }
            if (redirectMapping != null && HttpContext.Current != null)
            {
                var targetUrl = this.GetTargetUrl(redirectMapping, text);
                if (redirectMapping.RedirectType == RedirectType.Redirect301)
                {
                    Redirect301(HttpContext.Current.Response, targetUrl);
                }
                if (redirectMapping.RedirectType == RedirectType.Redirect302)
                {
                    HttpContext.Current.Response.Redirect(targetUrl, true);
                }
                if (redirectMapping.RedirectType == RedirectType.ServerTransfer)
                {
                    HttpContext.Current.Server.TransferRequest(targetUrl);
                }
            }
        }

        protected virtual bool IsFile(string filePath) => string.IsNullOrEmpty(filePath) || WebUtil.IsExternalUrl(filePath) || File.Exists(HttpContext.Current.Server.MapPath(filePath));

        protected virtual RedirectMapping GetResolvedMapping(string filePath)
        {
            if (HttpRuntime.Cache[this.ResolvedMappingsPrefix] is Dictionary<string, RedirectMapping> dictionary && dictionary.ContainsKey(filePath))
            {
                return dictionary[filePath];
            }

            return null;
        }

        protected virtual RedirectMapping FindMapping(string filePath)
        {
            foreach (var redirectMapping in MappingsMap)
            {
                if ((!redirectMapping.IsRegex && redirectMapping.Pattern == filePath) || (redirectMapping.IsRegex && redirectMapping.Regex.IsMatch(filePath)))
                {
                    return redirectMapping;
                }
            }
            return null;
        }

        protected virtual IList<RedirectMapping> MappingsMap
        {
            get
            {
                var list = HttpRuntime.Cache[AllMappingsPrefix] as List<RedirectMapping>;
                if (list == null || !list.Any())
                {
                    list = new List<RedirectMapping>();

                    var siteRoot = Context.Site.GetRootItem();
                    if (siteRoot != null)
                    {
                        var item = siteRoot.Children.FirstOrDefault(ent => ent.IsDerived(Templates.RedirectMapGrouping.ID));
                        if (item != null)
                        {
                            var array = item.Axes.GetDescendants()
                                .Where(i => i.IsDerived(Templates.RedirectMap.ID))
                                .Cast<Item>();

                            foreach (var item2 in array)
                            {
                                if (!Enum.TryParse<RedirectType>(item2[Templates.RedirectMap.Fields.RedirectType], out var redirectType))
                                {
                                    Log.Info(string.Format("Redirect map {0} does not specify redirect type.", item2.Paths.FullPath), this);
                                }
                                else
                                {
                                    var @bool = MainUtil.GetBool(item2[Templates.RedirectMap.Fields.PreserveQueryString], false);
                                    var urlString = new UrlString
                                    {
                                        Query = item2[Templates.RedirectMap.Fields.UrlMapping]
                                    };
                                    foreach (var obj in urlString.Parameters.Keys)
                                    {
                                        var text = (string)obj;
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            var text2 = urlString.Parameters[text];
                                            if (!string.IsNullOrEmpty(text2))
                                            {
                                                var text3 = text.ToLower();
                                                var flag = text3.StartsWith("^", StringComparison.Ordinal) && text3.EndsWith("$", StringComparison.Ordinal);
                                                if (!flag)
                                                {
                                                    text3 = this.EnsureSlashes(text3);
                                                }
                                                text2 = (HttpUtility.UrlDecode(text2.ToLower()) ?? string.Empty);
                                                text2 = text2.TrimStart(new char[]
                                                {
                                                '^'
                                                }).TrimEnd(new char[]
                                                {
                                                '$'
                                                });
                                                list.Add(new RedirectMapping
                                                {
                                                    RedirectType = redirectType,
                                                    PreserveQueryString = @bool,
                                                    Pattern = text3,
                                                    Target = text2,
                                                    IsRegex = flag
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                if (CacheExpiration > 0)
                {
                    HttpRuntime.Cache.Add(AllMappingsPrefix, list, null, DateTime.UtcNow.AddMinutes(CacheExpiration), TimeSpan.Zero, CacheItemPriority.Normal, null);
                }

                return list;
            }
        }

        protected virtual string GetTargetUrl(RedirectMapping mapping, string input)
        {
            var text = mapping.Target;
            if (mapping.IsRegex)
            {
                text = mapping.Regex.Replace(input, text);
            }
            if (mapping.PreserveQueryString)
            {
                text += HttpContext.Current.Request.Url.Query;
            }
            if (!string.IsNullOrEmpty(Context.Site.VirtualFolder))
            {
                text = StringUtil.EnsurePostfix('/', Context.Site.VirtualFolder) + text.TrimStart(new char[]
                {
                    '/'
                });
            }
            return text;
        }

        protected virtual void Redirect301(HttpResponse response, string url)
        {
            var httpCookieCollection = new HttpCookieCollection();
            for (var i = 0; i < response.Cookies.Count; i++)
            {
                var httpCookie = response.Cookies[i];
                if (httpCookie != null)
                {
                    httpCookieCollection.Add(httpCookie);
                }
            }
            response.Clear();
            for (var j = 0; j < httpCookieCollection.Count; j++)
            {
                var httpCookie2 = httpCookieCollection[j];
                if (httpCookie2 != null)
                {
                    response.Cookies.Add(httpCookie2);
                }
            }
            response.Status = "301 Moved Permanently";
            response.AddHeader("Location", url);
            response.End();
        }

        private string EnsureSlashes(string text)
        {
            return StringUtil.EnsurePostfix('/', StringUtil.EnsurePrefix('/', text));
        }
    }
}
