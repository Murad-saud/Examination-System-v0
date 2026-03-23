using ExamSys.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExamSys.Application.BackgroundServices
{
    public class AutoSyncExamsStateService : BackgroundService, IDisposable
    {
        private readonly ILogger<AutoSyncExamsStateService> _logger;
        private PeriodicTimer _periodicTimer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public AutoSyncExamsStateService(IServiceScopeFactory serviceScopeFactory,
            ILogger<AutoSyncExamsStateService> logger)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            try
            {
                while (await _periodicTimer.WaitForNextTickAsync(stoppingToken))
                {
                    await DoWorkyAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("SeniorBackgroundService is stopping gracefully.");
            }
            finally
            {
                _periodicTimer.Dispose();
                _logger.LogInformation("SeniorBackgroundService has stopped.");
            }
        }

        private async Task DoWorkyAsync()
        {
            // Create a new scope for each execution to resolve scoped services safely
            using var scope = _serviceScopeFactory.CreateScope();
            var scopedService = scope.ServiceProvider.GetRequiredService<IExamService>();

            try
            {
                await scopedService.SyncExamsState();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Log the error but keep the service running
                _logger.LogError(ex, "An error occurred while executing the background task. The service will continue.");

                // Optional: Add a delay before retrying after an error to avoid tight failure loops
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }


    }
}
