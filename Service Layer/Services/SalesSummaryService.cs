using DataAccessLayer;
using DataAccessLayer.Models;
using System;
using System.Linq;

namespace Service_Layer.Services
{
    public interface ISalesSummaryService
    {
        void CalculateSalesSummaryForPreviousMonth();
    }

    public class SalesSummaryService : ISalesSummaryService
    {
        private readonly IApplicationDbContext context;
        private readonly IDateTimeProvider dateTimeProvider;
        public SalesSummaryService(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
        {
            this.context = context;
            this.dateTimeProvider = dateTimeProvider;
        }

        public void CalculateSalesSummaryForPreviousMonth()
        {
            var date = dateTimeProvider.Now;
            int year = date.Year;
            int month = date.Month;

            //get previous month number
            if (month == 1)
            {
                month = 12;
                year = year - 1; // get previous year
            }
            else
            {
                month = month - 1;
            }

            var firstDayOfMonth = new DateTime(year, month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddTicks(-1);

            string monthName = firstDayOfMonth.ToString("MMMM");

            bool summaryExists = context.SalesSummary.Where(s => s.MonthName == monthName && s.Year == year).Any();

            if (!summaryExists)
            {
                //get orders summary where order realization date is between firstDay and lastDay
                decimal summary = context.Orders.Where(o => o.OrderRealizationDate >= firstDayOfMonth && o.OrderRealizationDate <= lastDayOfMonth).Select(p => p.TotalAmount).Sum();

                SaleSummary saleSummary = new SaleSummary();
                saleSummary.MonthName = monthName;
                saleSummary.Year = year;
                saleSummary.Summary = summary;

                context.SalesSummary.Add(saleSummary);
                context.SaveChanges();
            }
        }
    }
}