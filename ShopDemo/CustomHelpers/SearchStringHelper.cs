using System.Collections.Generic;
using System.Linq;

namespace ShopDemo.CustomHelpers
{
    public static class SearchStringHelper
    {
        public static string ToSearchString(this Dictionary<string, IEnumerable<string>> dictionary)
        {
            string searchString = "";

            if (dictionary != null)
            {
                var dictionaryKeys = dictionary.Keys.ToArray();

                for (int i = 0; i < dictionaryKeys.Count(); i++)
                {
                    string paramName = "";
                    if (i == 0)
                        paramName = dictionaryKeys[i] + "-";
                    else
                        paramName = "." + dictionaryKeys[i] + "-";

                    string paramValue = "";
                    var dictionaryValue = dictionary[dictionaryKeys[i]].ToArray();
                    for (int j = 0; j < dictionaryValue.Count(); j++)
                    {
                        if (j == dictionaryValue.Count() - 1)
                            paramValue += dictionaryValue[j];
                        else
                            paramValue += dictionaryValue[j] + "_";
                    }
                    searchString += paramName + paramValue;
                }
            }

            return searchString.ToLower();
        }
    }
}