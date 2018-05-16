using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using Sitecore.Diagnostics;
using Sitecore.Text;

namespace Sitecore.Feature.Redirects.FieldTypes
{
    [UsedImplicitly]
    public class UrlMappingList : Shell.Applications.ContentEditor.NameValue
    {
        protected override string NameStyle { get; } = "width:300px";

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            if (Sitecore.Context.ClientPage.IsEvent)
            {
                this.LoadValue();
                return;
            }

            this.BuildControl();
        }

        protected virtual void LoadValue()
        {
            if (this.ReadOnly || this.Disabled)
            {
                return;
            }

            var nameValueCollection = HttpContext.Current.Handler is Page page ? page.Request.Form : new NameValueCollection();
            var urlString = new UrlString();
            foreach (var obj in nameValueCollection.Keys)
            {
                var text = (string)obj;
                if (!string.IsNullOrEmpty(text) && text.StartsWith(this.ID + "_Param", StringComparison.InvariantCulture) && !text.EndsWith("_value", StringComparison.InvariantCulture))
                {
                    var text2 = nameValueCollection[text];
                    var text3 = nameValueCollection[text + "_value"];
                    if (!string.IsNullOrEmpty(text2))
                    {
                        urlString[text2] = text3 ?? string.Empty;
                    }
                }
            }

            var text4 = urlString.ToString();
            if (this.Value != text4)
            {
                this.Value = text4;
                this.SetModified();
            }
        }

        protected virtual void BuildControl()
        {
            var urlString = new UrlString
            {
                Query = this.Value
            };
            foreach (var obj in urlString.Parameters.Keys)
            {
                var text = (string)obj;
                if (text.Length > 0)
                {
                    this.Controls.Add(new LiteralControl(this.BuildParameterKeyValue(text, urlString.Parameters[text])));
                }
            }

            this.Controls.Add(new LiteralControl(this.BuildParameterKeyValue(string.Empty, string.Empty)));
        }

        [UsedImplicitly]
        protected new void ParameterChange()
        {
            var clientPage = Sitecore.Context.ClientPage;
            if (clientPage.ClientRequest.Source == StringUtil.GetString(clientPage.ServerProperties[this.ID + "_LastParameterID"]) && !string.IsNullOrEmpty(clientPage.ClientRequest.Form[clientPage.ClientRequest.Source]))
            {
                var value = this.BuildParameterKeyValue(string.Empty, string.Empty);
                clientPage.ClientResponse.Insert(this.ID, "beforeEnd", value);
            }

            clientPage.ClientResponse.SetReturnValue(true);
        }

        protected virtual string BuildParameterKeyValue(string key, string value)
        {
            Assert.ArgumentNotNull(key, "key");
            Assert.ArgumentNotNull(value, "value");
            var uniqueID = GetUniqueID(this.ID + "_Param");
            Sitecore.Context.ClientPage.ServerProperties[this.ID + "_LastParameterID"] = uniqueID;
            var clientEvent = Sitecore.Context.ClientPage.GetClientEvent(this.ID + ".ParameterChange");
            var text = this.ReadOnly ? " readonly=\"readonly\"" : string.Empty;
            var text2 = this.Disabled ? " disabled=\"disabled\"" : string.Empty;
            var arg = this.IsVertical ? "</tr><tr>" : string.Empty;

            return string.Format("<table width=\"100%\" class='scAdditionalParameters'><tr><td>{0}</td>{2}<td width=\"100%\">{1}</td></tr></table>", $"<input id=\"{uniqueID}\" name=\"{uniqueID}\" type=\"text\"{text}{text2} style=\"{this.NameStyle}\" value=\"{StringUtil.EscapeQuote(key)}\" onchange=\"{clientEvent}\"/>", this.GetValueHtmlControl(uniqueID, StringUtil.EscapeQuote(HttpUtility.UrlDecode(value))), arg);
        }
    }
}
