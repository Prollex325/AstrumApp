using AstrumApp.Services.Updates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;
using Velopack;

namespace AstrumApp
{

    public class UpdateCoordinatorOLD
    {
        public event Action? StatusChanged;

        public UpdateStatus Status { get; private set; } = UpdateStatus.Idle;

        public UpdateInfo? AvailableUpdate { get; private set; }

        public int DownloadProgress { get; private set; }

        public string? LastError { get; private set; }

        private readonly UpdateService _service;

        public UpdateCoordinatorOLD(UpdateService service)
        {
            _service = service;
        }

        public async Task CheckAsync() 
        {
            if (Status is UpdateStatus.Checking or UpdateStatus.Downloading or UpdateStatus.Installing) return;

            Status = UpdateStatus.Checking;
            LastError = string.Empty;

            StatusChanged?.Invoke();

            try
            {
                var update = await _service.CheckAsync();

                Status = update == null ? UpdateStatus.UpToDate : UpdateStatus.Available;

                AvailableUpdate = update;
            } catch (Exception ex) {
                Status = UpdateStatus.Failed;
                LastError = ex.Message;
            }

            StatusChanged?.Invoke();
        }

        public void DownloadAsync()
        {
            // void -> Task
        }

        public void InstallAndRestart()
        {

        }
    }
}
