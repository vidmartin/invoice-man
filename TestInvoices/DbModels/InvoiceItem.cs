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
    /// Položka faktury
    /// </summary>
    public class InvoiceItem
    {
        public int Id { get; set; } = -1;

        /// <summary>
        /// Id příslušné faktury
        /// </summary>
        [JsonIgnore]
        public int InvoiceId { get; set; }

        /// <summary>
        /// Příslušná faktura
        /// </summary>
        [JsonIgnore]
        public Invoice Invoice { get; set; }

        /// <summary>
        /// Počet položek
        /// </summary>
        [Required]
        [ApiEditable]
        [Display(Name = "Počet")]
        public int Count { get; set; }

        /// <summary>
        /// Cena za kus (v Kč)
        /// </summary>
        [Required]
        [ApiEditable]
        [Display(Name = "Cena za kus")]
        public decimal PricePerOne { get; set; }

        /// <summary>
        /// Popis položky
        /// </summary>
        [Required]
        [ApiEditable]
        [Display(Name = "Popis")]
        public string Description { get; set; }
    }
}
