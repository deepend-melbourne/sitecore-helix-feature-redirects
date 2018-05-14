using System.Text.RegularExpressions;

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
        public RedirectType RedirectType { get; set; }

        public bool PreserveQueryString { get; set; }

        public string Pattern { get; set; }

        public string Target { get; set; }

        public bool IsRegex { get; set; }

        public Regex Regex
        {
            get
            {
                if (!this.IsRegex)
                {
                    return null;
                }
                Regex result;
                if ((result = this._regex) == null)
                {
                    result = (this._regex = new Regex(this.Pattern, RegexOptions.IgnoreCase));
                }
                return result;
            }
        }

        private Regex _regex;
    }
}
