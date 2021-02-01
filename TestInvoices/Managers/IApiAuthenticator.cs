using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestInvoices.Managers
{
    public interface IApiAuthenticator
    {
        bool Authenticate(HttpRequest context);
    }
}
