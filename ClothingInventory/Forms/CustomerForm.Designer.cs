namespace ClothingInventory.Forms
{
    partial class CustomerForm
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
            label1 = new Label();
            txtSearch = new TextBox();
            btnSearch = new Button();
            lstResults = new ListBox();
            grpDetails = new GroupBox();
            lblStock = new Label();
            lblPrice = new Label();
            lblLocation = new Label();
            lblColor = new Label();
            lblSize = new Label();
            lblName = new Label();
            btnConfirm = new Button();
            lblStatus = new Label();
            label2 = new Label();
            nudQuantity = new NumericUpDown();
            btnCancel = new Button();
            grpDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 27);
            label1.Name = "label1";
            label1.Size = new Size(69, 15);
            label1.TabIndex = 0;
            label1.Text = "Item Name:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(93, 24);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(312, 23);
            txtSearch.TabIndex = 1;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(429, 27);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Search ";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // lstResults
            // 
            lstResults.FormattingEnabled = true;
            lstResults.Location = new Point(346, 97);
            lstResults.Name = "lstResults";
            lstResults.Size = new Size(360, 259);
            lstResults.TabIndex = 3;
            // 
            // grpDetails
            // 
            grpDetails.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpDetails.Controls.Add(lblStock);
            grpDetails.Controls.Add(lblPrice);
            grpDetails.Controls.Add(lblLocation);
            grpDetails.Controls.Add(lblColor);
            grpDetails.Controls.Add(lblSize);
            grpDetails.Controls.Add(lblName);
            grpDetails.Location = new Point(18, 87);
            grpDetails.Name = "grpDetails";
            grpDetails.Size = new Size(295, 269);
            grpDetails.TabIndex = 4;
            grpDetails.TabStop = false;
            grpDetails.Text = "Item Details";
            // 
            // lblStock
            // 
            lblStock.AutoSize = true;
            lblStock.Location = new Point(21, 150);
            lblStock.Name = "lblStock";
            lblStock.Size = new Size(36, 15);
            lblStock.TabIndex = 5;
            lblStock.Text = "Stock";
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Location = new Point(21, 180);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(33, 15);
            lblPrice.TabIndex = 4;
            lblPrice.Text = "Price";
            // 
            // lblLocation
            // 
            lblLocation.AutoSize = true;
            lblLocation.Location = new Point(21, 118);
            lblLocation.Name = "lblLocation";
            lblLocation.Size = new Size(53, 15);
            lblLocation.TabIndex = 3;
            lblLocation.Text = "Location";
            // 
            // lblColor
            // 
            lblColor.AutoSize = true;
            lblColor.Location = new Point(24, 92);
            lblColor.Name = "lblColor";
            lblColor.Size = new Size(36, 15);
            lblColor.TabIndex = 2;
            lblColor.Text = "Color";
            // 
            // lblSize
            // 
            lblSize.AutoSize = true;
            lblSize.Location = new Point(22, 64);
            lblSize.Name = "lblSize";
            lblSize.Size = new Size(27, 15);
            lblSize.TabIndex = 1;
            lblSize.Text = "Size";
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Location = new Point(21, 40);
            lblName.Name = "lblName";
            lblName.Size = new Size(39, 15);
            lblName.TabIndex = 0;
            lblName.Text = "Name";
            // 
            // btnConfirm
            // 
            btnConfirm.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnConfirm.Location = new Point(515, 373);
            btnConfirm.Name = "btnConfirm";
            btnConfirm.Size = new Size(110, 23);
            btnConfirm.TabIndex = 5;
            btnConfirm.Text = "Confirm Purchase";
            btnConfirm.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(586, 69);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(39, 15);
            lblStatus.TabIndex = 6;
            lblStatus.Text = "Status";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(392, 377);
            label2.Name = "label2";
            label2.Size = new Size(56, 15);
            label2.TabIndex = 7;
            label2.Text = "Quantity:";
            // 
            // nudQuantity
            // 
            nudQuantity.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            nudQuantity.Location = new Point(454, 375);
            nudQuantity.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudQuantity.Name = "nudQuantity";
            nudQuantity.Size = new Size(47, 23);
            nudQuantity.TabIndex = 8;
            nudQuantity.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(631, 373);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // CustomerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(718, 414);
            Controls.Add(btnCancel);
            Controls.Add(nudQuantity);
            Controls.Add(label2);
            Controls.Add(lblStatus);
            Controls.Add(btnConfirm);
            Controls.Add(grpDetails);
            Controls.Add(lstResults);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);
            Controls.Add(label1);
            Name = "CustomerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Customer Screen";
            grpDetails.ResumeLayout(false);
            grpDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtSearch;
        private Button btnSearch;
        private ListBox lstResults;
        private GroupBox grpDetails;
        private Label lblColor;
        private Label lblSize;
        private Label lblName;
        private Label lblLocation;
        private Label lblPrice;
        private Label lblStock;
        private Button btnConfirm;
        private Label lblStatus;
        private Label label2;
        private NumericUpDown nudQuantity;
        private Button btnCancel;
    }
}