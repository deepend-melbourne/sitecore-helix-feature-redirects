using System;
using System.Collections.Generic;
using System.Web;
using Sitecore.Diagnostics;

namespace Sitecore.Feature.Redirects
{
    internal static class RedirectsCache
    {
        const string LoggerName = "Sitecore.Feature.Redirects";

        public static void Reset()
        {
            Log.Info("RedirectMapCacheClearer clearing redirect map cache.", LoggerName);

            var list = new List<string>();

            var enumerator = HttpRuntime.Cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                if (key.StartsWith(Constants.CachePrefix, StringComparison.Ordinal))
                {
                    list.Add(key);
                }
            }

            foreach (var key in list)
            {
                HttpRuntime.Cache.Remove(key);
            }

            Log.Info("RedirectMapCacheClearer done.", LoggerName);
        }
    }
}
