using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Para.UI
{
    public class BeatSyncedControl : System.Windows.Controls.Control
    {
        protected static HashSet<BeatSyncedControl> _instances = [];

        public BeatSyncedControl()
        {
            _instances.Add(this);
        }

        ~BeatSyncedControl()
        {
            _instances.Remove(this);
        }


        public static void GlobalTriggerBeat(double interval)
        {
            foreach (var instance in _instances)
            {
                instance.OnBeat(interval);
            }
        }

        public virtual void OnBeat(double interval)
        {

        }
    }
}
