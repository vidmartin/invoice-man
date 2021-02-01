using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TestInvoices.DbModels;

namespace TestInvoices.Managers
{
    /// <summary>
    /// Uchovává faktury v paměti pomocí slovníku
    /// </summary>
    public class MockInvoiceManager : IInvoiceManager
    {
        private Dictionary<int, Invoice> _invoices = new Dictionary<int, Invoice>();
        private int _invoiceIdCounter = 0; //v SQL databázi by tohle byl primární klíč v tabulce s fakturami
        private int _invoiceItemIdCounter = 0; //v SQL databázi by tohle byl primární klíč v tabulce s položkami faktur 

        public void Add(Invoice invoice)
        {
            try
            {
                invoice.Id = _invoiceIdCounter; //ujistit se, že faktura má správné id
                Interlocked.Increment(ref _invoiceIdCounter); //zvýšit počitadlo faktur (pro jistotu používám thread-safe způsob)

                invoice.Items.ForEach(item =>
                {
                    //ujistit se, že položky faktury mají odkaz na správnou fakturu
                    item.InvoiceId = invoice.Id;
                    item.Invoice = invoice;

                    //zařídit, aby každá položka měla unikátní id (i napříč fakturami)
                    item.Id = _invoiceItemIdCounter;
                    Interlocked.Increment(ref _invoiceItemIdCounter); //zvýšit počitadlo položek (pro jistotu používám thread-safe způsob)
                });

                _invoices.Add(invoice.Id, invoice); //přidat fakturu do seznamu
            }
            catch
            {
                _invoices.Remove(invoice.Id); //pokud se něco nepovede, ujistit se, že rozbitá faktura není v listu
                throw;
            }
        }

        public bool Delete(int invoiceId)
        {
            return _invoices.Remove(invoiceId); //odstranit fakturu ze slovníku
        }

        public void Update(int invoiceId, Invoice invoice)
        {
            if (!_invoices.ContainsKey(invoiceId))
                throw new InvalidOperationException($"Faktura s id {invoice.Id} nenalezena.");
            invoice.Id = invoiceId;
            var oldInvoice = _invoices[invoiceId];
            _invoices[invoiceId] = invoice;
            invoice.Items.ForEach(item =>
            {
                //ujistit se, že položky faktury mají odkaz na správnou fakturu
                item.InvoiceId = invoiceId;
                item.Invoice = invoice;

                //TODO: zamezit tomu, aby více faktur mělo stejný id
                //zařídit, aby každá NOVĚ PŘIDANÁ položka měla unikátní id (i napříč fakturami)
                if (item.Id == -1 || item.Id > _invoiceItemIdCounter)
                {
                    item.Id = _invoiceItemIdCounter;
                    Interlocked.Increment(ref _invoiceItemIdCounter); //zvýšit počitadlo položek (pro jistotu používám thread-safe způsob)
                }
            });
        }

        public Invoice GetById(int id) => _invoices.TryGetValue(id, out Invoice val) ? val : null;
        public IEnumerable<Invoice> GetWhere(Func<Invoice, bool> filter) => _invoices.Values.Where(filter);
    }
}
