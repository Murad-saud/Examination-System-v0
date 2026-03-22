using ExamSys.Application.BackgroundServices;
using ExamSys.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace ExamSys.Tests.Background
{
    public class AutoSyncExamsStateServiceTests
    {
        private readonly Mock<IServiceScopeFactory> _scopeFactoryMock = new();
        private readonly Mock<IServiceScope> _scopeMock = new();
        private readonly Mock<IServiceProvider> _serviceProviderMock = new();
        private readonly Mock<IExamService> _examServiceMock = new();
        private readonly Mock<ILogger<AutoSyncExamsStateService>> _loggerMock = new();

        private AutoSyncExamsStateService CreateService()
        {
            _scopeFactoryMock
                .Setup(f => f.CreateScope())
                .Returns(_scopeMock.Object);

            _scopeMock
                .Setup(s => s.ServiceProvider)
                .Returns(_serviceProviderMock.Object);

            _serviceProviderMock
                .Setup(sp => sp.GetService(typeof(IExamService)))
                .Returns(_examServiceMock.Object);

            return new AutoSyncExamsStateService(
                _scopeFactoryMock.Object,
                _loggerMock.Object
            );
        }

        // =========================
        // Main Test 1
        // =========================
        [Fact]
        public async Task DoWorkyAsync_WhenCalled_CallsSyncExamsState()
        {
            // Arrange
            var service = CreateService();

            _examServiceMock
                .Setup(s => s.SyncExamsState())
                .Returns(Task.CompletedTask);

            // Act
            var method = typeof(AutoSyncExamsStateService)
                .GetMethod("DoWorkyAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            await (Task)method.Invoke(service, null);

            // Assert
            _examServiceMock.Verify(s => s.SyncExamsState(), Times.Once);
        }

        // =========================
        // Main Test 2
        // =========================
        [Fact]
        public async Task DoWorkyAsync_WhenSyncThrows_ExceptionIsHandled()
        {
            // Arrange
            var service = CreateService();

            _examServiceMock
                .Setup(s => s.SyncExamsState())
                .ThrowsAsync(new Exception("DB error"));

            // Act
            var method = typeof(AutoSyncExamsStateService)
                .GetMethod("DoWorkyAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var exception = await Record.ExceptionAsync(() =>
                (Task)method.Invoke(service, null));

            // Assert
            Assert.Null(exception); // Service should NOT crash
        }

        // =========================
        // Edge Case
        // =========================
        [Fact]
        public async Task DoWorkyAsync_CreatesNewServiceScope()
        {
            // Arrange
            var service = CreateService();

            _examServiceMock
                .Setup(s => s.SyncExamsState())
                .Returns(Task.CompletedTask);

            // Act
            var method = typeof(AutoSyncExamsStateService)
                .GetMethod("DoWorkyAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            await (Task)method.Invoke(service, null);

            // Assert
            _scopeFactoryMock.Verify(f => f.CreateScope(), Times.Once);
        }
    }
}
