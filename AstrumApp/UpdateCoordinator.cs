using AstrumApp.Services.Updates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Velopack;

namespace AstrumApp
{
    public enum UpdateStatus
    {
        Idle,
        Checking,
        UpToDate,
        Available,
        Downloading,
        ReadyToInstall,
        Installing,
        Failed
    }

    public class UpdateCoordinator
    {
        public event Action? StatusChanged;
        public event Action? ProgressChanged;
        public UpdateStatus Status { get; private set; } = UpdateStatus.Idle;
        public string? LastError { get; private set; }
        public UpdateInfo? AvailableUpdate { get; private set; }
        public int DownloadProgress { get; private set; }
        public bool IsInstalled => _service.IsInstalled;
        public bool IsRequired { get; private set; }
        public SemanticVersion? MinimumSupportedVersion { get; private set; }
        public SemanticVersion? PendingVersion => _service.PendingVersion;

        private readonly UpdateService _service;
        private readonly UpdatePolicyService _policyService;

        public UpdateCoordinator(UpdateService service)
        {
            _service = service;
            _policyService = new();

            if (_service.IsUpdateReady)
            {
                StatusUpdate(UpdateStatus.ReadyToInstall);
                return;
            }
        }

        public async Task CheckAsync(bool force = false)
        {
            if (!force && _service.IsUpdateReady)
            {
                StatusUpdate(UpdateStatus.ReadyToInstall);
                return;
            }

            if (Status is UpdateStatus.Checking or UpdateStatus.Downloading or UpdateStatus.Installing) 
                return;

            LastError = string.Empty;

            StatusUpdate(UpdateStatus.Checking);

            try
            {
                var update = await _service.CheckAsync();

                StatusUpdate(update == null ? UpdateStatus.UpToDate : UpdateStatus.Available);

                AvailableUpdate = update;
            }
            catch (Exception ex)
            {
                StatusUpdate(UpdateStatus.Failed);
                LastError = ex.Message;
            }
        }

        public async Task DownloadAsync()
        {
            if (Status != UpdateStatus.Available) return;

            if (AvailableUpdate == null)
            {
                LastError = "Доступное обновление не найдено";
                StatusUpdate(UpdateStatus.Failed);
                return;
            }

            try
            {
                LastError = null;
                DownloadProgress = 0;
                StatusUpdate(UpdateStatus.Downloading);

                await _service.DownloadAsync(AvailableUpdate, progress =>
                {
                    DownloadProgress = progress;
                    ProgressChanged?.Invoke();
                });

                StatusUpdate(UpdateStatus.ReadyToInstall);
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                StatusUpdate(UpdateStatus.Failed);
            }
        }

        public void InstallAndRestart()
        {
            if (!_service.IsUpdateReady)
            {
                LastError = "Нет загруженного обновления для установки";
                StatusUpdate(UpdateStatus.Failed);
                return;
            }

            try
            {
                LastError = null;
                StatusUpdate(UpdateStatus.Installing);

                _service.InstallAndRestart();
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                StatusUpdate(UpdateStatus.Failed);
            }
        }
        public async Task RefreshPolicyAsync()
        {
            var policy = await _policyService.GetPolicyAsync();

            var current = _service.CurrentVersion
                ?? throw new InvalidOperationException("Не удалось определить версию Astrum.");

            var minimum = SemanticVersion.Parse(policy.MinimumSupportedVersion);

            MinimumSupportedVersion = SemanticVersion.Parse(policy.MinimumSupportedVersion);

            IsRequired = _service.CurrentVersion != null &&
                         _service.CurrentVersion.CompareTo(MinimumSupportedVersion) < 0;
        }

        private void StatusUpdate(UpdateStatus status)
        {
            Status = status;
            StatusChanged?.Invoke();
        }
    }
}
