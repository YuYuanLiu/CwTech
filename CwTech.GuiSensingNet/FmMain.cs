using CwTech.GuiSensingNet.SignalChart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CwTech.GuiSensingNet
{
    public partial class FmMain : Form
    {
        public FmMain()
        {
            InitializeComponent();
        }

        private void tsmiChart_Click(object sender, EventArgs e)
        {
            var fm = new FmSignalChart();
            fm.MdiParent = this;
            fm.Show();
        }
    }
}
