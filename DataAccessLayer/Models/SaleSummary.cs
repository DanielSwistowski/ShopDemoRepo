using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Models
{
    public class SaleSummary
    {
        public int SaleSummaryId { get; set; }

        public string MonthName { get; set; }

        public int Year { get; set; }

        public decimal Summary { get; set; }
    }
}
