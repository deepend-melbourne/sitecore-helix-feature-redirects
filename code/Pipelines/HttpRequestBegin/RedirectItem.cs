using System.Web;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
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

                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    HttpContext.Current.Response.Redirect(redirectUrl, true);
                    args.AbortPipeline();
                }
            }
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
