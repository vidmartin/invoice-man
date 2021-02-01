using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestInvoices.Utils
{
    public static class StringUtils
    {
        public static string NumberSeparate(string number, int digitsPerGroup, char separator = ' ')
        {
            string result = "";
            int counter = 0;
            foreach (char ch in number)
            {
                result += ch;
                if (char.IsDigit(ch) && (++counter) >= digitsPerGroup)
                {
                    result += separator;
                    counter = 0;
                }
            }
            return result;
        }
    }
}
