using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestInvoices.Api;
using TestInvoices.DbModels;

namespace TestInvoices.Managers
{
    /// <summary>
    /// upravuje objekty (např. faktury) podle modelu
    /// </summary>
    public interface IModelPatcher<TObject, TModel> where TObject : class
    {
        void Patch(TObject obj, TModel model);
    }
}
