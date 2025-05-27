using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Para.UI
{
    /// <summary>
    /// Provide a rhythm synced control that can be used to trigger animations or updates!
    /// </summary>
    public class BeatSyncedControl : System.Windows.Controls.Control
    {
        /// <summary>
        /// Logged control llist for triggering beat events globally.
        /// </summary>
        protected static HashSet<BeatSyncedControl> _instances = [];

        public BeatSyncedControl()
        {
            _instances.Add(this);
        }

        ~BeatSyncedControl()
        {
            _instances.Remove(this);
        }

        /// <summary>
        /// Triggers a beat event for all instances of BeatSyncedControl.
        /// </summary>
        /// <param name="interval"></param>
        public static void GlobalTriggerBeat(double interval)
        {
            foreach (var instance in _instances)
            {
                instance.OnBeat(interval);
            }
        }

        //For instances

        /// <summary>
        /// On every beat, this method will be called.
        /// </summary>
        /// <param name="interval"></param>
        public virtual void OnBeat(double interval)
        {

        }
    }
}
