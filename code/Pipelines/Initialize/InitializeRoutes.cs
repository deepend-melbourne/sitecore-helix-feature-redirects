using System.Web.Mvc;
using System.Web.Routing;
using Sitecore.Pipelines;

namespace Sitecore.Feature.Redirects.Pipelines.Initialize
{
    public class InitializeRoutes
    {
        public virtual void Process(PipelineArgs args)
        {
            RouteTable.Routes.MapRoute("Feature.Redirects.ImportCsv", "api/redirects/import", new
            {
                controller = "ImportCsv",
                action = "Import"
            });
        }
    }
}
