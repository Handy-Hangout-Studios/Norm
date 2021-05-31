using Hangfire;
using Hangfire.Storage;
using MediatR;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Services
{
    public class NormHostedService : IHostedService
    {
        private readonly BotService bot;
        private readonly IMediator mediator;
        private readonly LatexRenderService latexRenderService;

        public NormHostedService(BotService bot, IMediator mediator, LatexRenderService renderer)
        {
            this.bot = bot;
            this.mediator = mediator;
            this.latexRenderService = renderer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.mediator.Send(new Database.Requests.Database.Migrate(), cancellationToken);
            using IStorageConnection connection = JobStorage.Current.GetConnection();
            foreach (RecurringJobDto rJob in connection.GetRecurringJobs())
            {
                RecurringJob.RemoveIfExists(rJob.Id);
            }
            await this.bot.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.bot.StopAsync();
            this.latexRenderService.Dispose();
        }
    }
}
