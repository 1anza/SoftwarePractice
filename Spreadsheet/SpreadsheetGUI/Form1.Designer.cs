
namespace SpreadsheetGUI
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
            this.components = new System.ComponentModel.Container();
            this.spreadsheetPanel1 = new SS.SpreadsheetPanel();
            this.FileDropDown = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.NewButton = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenButton = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveButton = new System.Windows.Forms.ToolStripMenuItem();
            this.CellInput = new System.Windows.Forms.TextBox();
            this.FileButton = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.CellDataText = new System.Windows.Forms.TextBox();
            this.FilenameText = new System.Windows.Forms.TextBox();
            this.HelpButton = new System.Windows.Forms.Button();
            this.FileDropDown.SuspendLayout();
            this.SuspendLayout();
            // 
            // spreadsheetPanel1
            // 
            this.spreadsheetPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.spreadsheetPanel1.BackColor = System.Drawing.Color.PeachPuff;
            this.spreadsheetPanel1.BackgroundImage = global::SpreadsheetGUI.Properties.Resources._4964650__1_;
            this.spreadsheetPanel1.Location = new System.Drawing.Point(11, 64);
            this.spreadsheetPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.spreadsheetPanel1.Name = "spreadsheetPanel1";
            this.spreadsheetPanel1.Size = new System.Drawing.Size(802, 399);
            this.spreadsheetPanel1.TabIndex = 0;
            // 
            // FileDropDown
            // 
            this.FileDropDown.AllowDrop = true;
            this.FileDropDown.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewButton,
            this.OpenButton,
            this.SaveButton});
            this.FileDropDown.Name = "FileDropDown";
            this.FileDropDown.ShowImageMargin = false;
            this.FileDropDown.Size = new System.Drawing.Size(79, 70);
            this.FileDropDown.Opening += new System.ComponentModel.CancelEventHandler(this.FileDropDown_Opening);
            // 
            // NewButton
            // 
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(78, 22);
            this.NewButton.Text = "New";
            this.NewButton.Click += new System.EventHandler(this.buttonToolStripMenuItem_Click);
            // 
            // OpenButton
            // 
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(78, 22);
            this.OpenButton.Text = "Open";
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // SaveButton
            // 
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(78, 22);
            this.SaveButton.Text = "Save";
            this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
            // 
            // CellInput
            // 
            this.CellInput.Location = new System.Drawing.Point(93, 13);
            this.CellInput.Name = "CellInput";
            this.CellInput.Size = new System.Drawing.Size(356, 20);
            this.CellInput.TabIndex = 3;
            this.CellInput.TextChanged += new System.EventHandler(this.CellInput_TextChanged);
            // 
            // FileButton
            // 
            this.FileButton.Location = new System.Drawing.Point(12, 10);
            this.FileButton.Name = "FileButton";
            this.FileButton.Size = new System.Drawing.Size(75, 23);
            this.FileButton.TabIndex = 4;
            this.FileButton.Text = "File";
            this.FileButton.UseVisualStyleBackColor = true;
            this.FileButton.Click += new System.EventHandler(this.FileButton_Click);
            // 
            // CellDataText
            // 
            this.CellDataText.Location = new System.Drawing.Point(455, 13);
            this.CellDataText.Name = "CellDataText";
            this.CellDataText.ReadOnly = true;
            this.CellDataText.Size = new System.Drawing.Size(263, 20);
            this.CellDataText.TabIndex = 5;
            // 
            // FilenameText
            // 
            this.FilenameText.Location = new System.Drawing.Point(12, 39);
            this.FilenameText.Name = "FilenameText";
            this.FilenameText.ReadOnly = true;
            this.FilenameText.Size = new System.Drawing.Size(787, 20);
            this.FilenameText.TabIndex = 7;
            // 
            // HelpButton
            // 
            this.HelpButton.Location = new System.Drawing.Point(724, 11);
            this.HelpButton.Name = "HelpButton";
            this.HelpButton.Size = new System.Drawing.Size(75, 23);
            this.HelpButton.TabIndex = 8;
            this.HelpButton.Text = "Help";
            this.HelpButton.UseVisualStyleBackColor = true;
            this.HelpButton.Click += new System.EventHandler(this.HelpButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(811, 459);
            this.Controls.Add(this.HelpButton);
            this.Controls.Add(this.FilenameText);
            this.Controls.Add(this.CellDataText);
            this.Controls.Add(this.FileButton);
            this.Controls.Add(this.CellInput);
            this.Controls.Add(this.spreadsheetPanel1);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FileDropDown.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private SS.SpreadsheetPanel spreadsheetPanel1;
        private System.Windows.Forms.ContextMenuStrip FileDropDown;
        private System.Windows.Forms.ToolStripMenuItem NewButton;
        private System.Windows.Forms.ToolStripMenuItem OpenButton;
        private System.Windows.Forms.ToolStripMenuItem SaveButton;
        private System.Windows.Forms.TextBox CellInput;
        private System.Windows.Forms.Button FileButton;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.TextBox CellDataText;
        private System.Windows.Forms.TextBox FilenameText;
        private System.Windows.Forms.Button HelpButton;
    }
}

