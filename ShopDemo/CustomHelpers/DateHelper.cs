using System.Collections.Generic;

namespace ShopDemo.CustomHelpers
{
    public static class DateHelper
    {
        public static int GetMonthNumber(string monthName)
        {
            Dictionary<string, int> monthsDictionary = new Dictionary<string, int>();
            monthsDictionary.Add("styczeń", 1);
            monthsDictionary.Add("luty", 2);
            monthsDictionary.Add("marzec", 3);
            monthsDictionary.Add("kwiecień", 4);
            monthsDictionary.Add("maj", 5);
            monthsDictionary.Add("czerwiec", 6);
            monthsDictionary.Add("lipiec", 7);
            monthsDictionary.Add("sierpień", 8);
            monthsDictionary.Add("wrzesień", 9);
            monthsDictionary.Add("październik", 10);
            monthsDictionary.Add("listopad", 11);
            monthsDictionary.Add("grudzień", 12);

            return monthsDictionary[monthName];
        }
    }
}