using System;
using System.Windows.Forms;

namespace ModUpdater
{
    public class Program
    {
        [STAThread]
        public static void Main(String[] args)
        {
            Application.Run(new ModUpdaterForm());
        }
    }
}