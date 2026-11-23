namespace ClothingInventory.Forms
{
    partial class StaffSearchForm
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
            dgvResults = new DataGridView();
            lblStatus = new Label();
            btnManage = new Button();
            btnSales = new Button();
            btnLowStock = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvResults).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 18);
            label1.Name = "label1";
            label1.Size = new Size(69, 15);
            label1.TabIndex = 0;
            label1.Text = "Item Name:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(97, 12);
            txtSearch.Name = "txtSearch";
            txtSearch.Size = new Size(100, 23);
            txtSearch.TabIndex = 1;
            // 
            // btnSearch
            // 
            btnSearch.Location = new Point(217, 14);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(75, 23);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Search";
            btnSearch.UseVisualStyleBackColor = true;
            // 
            // dgvResults
            // 
            dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResults.Location = new Point(22, 94);
            dgvResults.Name = "dgvResults";
            dgvResults.RowHeadersVisible = false;
            dgvResults.Size = new Size(766, 344);
            dgvResults.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(97, 55);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(0, 15);
            lblStatus.TabIndex = 4;
            // 
            // btnManage
            // 
            btnManage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnManage.Location = new Point(495, 18);
            btnManage.Name = "btnManage";
            btnManage.Size = new Size(154, 23);
            btnManage.TabIndex = 5;
            btnManage.Text = "Manage Inventory";
            btnManage.UseVisualStyleBackColor = true;
            btnManage.Click += btnManage_Click;
            // 
            // btnSales
            // 
            btnSales.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSales.Location = new Point(373, 18);
            btnSales.Name = "btnSales";
            btnSales.Size = new Size(116, 23);
            btnSales.TabIndex = 6;
            btnSales.Text = "Sales History";
            btnSales.UseVisualStyleBackColor = true;
            btnSales.Click += btnSales_Click;
            // 
            // btnLowStock
            // 
            btnLowStock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLowStock.Location = new Point(655, 18);
            btnLowStock.Name = "btnLowStock";
            btnLowStock.Size = new Size(116, 23);
            btnLowStock.TabIndex = 7;
            btnLowStock.Text = "Low Stock Items";
            btnLowStock.UseVisualStyleBackColor = true;
            btnLowStock.Click += btnLowStock_Click;
            // 
            // StaffSearchForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnLowStock);
            Controls.Add(btnSales);
            Controls.Add(btnManage);
            Controls.Add(lblStatus);
            Controls.Add(dgvResults);
            Controls.Add(btnSearch);
            Controls.Add(txtSearch);
            Controls.Add(label1);
            Name = "StaffSearchForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Staff Search";
            ((System.ComponentModel.ISupportInitialize)dgvResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtSearch;
        private Button btnSearch;
        private DataGridView dgvResults;
        private Label lblStatus;
        private Button btnManage;
        private Button btnSales;
        private Button btnLowStock;
    }
}