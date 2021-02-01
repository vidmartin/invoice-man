using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestInvoices.Api
{
    /// <summary>
    /// Označuje vlastnosti faktury, které lze editovat přes api.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class ApiEditableAttribute : Attribute { }
}
