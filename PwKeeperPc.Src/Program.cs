using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;


namespace PwKeeperPC
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string sl = "";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                sl = System.Globalization.CultureInfo.CurrentUICulture.Parent.Name;

                if ("ru en".IndexOf(sl) < 0) sl = "en";
            
                System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(sl);
                System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo(sl);
            }
            catch (CultureNotFoundException)
            {
                MessageBox.Show("Language not available [" + sl + "]", "",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }

            Application.Run(new Form1());
        }
    }
}
