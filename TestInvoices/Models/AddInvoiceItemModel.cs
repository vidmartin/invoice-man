using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestInvoices.DbModels;

namespace TestInvoices.Models
{
    public class AddInvoiceItemModel
    {
        public int InvoiceId { get; set; }
        public InvoiceItem Item { get; set; }
    }
}
