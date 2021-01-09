using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winform主体
{
    public partial class Form1 : Form
    {
        ProcessHelper myProcess = null;
        public Form1()
        {
            InitializeComponent();
            //this.TopLevel = false;
            //this.FormBorderStyle = FormBorderStyle.None;
            myProcess = new ProcessHelper(BrowserPanel, "");
            myProcess.Start("C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe");
            this.FormClosing += Form1_FormClosing;
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            myProcess.Stop();
        }
    }
}
