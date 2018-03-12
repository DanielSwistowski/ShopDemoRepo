using Hangfire;
using Service_Layer.Services;
using ShopDemo.Areas.Admin;

namespace ShopDemo.App_Start
{
    public static class HangfireJobsInitializer
    {
        public static void Initialize()
        {
            RecurringJob.AddOrUpdate<ISalesSummaryService>(s => s.CalculateSalesSummaryForPreviousMonth(), "0 1 1 * *"); // start at every first day in month at 01:00 o clock
            RecurringJob.AddOrUpdate<IHangfireAutoCancelOrder>(s => s.CancelNotPaidOrders(), "0 2 * * *"); // start at every day at 02:00 o clock
            RecurringJob.AddOrUpdate<IHangfireRemovePhotoFiles>(s => s.RemovePhotoFilesWhichNotExistsIntoDb(), Cron.Weekly);
        }
    }
}