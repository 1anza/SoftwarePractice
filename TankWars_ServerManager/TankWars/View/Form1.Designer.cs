
namespace TankWars
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
            this.serverLabel = new System.Windows.Forms.Label();
            this.nameText = new System.Windows.Forms.Label();
            this.serverText = new System.Windows.Forms.TextBox();
            this.playerNameText = new System.Windows.Forms.TextBox();
            this.connectBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // serverLabel
            // 
            this.serverLabel.AutoSize = true;
            this.serverLabel.Location = new System.Drawing.Point(13, 13);
            this.serverLabel.Name = "serverLabel";
            this.serverLabel.Size = new System.Drawing.Size(42, 13);
            this.serverLabel.TabIndex = 0;
            this.serverLabel.Text = "server: ";
            // 
            // nameText
            // 
            this.nameText.AutoSize = true;
            this.nameText.Location = new System.Drawing.Point(225, 13);
            this.nameText.Name = "nameText";
            this.nameText.Size = new System.Drawing.Size(39, 13);
            this.nameText.TabIndex = 1;
            this.nameText.Text = "name: ";
            // 
            // serverText
            // 
            this.serverText.Location = new System.Drawing.Point(61, 6);
            this.serverText.Name = "serverText";
            this.serverText.Size = new System.Drawing.Size(158, 20);
            this.serverText.TabIndex = 2;
            this.serverText.Text = "localhost";
            // 
            // playerNameText
            // 
            this.playerNameText.Location = new System.Drawing.Point(270, 6);
            this.playerNameText.Name = "playerNameText";
            this.playerNameText.Size = new System.Drawing.Size(162, 20);
            this.playerNameText.TabIndex = 3;
            this.playerNameText.Text = "player";
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(438, 4);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(75, 23);
            this.connectBtn.TabIndex = 4;
            this.connectBtn.Text = "connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.connectBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.playerNameText);
            this.Controls.Add(this.serverText);
            this.Controls.Add(this.nameText);
            this.Controls.Add(this.serverLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label serverLabel;
        private System.Windows.Forms.Label nameText;
        private System.Windows.Forms.TextBox serverText;
        private System.Windows.Forms.TextBox playerNameText;
        private System.Windows.Forms.Button connectBtn;
    }
}

