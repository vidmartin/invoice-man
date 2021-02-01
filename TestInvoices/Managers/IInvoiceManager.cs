using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestInvoices.DbModels;

namespace TestInvoices.Managers
{
    public interface IInvoiceManager
    {
        void Add(Invoice invoice);
        bool Delete(int invoiceId);
        void Update(int invoiceId, Invoice invoice);

        Invoice GetById(int id);
        IEnumerable<Invoice> GetWhere(Func<Invoice, bool> filter);
    }
}
