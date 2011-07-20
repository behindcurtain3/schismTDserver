using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace schismTDdev
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //PlayerIO.DevelopmentServer.Server.StartWithDebugging();
            PlayerIO.DevelopmentServer.Server.StartWithDebugging("schismtd-3r3otmhvkki9ixublwca", "public", "schismTD", "bob", "", 30000);
        }
    }
}
