using System;
using System.Windows.Forms;

namespace GWxLauncher
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // Standard WinForms startup plumbing
            ApplicationConfiguration.Initialize();

            // This tells the app which window to open first
            Application.Run(new MainForm());
        }
    }
}
