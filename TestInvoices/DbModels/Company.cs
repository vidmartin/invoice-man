using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using TestInvoices.Api;

namespace TestInvoices.DbModels
{
    //TODO: [Owned]
    /// <summary>
    /// Info o firmě
    /// </summary>
    public class Company
    {
        /// <summary>
        /// Název firmy
        /// </summary>
        [ApiEditable]
        [Required]
        [MinLength(3)]
        [Display(Name = "Název")]
        public string Name { get; set; }

        /// <summary>
        /// Adresa firmy v rámci města (ulice + číslo popisné)
        /// </summary>
        [ApiEditable]
        [Required]
        [MinLength(5)]
        [Display(Name = "Ulice, čp.")]
        public string Address { get; set; }

        private string _psč;

        /// <summary>
        /// Poštovní směrovací číslo
        /// </summary>
        [ApiEditable]
        [Required]
        [RegularExpression(@"^\d\d\d\s?\d\d$", ErrorMessage = "PSČ neodpovídá požadovanému formátu.")]
        [Display(Name = "PSČ")]
        public string PSČ
        {
            get => _psč;
            set => _psč = value?.Replace(" ", ""); //odstranit mezery (např. nastavujem-li psč na "150 00", nastaví se "15000")
        }

        /// <summary>
        /// Město
        /// </summary>
        [ApiEditable]
        [Required]
        [MinLength(2)] //Aš
        [Display(Name = "Město")]
        public string City { get; set; }

        /// <summary>
        /// Telefonní číslo
        /// </summary>
        [ApiEditable]
        [Required]
        [Display(Name = "Telefon")]
        public string Phone { get; set; }

        /// <summary>
        /// Identifikační číslo osoby
        /// </summary>
        [ApiEditable]
        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Ičo musí být osmimístné číslo bez mezer.")]
        [Display(Name = "IČO")]
        public string IČO { get; set; }

        /// <summary>
        /// Daňové identifikační číslo
        /// </summary>
        [ApiEditable]
        [Required]
        [Display(Name = "DIČ")]
        public string DIČ { get; set; }
    }
}
