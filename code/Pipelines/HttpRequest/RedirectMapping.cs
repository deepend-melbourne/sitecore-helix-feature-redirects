using System.Text.RegularExpressions;
using System.Web;

namespace Sitecore.Feature.Redirects.Pipelines.HttpRequest
{
    public enum RedirectType
    {
        Redirect301,
        Redirect302,
        ServerTransfer
    }

    public class RedirectMapping
    {
        Regex _regex;

        public RedirectType RedirectType { get; set; }

        public bool PreserveQueryString { get; set; }

        public string Pattern { get; set; }

        public string Target { get; set; }

        public bool IsRegex { get; set; }

        public Regex Regex
        {
            get
            {
                if (!IsRegex)
                {
                    return null;
                }

                return _regex = _regex ?? new Regex(Pattern, RegexOptions.IgnoreCase);
            }
        }

        public string GetTargetUrl(HttpContext httpContext, string input)
        {
            var target = Target;
            if (IsRegex)
            {
                target = Regex.Replace(input, target);
            }

            if (PreserveQueryString)
            {
                target += httpContext.Request.Url.Query;
            }

            if (!string.IsNullOrEmpty(Context.Site.VirtualFolder))
            {
                target = StringUtil.EnsurePostfix('/', Context.Site.VirtualFolder) + target.TrimStart(new char[] { '/' });
            }

            return target;
        }
    }
}
