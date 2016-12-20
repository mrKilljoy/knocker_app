using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnockerLib;

namespace KnockerWPF.ViewModels
{
    public class ParametersVM : ViewModelBase
    {
        public int NumberOfHops
        {
            get { return PointCheckSettings.TraceCheckNumberOfHops; }
            set
            {
                PointCheckSettings.TraceCheckNumberOfHops = value;
                OnPropertyChanged();
            }
        }

        public int PingCheckTimeout
        {
            get { return PointCheckSettings.PingCheckTimeout; }
            set
            {
                PointCheckSettings.PingCheckTimeout = value;
                OnPropertyChanged();
            }
        }

        public int PortCheckTimeout
        {
            get { return PointCheckSettings.PortCheckTimeout; }
            set
            {
                PointCheckSettings.PortCheckTimeout = value;
                OnPropertyChanged();
            }
        }

        public int TraceCheckTimeout
        {
            get { return PointCheckSettings.TraceCheckStepTimeout; }
            set
            {
                PointCheckSettings.TraceCheckStepTimeout = value;
                OnPropertyChanged();
            }
        }
    }
}
