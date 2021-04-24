using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocAuth
{
    public class DocsDBContext : DbContext
    {
        public DocsDBContext() : base("name=DocScan")
        {

        }
        public DbSet<DocAuth.Model.Document> Documents { get; set; }
    }
}
