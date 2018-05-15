using System;
using Sitecore.Data.Events;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions;

namespace Sitecore.Feature.Redirects.EventHandlers
{
    public class RedirectMapCacheClearer
    {
        public void ClearCache(object sender, EventArgs args)
        {
            RedirectsCache.Reset();
        }

        public void OnItemSaved(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(args, nameof(args));

            CheckClearCache((args as ItemSavedEventArgs)?.Item);
        }

        public void OnItemSavedRemote(object sender, EventArgs args)
        {
            Assert.ArgumentNotNull(sender, nameof(sender));
            Assert.ArgumentNotNull(args, nameof(args));

            CheckClearCache((args as ItemSavedRemoteEventArgs)?.Item);
        }

        void CheckClearCache(Item item)
        {
            if (item != null && item.TemplateID.Equals(Templates.RedirectMap.ID) && !JobsHelper.IsPublishing())
            {
                RedirectsCache.Reset();
            }
        }
    }
}
