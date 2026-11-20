namespace ClothingInventory.Forms
{
    partial class InventoryManagementForm
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
            dgvProducts = new DataGridView();
            txtName = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtCategory = new TextBox();
            label3 = new Label();
            txtSize = new TextBox();
            label4 = new Label();
            txtColor = new TextBox();
            label5 = new Label();
            txtLocation = new TextBox();
            nudPrice = new NumericUpDown();
            nudQuantity = new NumericUpDown();
            label6 = new Label();
            label7 = new Label();
            btnAdd = new Button();
            btnClear = new Button();
            btnUpdate = new Button();
            nudRestock = new NumericUpDown();
            label8 = new Label();
            btnRestock = new Button();
            lblStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvProducts).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPrice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudRestock).BeginInit();
            SuspendLayout();
            // 
            // dgvProducts
            // 
            dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            dgvProducts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvProducts.Location = new Point(24, 12);
            dgvProducts.Name = "dgvProducts";
            dgvProducts.Size = new Size(989, 459);
            dgvProducts.TabIndex = 0;
            // 
            // txtName
            // 
            txtName.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txtName.Location = new Point(1083, 24);
            txtName.Name = "txtName";
            txtName.Size = new Size(100, 23);
            txtName.TabIndex = 1;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(1019, 27);
            label1.Name = "label1";
            label1.Size = new Size(39, 15);
            label1.TabIndex = 2;
            label1.Text = "Name";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(1019, 66);
            label2.Name = "label2";
            label2.Size = new Size(55, 15);
            label2.TabIndex = 4;
            label2.Text = "Category";
            // 
            // txtCategory
            // 
            txtCategory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txtCategory.Location = new Point(1083, 66);
            txtCategory.Name = "txtCategory";
            txtCategory.Size = new Size(100, 23);
            txtCategory.TabIndex = 3;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(1019, 102);
            label3.Name = "label3";
            label3.Size = new Size(27, 15);
            label3.TabIndex = 6;
            label3.Text = "Size";
            // 
            // txtSize
            // 
            txtSize.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txtSize.Location = new Point(1083, 99);
            txtSize.Name = "txtSize";
            txtSize.Size = new Size(100, 23);
            txtSize.TabIndex = 5;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label4.AutoSize = true;
            label4.Location = new Point(1019, 134);
            label4.Name = "label4";
            label4.Size = new Size(36, 15);
            label4.TabIndex = 8;
            label4.Text = "Color";
            // 
            // txtColor
            // 
            txtColor.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txtColor.Location = new Point(1083, 131);
            txtColor.Name = "txtColor";
            txtColor.Size = new Size(100, 23);
            txtColor.TabIndex = 7;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(1019, 164);
            label5.Name = "label5";
            label5.Size = new Size(53, 15);
            label5.TabIndex = 10;
            label5.Text = "Location";
            // 
            // txtLocation
            // 
            txtLocation.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            txtLocation.Location = new Point(1083, 161);
            txtLocation.Name = "txtLocation";
            txtLocation.Size = new Size(100, 23);
            txtLocation.TabIndex = 9;
            // 
            // nudPrice
            // 
            nudPrice.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            nudPrice.DecimalPlaces = 2;
            nudPrice.Location = new Point(1083, 190);
            nudPrice.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudPrice.Name = "nudPrice";
            nudPrice.Size = new Size(74, 23);
            nudPrice.TabIndex = 11;
            // 
            // nudQuantity
            // 
            nudQuantity.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            nudQuantity.Location = new Point(1083, 219);
            nudQuantity.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudQuantity.Name = "nudQuantity";
            nudQuantity.Size = new Size(74, 23);
            nudQuantity.TabIndex = 12;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(1019, 192);
            label6.Name = "label6";
            label6.Size = new Size(33, 15);
            label6.TabIndex = 13;
            label6.Text = "Price";
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Location = new Point(1019, 221);
            label7.Name = "label7";
            label7.Size = new Size(53, 15);
            label7.TabIndex = 14;
            label7.Text = "Quantity";
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnAdd.Location = new Point(1021, 248);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 15;
            btnAdd.Text = "Add New";
            btnAdd.UseVisualStyleBackColor = true;
            // 
            // btnClear
            // 
            btnClear.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnClear.Location = new Point(1207, 248);
            btnClear.Name = "btnClear";
            btnClear.Size = new Size(75, 23);
            btnClear.TabIndex = 16;
            btnClear.Text = "Clear";
            btnClear.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnUpdate.Location = new Point(1102, 248);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(99, 23);
            btnUpdate.TabIndex = 17;
            btnUpdate.Text = "Save Changes";
            btnUpdate.UseVisualStyleBackColor = true;
            // 
            // nudRestock
            // 
            nudRestock.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            nudRestock.Location = new Point(1094, 290);
            nudRestock.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            nudRestock.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudRestock.Name = "nudRestock";
            nudRestock.Size = new Size(74, 23);
            nudRestock.TabIndex = 18;
            nudRestock.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Location = new Point(1024, 292);
            label8.Name = "label8";
            label8.Size = new Size(64, 15);
            label8.TabIndex = 19;
            label8.Text = "Restock by";
            // 
            // btnRestock
            // 
            btnRestock.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnRestock.Location = new Point(1174, 288);
            btnRestock.Name = "btnRestock";
            btnRestock.Size = new Size(75, 23);
            btnRestock.TabIndex = 20;
            btnRestock.Text = "Restock";
            btnRestock.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(1121, 343);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 21;
            // 
            // InventoryManagementForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1301, 483);
            Controls.Add(lblStatus);
            Controls.Add(btnRestock);
            Controls.Add(label8);
            Controls.Add(nudRestock);
            Controls.Add(btnUpdate);
            Controls.Add(btnClear);
            Controls.Add(btnAdd);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(nudQuantity);
            Controls.Add(nudPrice);
            Controls.Add(label5);
            Controls.Add(txtLocation);
            Controls.Add(label4);
            Controls.Add(txtColor);
            Controls.Add(label3);
            Controls.Add(txtSize);
            Controls.Add(label2);
            Controls.Add(txtCategory);
            Controls.Add(label1);
            Controls.Add(txtName);
            Controls.Add(dgvProducts);
            Name = "InventoryManagementForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Inventory Management";
            ((System.ComponentModel.ISupportInitialize)dgvProducts).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPrice).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudQuantity).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudRestock).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dgvProducts;
        private TextBox txtName;
        private Label label1;
        private Label label2;
        private TextBox txtCategory;
        private Label label3;
        private TextBox txtSize;
        private Label label4;
        private TextBox txtColor;
        private Label label5;
        private TextBox txtLocation;
        private NumericUpDown nudPrice;
        private NumericUpDown nudQuantity;
        private Label label6;
        private Label label7;
        private Button btnAdd;
        private Button btnClear;
        private Button btnUpdate;
        private NumericUpDown nudRestock;
        private Label label8;
        private Button btnRestock;
        private Label lblStatus;
    }
}