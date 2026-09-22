using StockLib.PublicService;

namespace TeleStock
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ITeleStockService _teleService;

        public Worker(ILogger<Worker> logger,
                    ITeleStockService teleService)
        {
            _logger = logger;
            _teleService = teleService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _teleService.BotSyncUpdate();
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
