using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TestInvoices.Api;

namespace TestInvoices.DbModels
{
    /// <summary>
    /// Faktura
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; } = -1;

        /// <summary>
        /// Ten, co prodává
        /// </summary>
        [ApiEditable]
        public Company Seller { get; set; }

        /// <summary>
        /// Ten, co kupuje
        /// </summary>
        [ApiEditable]
        public Company Buyer { get; set; }

        /// <summary>
        /// Datum vystavení
        /// </summary>
        [ApiEditable]
        [Display(Name = "Datum vystavení")]
        public DateTime DateOfIssue { get; set; }

        /// <summary>
        /// Datum splatnosti
        /// </summary>
        [ApiEditable]
        [Display(Name = "Datum splatnosti")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Zdalipak byla faktura zaplacena
        /// </summary>
        [ApiEditable]
        [Display(Name = "Je zaplaceno?")]
        public bool Paid { get; set; } = false;

        //TODO: EF
        /// <summary>
        /// Položky faktury
        /// </summary>
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}
