using System;
using System.Collections.Generic;
using System.Web;

namespace Sitecore.Feature.Redirects.Repositories
{
    public class RedirectsRepository : IRedirectsRepository
    {
        public void Reset()
        {
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
        }
    }
}
