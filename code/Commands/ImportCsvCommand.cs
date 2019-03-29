using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Shell.Framework.Commands;
using System.Linq;

namespace Sitecore.Feature.Redirects.Commands
{
    public class ImportCsvCommand : Command
    {
        public override void Execute(CommandContext context)
        {
            var item = context.Items.First();
            if (item.IsDerived(Templates.RedirectMap.ID))
            {
                Context.ClientPage.ClientResponse.ShowModalDialog(new Web.UI.Sheer.ModalDialogOptions($"/sitecore/shell/Applications/Redirects/upload.html?items={string.Join(",", item.ID)}")
                {
                    Maximizable = false,
                    Header = "Import redirects"
                });
            }
            else
            {
                Context.ClientPage.ClientResponse.Alert("This operation is only available on 'Redirect Map' items");
            }
        }
    }
}
