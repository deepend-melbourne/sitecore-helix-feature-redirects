using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Text;

namespace Sitecore.Feature.Redirects.FieldTypes
{
    [UsedImplicitly]
    public class UrlMappingList : Shell.Applications.ContentEditor.NameValue
    {
        protected override string NameStyle { get; } = "width:300px";

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, nameof(e));

            if (Sitecore.Context.ClientPage.IsEvent)
            {
                LoadValue();
            }
            else
            {
                BuildControl();
            }
        }

        protected virtual void LoadValue()
        {
            if (ReadOnly || Disabled)
            {
                return;
            }

            var values = HttpContext.Current.Handler is Page page ? page.Request.Form : new NameValueCollection();
            var dict = values.Keys.Cast<string>()
                .Where(ent => !string.IsNullOrEmpty(ent))
                .Where(ent => ent.StartsWith(ID + "_Param", StringComparison.InvariantCulture))
                .Where(ent => !ent.EndsWith("_value", StringComparison.InvariantCulture))
                .Where(ent => !string.IsNullOrEmpty(values[ent]))
                .ToDictionary(ent => values[ent], ent => values[$"{ent}_value"])
                .ToNameValueCollection();

            var urlString = new UrlString(dict);

            var value = urlString.ToString();
            if (Value != value)
            {
                Value = value;
                SetModified();
            }
        }

        protected virtual void BuildControl()
        {
            var urlString = new UrlString { Query = Value };
            var keys = urlString.Parameters.Keys.Cast<string>()
                .Where(ent => !string.IsNullOrEmpty(ent));

            foreach (var key in keys)
            {
                Controls.Add(new LiteralControl(BuildParameterKeyValue(key, urlString.Parameters[key])));
            }

            Controls.Add(new LiteralControl(BuildParameterKeyValue(string.Empty, string.Empty)));
        }

        [UsedImplicitly]
        protected new void ParameterChange()
        {
            var clientPage = Sitecore.Context.ClientPage;
            if (clientPage.ClientRequest.Source == StringUtil.GetString(clientPage.ServerProperties[ID + "_LastParameterID"]) && !string.IsNullOrEmpty(clientPage.ClientRequest.Form[clientPage.ClientRequest.Source]))
            {
                var value = BuildParameterKeyValue(string.Empty, string.Empty);
                clientPage.ClientResponse.Insert(ID, "beforeEnd", value);
            }

            clientPage.ClientResponse.SetReturnValue(true);
        }

        protected virtual string BuildParameterKeyValue(string key, string value)
        {
            Assert.ArgumentNotNull(key, nameof(key));
            Assert.ArgumentNotNull(value, nameof(value));

            var uniqueID = GetUniqueID(ID + "_Param");
            Sitecore.Context.ClientPage.ServerProperties[ID + "_LastParameterID"] = uniqueID;
            var clientEvent = Sitecore.Context.ClientPage.GetClientEvent(ID + ".ParameterChange");
            var readOnlyAttr = ReadOnly ? " readonly=\"readonly\"" : string.Empty;
            var disabledAttr = Disabled ? " disabled=\"disabled\"" : string.Empty;
            var arg = IsVertical ? "</tr><tr>" : string.Empty;

            return string.Format("<table width=\"100%\" class='scAdditionalParameters'><tr><td>{0}</td>{2}<td width=\"100%\">{1}</td></tr></table>", $"<input id=\"{uniqueID}\" name=\"{uniqueID}\" type=\"text\"{readOnlyAttr}{disabledAttr} style=\"{NameStyle}\" value=\"{StringUtil.EscapeQuote(key)}\" onchange=\"{clientEvent}\"/>", GetValueHtmlControl(uniqueID, StringUtil.EscapeQuote(HttpUtility.UrlDecode(value))), arg);
        }
    }
}
