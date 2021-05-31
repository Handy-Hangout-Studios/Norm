using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;

namespace Norm.Attributes
{
    public class SkipRunningAttribute : JobFilterAttribute, IServerFilter
    {
        public void OnPerformed(PerformedContext filterContext)
        {
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            if (filterContext.Connection is not JobStorageConnection connection || connection == null)
                return;

            string? recurringJobId = filterContext.GetJobParameter<string?>("RecurringJobId");
            if (recurringJobId == null || !recurringJobId.EndsWith("start-movie"))
                return;

            if (connection.GetValueFromHash($"recurring-job:{recurringJobId}", "skip") != "true")
            {
                filterContext.Canceled = true;
            }
        }
    }
}
