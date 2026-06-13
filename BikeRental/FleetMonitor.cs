using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BikeRental
{
    /// <summary>
    /// Monitors the data folder for file changes and triggers a callback when detected.
    /// Uses CancellationToken for graceful shutdown in .NET 5+.
    /// </summary>
    internal sealed class FleetMonitor : IDisposable
    {
        private readonly string _dataFolder;
        private readonly Action? _onDataChanged;
        private CancellationTokenSource? _cancellation;
        private Task? _monitorTask;
        private DateTime _lastChecked;

        public FleetMonitor(string dataFolder, Action? onDataChanged = null)
        {
            _dataFolder = dataFolder;
            _onDataChanged = onDataChanged;
        }

        /// <summary>
        /// Starts the background monitor thread.
        /// </summary>
        public void Start()
        {
            if (_cancellation != null)
                throw new InvalidOperationException("Monitor is already running.");

            _lastChecked = DateTime.UtcNow;
            _cancellation = new CancellationTokenSource();
            _monitorTask = Task.Run(() => PollAsync(_cancellation.Token), _cancellation.Token);
        }

        /// <summary>
        /// Monitors the data folder for file changes.
        /// </summary>
        private async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(10000, cancellationToken);

                    try
                    {
                        foreach (var file in Directory.GetFiles(_dataFolder))
                        {
                            if (File.GetLastWriteTimeUtc(file) > _lastChecked)
                            {
                                _lastChecked = DateTime.UtcNow;
                                _onDataChanged?.Invoke();
                                break;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected during shutdown
                        throw;
                    }
                    catch
                    {
                        // Swallow filesystem hiccups; keep monitoring
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
        }

        /// <summary>
        /// Stops the background monitor and releases resources.
        /// </summary>
        public void Dispose()
        {
            _cancellation?.Cancel();
            try
            {
                _monitorTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            _cancellation?.Dispose();
            _monitorTask?.Dispose();
        }
    }
}
