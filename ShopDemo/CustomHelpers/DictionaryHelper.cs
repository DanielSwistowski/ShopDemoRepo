using System.Collections.Generic;
using System.Linq;

namespace ShopDemo.CustomHelpers
{
    public static class DictionaryHelper
    {
        public static Dictionary<string, IEnumerable<string>> ToDictionary(this string queryString)
        {
            Dictionary<string, IEnumerable<string>> dictionaryParams = null;
            if (!string.IsNullOrEmpty(queryString))
            {
                string[] parameters = queryString.ToLower().Split('.');
                try
                {
                    dictionaryParams = parameters.Select(item => item.Split('-')).ToDictionary(s => s[0], s => s[1].Split('_').AsEnumerable());
                }
                catch
                {
                    dictionaryParams = new Dictionary<string, IEnumerable<string>>();
                    dictionaryParams.Add("invalidFiltr", parameters);
                }
            }
            return dictionaryParams;
        }
    }
}