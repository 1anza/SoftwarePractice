
namespace TipCalculator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.EnterBillLabel = new System.Windows.Forms.Label();
            this.totalBill = new System.Windows.Forms.TextBox();
            this.tipCompute = new System.Windows.Forms.TextBox();
            this.ComputeBtn = new System.Windows.Forms.Button();
            this.tipLabel = new System.Windows.Forms.Label();
            this.TipPercent = new System.Windows.Forms.TextBox();
            this.totalTipLabel = new System.Windows.Forms.Label();
            this.totalCompute = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // EnterBillLabel
            // 
            this.EnterBillLabel.AutoSize = true;
            this.EnterBillLabel.Location = new System.Drawing.Point(198, 76);
            this.EnterBillLabel.Name = "EnterBillLabel";
            this.EnterBillLabel.Size = new System.Drawing.Size(75, 13);
            this.EnterBillLabel.TabIndex = 0;
            this.EnterBillLabel.Text = "Enter Total Bill";
            // 
            // totalBill
            // 
            this.totalBill.Location = new System.Drawing.Point(311, 76);
            this.totalBill.Name = "totalBill";
            this.totalBill.Size = new System.Drawing.Size(100, 20);
            this.totalBill.TabIndex = 1;
            this.totalBill.TextChanged += new System.EventHandler(this.totalBill_TextChanged);
            // 
            // tipCompute
            // 
            this.tipCompute.Location = new System.Drawing.Point(311, 175);
            this.tipCompute.Name = "tipCompute";
            this.tipCompute.Size = new System.Drawing.Size(100, 20);
            this.tipCompute.TabIndex = 2;
            // 
            // ComputeBtn
            // 
            this.ComputeBtn.Location = new System.Drawing.Point(198, 218);
            this.ComputeBtn.Name = "ComputeBtn";
            this.ComputeBtn.Size = new System.Drawing.Size(75, 23);
            this.ComputeBtn.TabIndex = 3;
            this.ComputeBtn.Text = "Compute";
            this.ComputeBtn.UseVisualStyleBackColor = true;
            this.ComputeBtn.Click += new System.EventHandler(this.ComputeBtn_Click);
            // 
            // tipLabel
            // 
            this.tipLabel.AutoSize = true;
            this.tipLabel.Location = new System.Drawing.Point(198, 126);
            this.tipLabel.Name = "tipLabel";
            this.tipLabel.Size = new System.Drawing.Size(62, 13);
            this.tipLabel.TabIndex = 4;
            this.tipLabel.Text = "Tip Percent";
            // 
            // TipPercent
            // 
            this.TipPercent.Location = new System.Drawing.Point(311, 126);
            this.TipPercent.Name = "TipPercent";
            this.TipPercent.Size = new System.Drawing.Size(100, 20);
            this.TipPercent.TabIndex = 5;
            this.TipPercent.TextChanged += new System.EventHandler(this.TipPercent_TextChanged);
            // 
            // totalTipLabel
            // 
            this.totalTipLabel.AutoSize = true;
            this.totalTipLabel.Location = new System.Drawing.Point(198, 175);
            this.totalTipLabel.Name = "totalTipLabel";
            this.totalTipLabel.Size = new System.Drawing.Size(49, 13);
            this.totalTipLabel.TabIndex = 6;
            this.totalTipLabel.Text = "Total Tip";
            // 
            // totalCompute
            // 
            this.totalCompute.Location = new System.Drawing.Point(311, 220);
            this.totalCompute.Name = "totalCompute";
            this.totalCompute.Size = new System.Drawing.Size(100, 20);
            this.totalCompute.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.totalCompute);
            this.Controls.Add(this.totalTipLabel);
            this.Controls.Add(this.TipPercent);
            this.Controls.Add(this.tipLabel);
            this.Controls.Add(this.ComputeBtn);
            this.Controls.Add(this.tipCompute);
            this.Controls.Add(this.totalBill);
            this.Controls.Add(this.EnterBillLabel);
            this.Name = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label EnterBillLabel;
        private System.Windows.Forms.TextBox totalBill;
        private System.Windows.Forms.TextBox tipCompute;
        private System.Windows.Forms.Button ComputeBtn;
        private System.Windows.Forms.Label tipLabel;
        private System.Windows.Forms.TextBox TipPercent;
        private System.Windows.Forms.Label totalTipLabel;
        private System.Windows.Forms.TextBox totalCompute;
    }
}

