using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Swapcode.EPiDebugViewLinks.Models
{
    public class DebugView
    {
        public DebugView(bool enabled)
        {
            Links = new List<DebugViewLink>(10);
            Enabled = enabled;
        }

        public List<DebugViewLink> Links { get; private set; }

        /// <summary>
        /// Is debug view enabled
        /// </summary>
        public bool Enabled { get; private set; }
    }
}