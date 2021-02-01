using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace TestInvoices.Api
{
    /// <summary>
    /// umí rekurzivně upravovat objekty podle jsonu; nejdřív se ujistí, že json data jsou platná => mělo by se jednat o atomickou operaci
    /// </summary>
    public class JsonPatch
    {
        private readonly Dictionary<string, PropertyInfo> _editableProps;
        private readonly object _obj;
        private readonly JsonElement _json;
        private readonly List<JsonPatch> _children = new List<JsonPatch>();
        private bool _isPrepared = false;

        private Func<JsonProperty, bool> _jsonPropertyFilter = new Func<JsonProperty, bool>(_ => true);
        public Func<JsonProperty, bool> JsonPropertyFilter
        {
            get => _jsonPropertyFilter;
            set
            {
                if (_isPrepared)
                    throw new InvalidOperationException("PropertyFilter nelze nastavit po zavolání Prepare()");
                _jsonPropertyFilter = value;
            }
        }

        public bool RespectDataAnnotations { get; set; } = true;

        public JsonPatch(object obj, JsonElement json)
        {
            _obj = obj;
            _json = json;

            //zjistit editovatelné vlastnosti objektu
            //POZOR! nastavování je case-insensitive => nepodporuje to typy, které mají více veřejných vlastností se stejným jménem akorát s jinou velikostí písmen
            var type = obj.GetType();
            var props = type.GetProperties();
            _editableProps = props
                .Where(prop => prop.GetCustomAttribute<ApiEditableAttribute>() != null)
                .ToDictionary(p => p.Name.ToLower());
        }

        private bool tryGet(Type type, JsonElement element, out object o)
        {
            if (type == typeof(string))
                o = element.GetString();
            else if (type == typeof(bool))
                o = element.GetBoolean();
            else if (type == typeof(int))
                o = element.GetInt32();
            else if (type == typeof(long))
                o = element.GetInt64();
            else if (type == typeof(float))
                o = element.GetSingle();
            else if (type == typeof(double))
                o = element.GetDouble();
            else if (type == typeof(decimal))
                o = element.GetDecimal();
            else if (type == typeof(DateTime))
                o = element.GetDateTime();
            else if (type == typeof(Array))
            {
                var eltype = type.GetElementType();
                try
                {
                    o = element.EnumerateArray().Select(el =>
                    {
                        if (tryGet(eltype, el, out object item))
                            return item;
                        else
                            throw new Exception();
                    });
                }
                catch
                {
                    o = null;
                }
            }
            else
            {
                o = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// ujistí se, že operaci lze provést (např. že nechceme nastavit vlastnost, kterou nastavit nelze)
        /// </summary>
        public void Prepare()
        {
            var validationContext = RespectDataAnnotations ? new ValidationContext(_obj) : null;

            //ujistit se, že všechny klíče v json elementu jsou editovatelné
            foreach (var jprop in _json.EnumerateObject().Where(JsonPropertyFilter))
            {
                if (!_editableProps.TryGetValue(jprop.Name.ToLower(), out PropertyInfo prop)) //pokud odpovídající vlastnost není ve slovníku, hodit výjimku
                    throw new InvalidOperationException($"Nelze nastavit vlastnost {jprop.Name}.");

                //pokud se jedná o typ objekt, přidat potomka a rovnou ho připravit
                if (jprop.Value.ValueKind == JsonValueKind.Object)
                {
                    var child = new JsonPatch(_editableProps[jprop.Name.ToLower()].GetValue(_obj), jprop.Value);
                    child.Prepare();
                    _children.Add(child);
                }
                else if (!tryGet(prop.PropertyType, jprop.Value, out object val)) //pokud json hodnota neodpovídá typu vlastnosti, hodit výjimku
                    throw new InvalidOperationException($"Nelze nastavit vlastnost {jprop.Name}.");
                else if (RespectDataAnnotations)
                {
                    //validovat vlastnost pomocí DataAnnotations
                    validationContext.MemberName = prop.Name;  
                    Validator.ValidateProperty(val, validationContext); 
                }
                
            }

            _isPrepared = true;
        }

        /// <summary>
        /// rekurzivně uplatní změny
        /// </summary>
        public void Apply()
        {
            if (!_isPrepared)
                Prepare();

            foreach (var jprop in _json.EnumerateObject().Where(JsonPropertyFilter))
            {
                if (jprop.Value.ValueKind != JsonValueKind.Object)
                {
                    var prop = _editableProps[jprop.Name.ToLower()];
                    tryGet(prop.PropertyType, jprop.Value, out object val);
                    prop.SetValue(_obj, val);
                }
            }

            foreach (var child in _children)
            {
                child.Apply();
            }
        }
    }
}
