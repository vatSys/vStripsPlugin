
namespace vStripsPlugin
{
    partial class SetupWindow
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
            this.storeButton = new vatsys.GenericButton();
            this.cancelButton = new vatsys.GenericButton();
            this.arrivalView = new vatsys.TreeViewEx();
            this.departureView = new vatsys.TreeViewEx();
            this.insetPanel1 = new vatsys.InsetPanel();
            this.insetPanel2 = new vatsys.InsetPanel();
            this.airportLabel = new vatsys.TextLabel();
            this.textLabel2 = new vatsys.TextLabel();
            this.textLabel3 = new vatsys.TextLabel();
            this.b_restartPlugin = new vatsys.GenericButton();
            this.l_vStripsHost = new vatsys.TextLabel();
            this.t_vStripsHostIP = new vatsys.TextField();
            this.insetPanel1.SuspendLayout();
            this.insetPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // storeButton
            // 
            this.storeButton.Enabled = false;
            this.storeButton.Font = new System.Drawing.Font("Terminus (TTF)", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.storeButton.Location = new System.Drawing.Point(93, 277);
            this.storeButton.Name = "storeButton";
            this.storeButton.Size = new System.Drawing.Size(75, 28);
            this.storeButton.SubFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.storeButton.SubText = "";
            this.storeButton.TabIndex = 0;
            this.storeButton.Text = "Store";
            this.storeButton.UseVisualStyleBackColor = true;
            this.storeButton.Click += new System.EventHandler(this.storeButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Terminus (TTF)", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.cancelButton.Location = new System.Drawing.Point(174, 277);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 28);
            this.cancelButton.SubFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.SubText = "";
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // arrivalView
            // 
            this.arrivalView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.arrivalView.CheckBoxes = true;
            this.arrivalView.Enabled = false;
            this.arrivalView.Location = new System.Drawing.Point(3, 3);
            this.arrivalView.Name = "arrivalView";
            this.arrivalView.Size = new System.Drawing.Size(108, 144);
            this.arrivalView.TabIndex = 2;
            this.arrivalView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.View_AfterCheck);
            // 
            // departureView
            // 
            this.departureView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.departureView.CheckBoxes = true;
            this.departureView.Enabled = false;
            this.departureView.Location = new System.Drawing.Point(3, 3);
            this.departureView.Name = "departureView";
            this.departureView.Size = new System.Drawing.Size(108, 144);
            this.departureView.TabIndex = 3;
            this.departureView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.View_AfterCheck);
            // 
            // insetPanel1
            // 
            this.insetPanel1.Controls.Add(this.arrivalView);
            this.insetPanel1.Location = new System.Drawing.Point(12, 53);
            this.insetPanel1.Name = "insetPanel1";
            this.insetPanel1.Size = new System.Drawing.Size(114, 150);
            this.insetPanel1.TabIndex = 4;
            // 
            // insetPanel2
            // 
            this.insetPanel2.Controls.Add(this.departureView);
            this.insetPanel2.Location = new System.Drawing.Point(135, 53);
            this.insetPanel2.Name = "insetPanel2";
            this.insetPanel2.Size = new System.Drawing.Size(114, 150);
            this.insetPanel2.TabIndex = 5;
            // 
            // airportLabel
            // 
            this.airportLabel.AutoSize = true;
            this.airportLabel.Enabled = false;
            this.airportLabel.Font = new System.Drawing.Font("Terminus (TTF)", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.airportLabel.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.airportLabel.HasBorder = false;
            this.airportLabel.InteractiveText = true;
            this.airportLabel.Location = new System.Drawing.Point(9, 9);
            this.airportLabel.Name = "airportLabel";
            this.airportLabel.Size = new System.Drawing.Size(248, 17);
            this.airportLabel.TabIndex = 6;
            this.airportLabel.Text = "No Airport selected in vStrips";
            this.airportLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textLabel2
            // 
            this.textLabel2.AutoSize = true;
            this.textLabel2.Font = new System.Drawing.Font("Terminus (TTF)", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.textLabel2.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.textLabel2.HasBorder = false;
            this.textLabel2.InteractiveText = false;
            this.textLabel2.Location = new System.Drawing.Point(12, 33);
            this.textLabel2.Name = "textLabel2";
            this.textLabel2.Size = new System.Drawing.Size(64, 17);
            this.textLabel2.TabIndex = 7;
            this.textLabel2.Text = "Arr RWY";
            this.textLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.textLabel2.Click += new System.EventHandler(this.textLabel2_Click);
            // 
            // textLabel3
            // 
            this.textLabel3.AutoSize = true;
            this.textLabel3.Font = new System.Drawing.Font("Terminus (TTF)", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.textLabel3.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.textLabel3.HasBorder = false;
            this.textLabel3.InteractiveText = false;
            this.textLabel3.Location = new System.Drawing.Point(135, 33);
            this.textLabel3.Name = "textLabel3";
            this.textLabel3.Size = new System.Drawing.Size(64, 17);
            this.textLabel3.TabIndex = 8;
            this.textLabel3.Text = "Dep RWY";
            this.textLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // b_restartPlugin
            // 
            this.b_restartPlugin.Font = new System.Drawing.Font("Terminus (TTF)", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.b_restartPlugin.Location = new System.Drawing.Point(12, 277);
            this.b_restartPlugin.Name = "b_restartPlugin";
            this.b_restartPlugin.Size = new System.Drawing.Size(75, 28);
            this.b_restartPlugin.SubFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.b_restartPlugin.SubText = "";
            this.b_restartPlugin.TabIndex = 9;
            this.b_restartPlugin.Text = "Restart";
            this.b_restartPlugin.UseVisualStyleBackColor = true;
            this.b_restartPlugin.Click += new System.EventHandler(this.restartButton_Click);
            // 
            // l_vStripsHost
            // 
            this.l_vStripsHost.AutoSize = true;
            this.l_vStripsHost.Font = new System.Drawing.Font("Terminus (TTF)", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.l_vStripsHost.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.l_vStripsHost.HasBorder = false;
            this.l_vStripsHost.InteractiveText = false;
            this.l_vStripsHost.Location = new System.Drawing.Point(12, 236);
            this.l_vStripsHost.Name = "l_vStripsHost";
            this.l_vStripsHost.Size = new System.Drawing.Size(104, 17);
            this.l_vStripsHost.TabIndex = 11;
            this.l_vStripsHost.Text = "vStrips Host";
            this.l_vStripsHost.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // t_vStripsHostIP
            // 
            this.t_vStripsHostIP.BackColor = System.Drawing.SystemColors.ControlDark;
            this.t_vStripsHostIP.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.t_vStripsHostIP.Location = new System.Drawing.Point(123, 233);
            this.t_vStripsHostIP.Name = "t_vStripsHostIP";
            this.t_vStripsHostIP.NumericCharOnly = false;
            this.t_vStripsHostIP.Size = new System.Drawing.Size(126, 25);
            this.t_vStripsHostIP.TabIndex = 12;
            this.t_vStripsHostIP.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.HostIP_onKeyup);
            this.t_vStripsHostIP.Leave += new System.EventHandler(this.storeHostIPChange);
            // 
            // SetupWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(260, 318);
            this.Controls.Add(this.t_vStripsHostIP);
            this.Controls.Add(this.l_vStripsHost);
            this.Controls.Add(this.b_restartPlugin);
            this.Controls.Add(this.textLabel3);
            this.Controls.Add(this.textLabel2);
            this.Controls.Add(this.airportLabel);
            this.Controls.Add(this.insetPanel2);
            this.Controls.Add(this.insetPanel1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.storeButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HasMinimizeButton = false;
            this.HasSendBackButton = false;
            this.MiddleClickClose = false;
            this.MinimizeBox = false;
            this.Name = "SetupWindow";
            this.Resizeable = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "vStrips INTAS Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetupWindow_FormClosing);
            this.insetPanel1.ResumeLayout(false);
            this.insetPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private vatsys.GenericButton storeButton;
        private vatsys.GenericButton cancelButton;
        private vatsys.TreeViewEx arrivalView;
        private vatsys.TreeViewEx departureView;
        private vatsys.InsetPanel insetPanel1;
        private vatsys.InsetPanel insetPanel2;
        private vatsys.TextLabel airportLabel;
        private vatsys.TextLabel textLabel2;
        private vatsys.TextLabel textLabel3;
        private vatsys.GenericButton b_restartPlugin;
        private vatsys.TextLabel l_vStripsHost;
        private vatsys.TextField t_vStripsHostIP;
    }
}