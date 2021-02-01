using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestInvoices.Api;
using TestInvoices.DbModels;
using TestInvoices.Managers;
using TestInvoices.Models;

namespace TestInvoices.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly IInvoiceManager _invoiceManager;

        public InvoiceController(IInvoiceManager invoiceManager)
        {
            this._invoiceManager = invoiceManager;
        }

        /// <summary>
        /// zobrazí seznam faktur
        /// </summary>
        /// <returns></returns>
        public IActionResult List()
        {
            return View(_invoiceManager.GetWhere(_ => true));
        }

        /// <summary>
        /// zobrazí detail faktury
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Detail(int id)
        {
            return View(_invoiceManager.GetById(id) ?? throw new InvalidOperationException("Faktura nenalezena."));
        }

        /// <summary>
        /// odstraní položku (id položky = itemId) dané faktury (id faktury = id)
        /// </summary>
        /// <param name="id">id faktury</param>
        /// <param name="itemId">id položky</param>
        /// <returns></returns>
        [NeedApiKey(isApi: false)]
        public IActionResult DeleteItem(int id, int itemId)
        {
            var invoice = _invoiceManager.GetById(id) ?? throw new InvalidOperationException("Faktura nenalezena.");
            var index = invoice.Items.FindIndex(item => item.Id == itemId);

            if (index == -1)
                throw new InvalidOperationException("Položka nenalezena.");

            invoice.Items.RemoveAt(index);

            _invoiceManager.Update(invoice.Id, invoice);

            return RedirectToAction("Detail", new { id = id });
        }

        /// <summary>
        /// zobrazí formulář pro přidání položky do dané faktury 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [NeedApiKey(isApi: false)]
        public IActionResult AddItem(int id)
        {
            return View(new AddInvoiceItemModel()
            {
                InvoiceId = id,
                Item = new InvoiceItem()
            });
        }

        /// <summary>
        /// přidá danou položku do dané faktury
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost]
        [NeedApiKey(false)]
        public IActionResult AddItem(AddInvoiceItemModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var invoice = _invoiceManager.GetById(model.InvoiceId) ?? throw new InvalidOperationException("Faktura nenalezena.");

            if (invoice.Paid)
                throw new InvalidOperationException("Nelze přidávat položky na již zaplacené faktury.");

            model.Item.Id = -1;
            invoice.Items.Add(model.Item);
            _invoiceManager.Update(invoice.Id, invoice);

            return RedirectToAction("Detail", "Invoice", new { id = invoice.Id });
        }

        /// <summary>
        /// odstraní danou fakturu.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NeedApiKey(isApi: false)]
        public IActionResult DeleteInvoice(int id)
        {
            if (!_invoiceManager.Delete(id))
                throw new InvalidOperationException("Faktura nenalezena.");
            return RedirectToAction("List", "Invoice");
        }

        /// <summary>
        /// zobrazí formulář pro přidání faktury.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [NeedApiKey(isApi: false)]
        public IActionResult AddInvoice()
        {
            return View(new Invoice());
        }

        /// <summary>
        /// přidá fakturu.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        [HttpPost]
        [NeedApiKey(isApi: false)]
        public IActionResult AddInvoice(Invoice invoice)
        {
            if (invoice.Items.Any())
                ModelState.AddModelError("", "Položky se přidávají až po přidání faktury. Nejprve přidejte prázdnou fakturu, a poté do ní přidejte položky.");
            if (invoice.Paid)
                ModelState.AddModelError("", "Nelze přidat zaplacenou fakturu. Nejprve ji přidejte, a pak ji nastavte jako zaplacenou.");

            if (!ModelState.IsValid || ModelState.ErrorCount > 0)
                return View(invoice);

            _invoiceManager.Add(invoice);

            return RedirectToAction("List", "Invoice");
        }
    }
}