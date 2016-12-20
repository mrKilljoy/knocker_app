using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockerLib
{
    public static class PointCheckSettings
    {
        public static int TraceCheckNumberOfHops { get; set; } = 30;

        public static int TraceCheckStepTimeout { get; set; } = 3000;

        public static int PortCheckTimeout { get; set; } = 1500;

        public static int PingCheckTimeout { get; set; } = 4000;
    }
}
