using System;
using System.Collections.Generic;

namespace Sitecore.Feature.Redirects.Models
{
    public class ImportCsvRequest
    {
        public Guid Id { get; set; }

        public IDictionary<string, string> Redirects { get; set; }
    }
}
