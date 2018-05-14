using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Feature.Redirects.Repositories;
using Sitecore.Foundation.SitecoreExtensions;
using System;

namespace Sitecore.Feature.Redirects.EventHandlers
{
    public class RedirectMapCacheClearer
    {
        public void ClearCache(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            Log.Info("RedirectMapCacheClearer clearing redirect map cache.", this);
            (ServiceLocator.ServiceProvider.GetService(typeof(IRedirectsRepository)) as IRedirectsRepository).Reset();
            Log.Info("RedirectMapCacheClearer done.", this);
        }

        public void OnItemSaved(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            var item = Event.ExtractParameter(args, 0) as Item;
            if (item == null || !item.TemplateID.Equals(Templates.RedirectMap.ID) || JobsHelper.IsPublishing())
            {
                return;
            }
            this.ClearCache(sender, args);
        }

        public void OnItemSavedRemote(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(args, "args");
            var itemSavedRemoteEventArgs = args as ItemSavedRemoteEventArgs;
            if (itemSavedRemoteEventArgs == null || itemSavedRemoteEventArgs.Item == null || !itemSavedRemoteEventArgs.Item.TemplateID.Equals(Templates.RedirectMap.ID) || JobsHelper.IsPublishing())
            {
                return;
            }
            this.ClearCache(sender, args);
        }
    }
}
