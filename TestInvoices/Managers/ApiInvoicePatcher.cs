using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using TestInvoices.Api;
using TestInvoices.DbModels;

namespace TestInvoices.Managers
{
    /// <summary>
    /// upravuje faktury podle EditInvoiceModelu
    /// </summary>
    public class ApiInvoicePatcher : IModelPatcher<Invoice, ApiEditInvoiceModel>
    {
        private readonly PropertyInfo[] _invoiceEditableProperties;
        public ApiInvoicePatcher()
        {
            _invoiceEditableProperties = typeof(Invoice).GetProperties().Where(prop => prop.GetCustomAttribute<ApiEditableAttribute>() != null).ToArray();
        }

        private JsonPatch getInvoicePatch(Invoice invoice, ApiEditInvoiceModel model)
        {
            //pokud model obsahuje json element pro změnu vlastností
            if (model.Patch is JsonElement json && json.ValueKind == JsonValueKind.Object)
            {
                var patch = new JsonPatch(invoice, json);
                patch.Prepare(); //příprava a validace (jinak výjimka
                return patch;
            }
            else
                return null;
        }

        private void validateAddedItems(Invoice invoice, ApiEditInvoiceModel model)
        {
            var list = new List<InvoiceItem>();

            if (model.AddItems == null) return;

            //validace přidávaných položek
            foreach (var item in model.AddItems)
            {
                if (item.Id != -1 && !invoice.Items.Any(invItem => invItem.Id == item.Id))
                    throw new InvalidOperationException("Faktura neobsahuje položku, kterou se snažíte upravit.");

                item.Id = -1; //nastavit id na -1, aby to InvoiceManager bral jako novou položku

                var context = new ValidationContext(item);
                Validator.ValidateObject(item, context); //validace přidávané položky (jinak výjimka)
            }
        }

        private void validateRemovedItems(Invoice invoice, ApiEditInvoiceModel model)
        {
            if (model.RemoveItems == null) return;

            //ujistit se, že všechny odstraňované položky jsou na faktuře
            foreach (var id in model.RemoveItems)
            {
                if (!invoice.Items.Any(i => i.Id == id))
                    throw new InvalidOperationException($"Neznám položku s idem {id}.");
            }
        }

        private JsonPatch[] getItemPatches(Invoice invoice, ApiEditInvoiceModel model)
        {
            if (model.PatchItems == null) return new JsonPatch[0];

            var itemPatches = new List<JsonPatch>();

            foreach (var item in model.PatchItems)
            {
                if (item is JsonElement el && el.ValueKind == JsonValueKind.Object)
                {
                    if (!el.TryGetProperty("id", out var idProp) || !idProp.TryGetInt32(out int id))
                        throw new InvalidOperationException($"Upravované položky musí obsahovat platný id.");

                    var correspondingItem = invoice.Items.FirstOrDefault(i => i.Id == id);
                    if (correspondingItem == null)
                        throw new InvalidOperationException($"Faktura neobsahuje položku s idem {id}.");

                    var patch = new JsonPatch(correspondingItem, el);
                    patch.JsonPropertyFilter = (jprop => jprop.Name.ToLower() != "id"); //chceme přeskočit vlastnost id 
                    patch.Prepare();

                    itemPatches.Add(patch);
                }
            }

            return itemPatches.ToArray();
        }

        private void addItems(Invoice invoice, ApiEditInvoiceModel model)
        {
            if (model.AddItems == null) return;

            invoice.Items.AddRange(model.AddItems);
        }

        private void removeItems(Invoice invoice, ApiEditInvoiceModel model)
        {
            if (model.RemoveItems == null) return;

            foreach (var id in model.RemoveItems)
            {
                var index = invoice.Items.FindIndex(item => item.Id == id);
                invoice.Items.RemoveAt(index);
            }
        }

        public void Patch(Invoice invoice, ApiEditInvoiceModel model)
        {
            JsonPatch invoicePatch = getInvoicePatch(invoice, model);
            var itemPatches = getItemPatches(invoice, model);
            validateAddedItems(invoice, model);            
            validateRemovedItems(invoice, model);

            invoicePatch?.Apply(); //aplikovat patch
            foreach (var ipatch in itemPatches)
                ipatch.Apply();
            addItems(invoice, model); //přidat položky
            removeItems(invoice, model); //odstranit položky
        }
    }
}
