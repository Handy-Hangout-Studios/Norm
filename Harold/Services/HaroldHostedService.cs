using Hangfire;
using Hangfire.Storage;
using Harold.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harold.Services
{
    public class HaroldHostedService : IHostedService
    {
        private readonly IBotService bot;
        private readonly BotPsqlContext psqlContext;

        public HaroldHostedService(IBotService bot, BotPsqlContext psqlContext)
        {
            this.bot = bot;
            this.psqlContext = psqlContext;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await psqlContext.Database.MigrateAsync(cancellationToken);
            using IStorageConnection connection = JobStorage.Current.GetConnection();
            foreach (RecurringJobDto rJob in StorageConnectionExtensions.GetRecurringJobs(connection))
            {
                RecurringJob.RemoveIfExists(rJob.Id);
            }
            await bot.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await bot.StopAsync();
        }
    }
}
