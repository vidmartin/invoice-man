using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestInvoices.DbModels;

namespace TestInvoices.Api
{
    public class ApiEditInvoiceModel
    {
        //public int Id { get; set; }
        public dynamic Patch { get; set; }
        public List<InvoiceItem> AddItems { get; set; }
        public List<dynamic> PatchItems { get; set; }
        public List<int> RemoveItems { get; set; }
    }
}
