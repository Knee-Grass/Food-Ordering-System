using System;
using System.Windows.Forms;
using FoodOrderingSystem.Forms; // Correct namespace for Forms

namespace FoodOrderingSystem
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}