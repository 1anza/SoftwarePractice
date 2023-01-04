using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TipCalculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ComputeBtn_Click(object sender, EventArgs e)
        {
            double total;
            double.TryParse(totalBill.Text, out total);
            double tip;
            double.TryParse(TipPercent.Text, out tip);

            tipCompute.ReadOnly = true;
            totalCompute.ReadOnly = true;

            double tipAmnt = total * tip;
            tipCompute.Text = (tipAmnt).ToString();

            totalCompute.Text = (tipAmnt + total).ToString();

        }

        private void totalBill_TextChanged(object sender, EventArgs e)
        {
            double total;
            bool tryBill = double.TryParse(totalBill.Text, out total);
            ComputeBtn.Enabled = false;

            if (tryBill)
            {
                ComputeBtn.Enabled = true;
            }
        }

        private void TipPercent_TextChanged(object sender, EventArgs e)
        {
            double tip;
            bool tryTip = double.TryParse(TipPercent.Text, out tip);
            ComputeBtn.Enabled = false;

            if (tryTip)
            {
                ComputeBtn.Enabled = true;
            }
        }
    }
}
