
using System;
using System.Threading.Tasks;
using System.Windows;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace AstrumApp.Services.Updates
{
    public class UpdateService
    {

        private readonly UpdateManager _manager;

        public bool IsInstalled => _manager.IsInstalled;

        public SemanticVersion? CurrentVersion => _manager.CurrentVersion;

        public bool IsUpdateReady => _manager.UpdatePendingRestart != null;

        public UpdateService(IUpdateSource source)
        {
            #if DEBUG
                var locator = new TestVelopackLocator(
                    appId: "AstrumApp.Test",
                    version: "0.1.3",
                    packagesDir: @"D:\AstrumVelopackTest\debug-packages"
                );

                _manager = new UpdateManager(source, null, locator);
            #else
                _manager = new UpdateManager(source);
            #endif
        }

        public Task<UpdateInfo?> CheckAsync() => _manager.CheckForUpdatesAsync();

        public Task DownloadAsync(UpdateInfo update, Action<int>? progress = null, CancellationToken cancellationToken = default)
        {
            return _manager.DownloadUpdatesAsync(update, progress, cancellationToken);
        }

        public void InstallAndRestart()
        {
            var update = _manager.UpdatePendingRestart;

            if (update == null)
                throw new InvalidOperationException(
                    "Нет загруженного обновления для установки."
                );

            _manager.ApplyUpdatesAndRestart(update);
        }
    }
}