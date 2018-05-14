using Sitecore.Pipelines.HttpRequest;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.StringExtensions;
using System.Web;
using Sitecore.Data.Fields;
using Sitecore.Links;
using Sitecore.Sites;

namespace Sitecore.Feature.Redirects.Pipelines.HttpRequestBegin
{
    public class RedirectItem : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            var item = Context.Item;
            if (Context.Database == null || item == null)
            {
                return;
            }

            if (item.IsDerived(Templates.Redirect.ID))
            {
                var redirectUrl = this.GetRedirectUrl();
                if (!string.IsNullOrEmpty(redirectUrl))
                {
                    HttpContext.Current.Response.Redirect(redirectUrl, true);
                    args.AbortPipeline();
                }
            }
        }

        protected virtual string GetRedirectUrl()
        {
            LinkField linkField = Context.Item.Fields[Templates.Redirect.Fields.RedirectUrl];

            if (linkField != null)
            {
                if (linkField.IsInternal && linkField.TargetItem != null)
                {
                    var siteInfo = linkField.TargetItem.GetSite();
                    var defaultOptions = UrlOptions.DefaultOptions;
                    defaultOptions.Site = SiteContextFactory.GetSiteContext(siteInfo.Name);
                    defaultOptions.AlwaysIncludeServerUrl = true;
                    return LinkManager.GetItemUrl(linkField.TargetItem, defaultOptions) + (string.IsNullOrEmpty(linkField.QueryString) ? "" : ("?" + linkField.QueryString));
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
