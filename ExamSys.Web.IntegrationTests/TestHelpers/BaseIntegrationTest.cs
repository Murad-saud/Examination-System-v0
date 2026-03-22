using ExamSys.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExamSys.IntegrationTests.Helpers
{
    public abstract class BaseIntegrationTest : IDisposable
    {
        protected readonly AppDbContext _context;
        protected readonly IServiceProvider _serviceProvider;

        protected BaseIntegrationTest()
        {
            // 1. Setup In-Memory Database
            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString())); // Unique DB per test

            // 2. Add Caching (Required for ExamTakingService)
            services.AddMemoryCache();
            services.AddLogging();

            _serviceProvider = services.BuildServiceProvider();
            _context = _serviceProvider.GetRequiredService<AppDbContext>();

            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}