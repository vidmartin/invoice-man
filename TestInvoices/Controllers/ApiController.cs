using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TestInvoices.Api;
using TestInvoices.DbModels;
using TestInvoices.Managers;

namespace TestInvoices.Controllers
{
    [ApiController]
    [Route("{controller}/{action}/{id?}")]
    public class ApiController : Controller
    {
        private readonly IInvoiceManager _invoiceManager;
        private readonly IApiAuthenticator _apiAuthenticator;
        private readonly IModelPatcher<Invoice, ApiEditInvoiceModel> _invoicePatcher;

        public ApiController(IInvoiceManager invoiceManager, IApiAuthenticator apiAuthenticator,
            IModelPatcher<Invoice, ApiEditInvoiceModel> invoicePatcher)
        {
            this._invoiceManager = invoiceManager;
            this._apiAuthenticator = apiAuthenticator;
            this._invoicePatcher = invoicePatcher;
        }

        private static readonly Type[] API_CATCH_EXCEPTIONS = new Type[]
        {
            typeof(InvalidOperationException),
            typeof(ValidationException)  ,
            typeof(FormatException)
        };

        /// <summary>
        /// chytat dané typy výjimek a udělat z nich json odpověď
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            if (context.Exception != null)
            {
                var exType = context.Exception.GetType();
                if (API_CATCH_EXCEPTIONS.Any(type => exType == type || exType.IsSubclassOf(type)))
                {
                    context.Result = Problem(detail: context.Exception.Message);
                    context.ExceptionHandled = true;
                }
            }  
        }

        /// <summary>
        /// vrátí seznam faktur; možno filtrovat podle toho, jestli byly zaplacené
        /// </summary>
        /// <param name="paid"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult List([FromQuery] bool? paid = null)
        {
            //pokud má parametr paid nějakou hodnotu, najdeme pouze faktury, které mu odpovídají
            //jinak berem všechny
            var invoices = paid.HasValue ?
                _invoiceManager.GetWhere(inv => inv.Paid == paid.Value) :
                _invoiceManager.GetWhere(_ => true);

            return new JsonResult(invoices); //TODO: určit, které vlastnosti faktury se serializují
        }

        /// <summary>
        /// nastaví fakturu s daným id na zaplacenou
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NeedApiKey(isApi: true)]
        [HttpGet]
        public IActionResult Pay(int id)
        {
            var invoice = _invoiceManager.GetById(id);
            if (invoice == null)
                throw new InvalidOperationException("Faktura s daným id nenalezena."); 
            if (invoice.Paid)
                throw new InvalidOperationException("Faktura byla již zaplacena."); //opatrnosti není nikdy dost
            invoice.Paid = true;
            _invoiceManager.Update(id, invoice);
            return Ok();
        }        
        
        /// <summary>
        /// upravit fakturu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [NeedApiKey(isApi: true)]
        [HttpPatch]
        public IActionResult Patch([FromRoute] int? id, [FromBody] ApiEditInvoiceModel model)
        {
            if (!id.HasValue)
                throw new InvalidOperationException("Musí být zadáno id.");

            var invoice = _invoiceManager.GetById(id.Value);
            if (invoice == null)
                throw new InvalidOperationException("Faktura s daným id nenalezena.");
            _invoicePatcher.Patch(invoice, model);
            _invoiceManager.Update(invoice.Id, invoice);
            return Ok();
        }
    }
}
