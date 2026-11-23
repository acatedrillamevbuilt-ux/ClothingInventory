namespace ClothingInventory.Forms
{
    partial class StartForm
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
            lblTitle = new Label();
            btnCustomer = new Button();
            btnStaff = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(149, 72);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(136, 15);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Clothing Store Inventory";
            // 
            // btnCustomer
            // 
            btnCustomer.Location = new Point(53, 126);
            btnCustomer.Name = "btnCustomer";
            btnCustomer.Size = new Size(154, 23);
            btnCustomer.TabIndex = 1;
            btnCustomer.Text = "Customer Screen";
            btnCustomer.UseVisualStyleBackColor = true;
            // 
            // btnStaff
            // 
            btnStaff.Location = new Point(228, 126);
            btnStaff.Name = "btnStaff";
            btnStaff.Size = new Size(152, 23);
            btnStaff.TabIndex = 2;
            btnStaff.Text = "Staff Screen";
            btnStaff.UseVisualStyleBackColor = true;
            // 
            // StartForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(420, 175);
            Controls.Add(btnStaff);
            Controls.Add(btnCustomer);
            Controls.Add(lblTitle);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Name = "StartForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Clothing Store Inventory";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Button btnCustomer;
        private Button btnStaff;
    }
}