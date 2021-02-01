using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestInvoices.Managers;

namespace TestInvoices.Api
{
    /// <summary>
    /// akce označená tímto atributem bude kontrolovat klíč k api, než se vyhodnotí; pokud
    /// klíč v requestu není nebo je nesprávný, vrátí buďto json chybu (jestliže isApi == true)
    /// nebo přesměruje klienta na stránku s chybovou hláškou (jestliže isApi == false)
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class NeedApiKeyAttribute : ActionFilterAttribute
    {
        private readonly bool _isApi; //zdali se jedná o akci v rámci api, nebo o normální mvc

        public NeedApiKeyAttribute(bool isApi)
        {
            this._isApi = isApi;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var apiAuth = context.HttpContext.RequestServices.GetService<IApiAuthenticator>();

            //pokud request neodpovídá bezpečnostním pravidlům, pošleme klienta do háje
            if (!apiAuth.Authenticate(context.HttpContext.Request))
            {
                var ctrl = (context.Controller as Controller);
                if (_isApi)
                    context.Result = ctrl.Unauthorized();
                else
                    context.Result = ctrl.RedirectToAction("Fail", "Home", new { message = "Špatný klíč!" });
            }
        }
    }
}
