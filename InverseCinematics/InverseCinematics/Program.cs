using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace InverseCinematics
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var q = new Line(3, 7, -2, -8);

            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
