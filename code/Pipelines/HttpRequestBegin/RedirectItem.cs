using System;
using System.Web;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Feature.Redirects.Pipelines.HttpRequest;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Links;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Sites;

namespace Sitecore.Feature.Redirects.Pipelines.HttpRequestBegin
{
    public class RedirectItem : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            var item = Context.Item;

            if (item == null)
            {
                return;
            }

            if (item.IsDerived(Templates.Redirect.ID))
            {
                var redirectUrl = GetRedirectUrl(item);
                if (!Enum.TryParse<RedirectType>(item[Templates.Redirect.Fields.RedirectType], out var redirectType))
                {
                    Log.Info(string.Format("Redirect item {0} does not specify redirect type.", item.Paths.FullPath), this);

                    return;
                }

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    switch (redirectType)
                    {
                        case RedirectType.Redirect301:
                            Redirect301(HttpContext.Current.Response, redirectUrl);
                            break;
                        case RedirectType.Redirect302:
                            HttpContext.Current.Response.Redirect(redirectUrl, true);
                            break;
                        case RedirectType.ServerTransfer:
                            HttpContext.Current.Server.TransferRequest(redirectUrl);
                            break;
                    }

                    args.AbortPipeline();
                }
            }
        }

        static void Redirect301(HttpResponse response, string url)
        {
            response.ClearContent();

            response.Status = "301 Moved Permanently";
            response.AddHeader("Location", url);
            response.End();
        }

        static string GetRedirectUrl(Item item)
        {
            var linkField = (LinkField)item.Fields[Templates.Redirect.Fields.RedirectUrl];

            if (linkField != null)
            {
                if (linkField.IsInternal && linkField.TargetItem != null)
                {
                    var siteInfo = linkField.TargetItem.GetSite();
                    var urlOptions = UrlOptions.DefaultOptions;
                    urlOptions.Site = SiteContextFactory.GetSiteContext(siteInfo.Name);
                    urlOptions.AlwaysIncludeServerUrl = true;

                    return LinkManager.GetItemUrl(linkField.TargetItem, urlOptions) + (string.IsNullOrEmpty(linkField.QueryString) ? string.Empty : $"?{linkField.QueryString}");
                }
                else
                {
                    return linkField.Url;
                }
            }

            return null;
        }
    }
}
