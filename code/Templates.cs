using Sitecore.Data;

namespace Sitecore.Feature.Redirects
{
    public static class Templates
    {
        public struct Redirect
        {
            public static ID ID = ID.Parse("{C14B6289-8AC2-439C-9E5B-40DE9F820C3F}");

            public struct Fields
            {
                public static ID RedirectUrl { get; } = new ID("{22753447-BE25-4035-A3C9-7F875AFE8E57}");
            }
        }

        public struct RedirectMap
        {
            public static ID ID = ID.Parse("{F4FB6125-F113-4373-8AA2-4648C2C1960E}");

            public struct Fields
            {
                public static ID RedirectType { get; } = new ID("{980DFA07-41D4-4C4E-AB28-DE657AFDB6BC}");

                public static ID PreserveQueryString { get; } = new ID("{F24D347E-98C2-4995-A14C-B15697BA86E5}");

                public static ID UrlMapping { get; } = new ID("{5ECFFFC9-D530-4512-B757-9E9830F07D5A}");
            }
        }

        public struct RedirectMapGrouping
        {
            public static ID ID = ID.Parse("{E1CF805E-7F49-4EC9-A25A-182DC798CB5F}");
        }
    }
}
