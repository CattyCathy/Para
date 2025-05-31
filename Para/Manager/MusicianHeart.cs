using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Para.Core
{
    /// <summary>
    /// Musician heart makes the program able to dance with a synced rhythm.
    /// </summary>
    class MusicianHeart
    {
        /// <summary>
        /// Artificial heart beat interval in seconds. For debugging or when no media source.
        /// </summary>
        public double _artificialInterval { get; set; } = 0.333;

        private CancellationTokenSource? _cts;

        public void HeartBeat(double interval)
        {
            Para.UI.BeatSyncedControl.GlobalTriggerBeat(interval);
        }

        /// <summary>
        /// If there's no media source, use it to provide rhythm.
        /// </summary>
        public void StartArtificialHeartBeat()
        {
            if (_cts != null)
                return; // Already running

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    HeartBeat(_artificialInterval);
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(_artificialInterval), token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, token);
        }

        /// <summary>
        /// Stop no media rhythm.
        /// </summary>
        public void StopArtificialHeartBeat()
        {
            _cts?.Cancel();
            _cts = null;
        }
    }
}
