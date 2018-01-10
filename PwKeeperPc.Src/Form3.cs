using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PwKeeperPC
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            //lVer.Text = Program.mVersion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string url = "";
            string business = "denvg67@gmail.com";  // your paypal email
            string description = "PwKeeper%20donation";            // '%20' represents a space. remember HTML!
            string country = "RU";                  // AU, US, etc.
            string currency = "USD";                 // AUD, USD, etc.

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            System.Diagnostics.Process.Start(url);
        }

        private void label10_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Den-W/PwKeeper.git");
        }
     
    }
}
