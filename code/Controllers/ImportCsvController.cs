using Sitecore.Data;
using Sitecore.Feature.Redirects.Models;
using Sitecore.Text;
using System.Net;
using System.Web.Mvc;

namespace Sitecore.Feature.Redirects.Controllers
{
    public class ImportCsvController : Controller
    {
        public ActionResult Import(ImportCsvRequest request)
        {
            var master = Sitecore.Configuration.Factory.GetDatabase("master");
            var item = master.GetItem(new ID(request.Id));

            var urlString = new UrlString
            {
                Query = item[Templates.RedirectMap.Fields.UrlMapping]
            };

            foreach (var kvp in request.Redirects)
            {
                urlString.Parameters.Set(kvp.Key, kvp.Value);
            }

            item.Editing.BeginEdit();
            try
            {
                item[Templates.RedirectMap.Fields.UrlMapping] = urlString.Query;
                item.Editing.AcceptChanges();
            }
            finally
            {
                item.Editing.EndEdit();
            }

            return new HttpStatusCodeResult(HttpStatusCode.NoContent);
        }
    }
}
