using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestInvoices.Managers
{
    /// <summary>
    /// Kontroluje header "Authorization" vůči pevně zakódovanému klíči (API_KEY).
    /// </summary>
    public class MockApiAuthenticator : IApiAuthenticator
    {
        public const string API_COOKIE = "vidmartin_invoice_api_key";
        public static string API_KEY { get; private set; }

        public MockApiAuthenticator(IConfiguration config)
        {
            API_KEY = config.GetValue<string>("apiKey") ?? throw new InvalidOperationException("Není zadán klíč k api!");
        }

        public bool Authenticate(HttpRequest request)
        {
            StringValues token;
            if (!request.Headers.TryGetValue("Authorization", out token))
                return tryAuthenticateWithCookies(request);
            if (token.Count != 1)
                return tryAuthenticateWithCookies(request);
            return token[0] == API_KEY; //autentifikace proběhla úspěšně právě pokud se hodnota rovná klíči
        }

        private bool tryAuthenticateWithCookies(HttpRequest request)
        {
            return request.Cookies.TryGetValue(API_COOKIE, out string val) && val == API_KEY;
        }
    }
}
