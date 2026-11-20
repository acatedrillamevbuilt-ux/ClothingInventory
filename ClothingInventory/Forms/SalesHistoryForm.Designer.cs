namespace ClothingInventory.Forms
{
    partial class SalesHistoryForm
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
            dtpDate = new DateTimePicker();
            chkAllDates = new CheckBox();
            dgvSales = new DataGridView();
            lblTotal = new Label();
            ((System.ComponentModel.ISupportInitialize)dgvSales).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(668, 17);
            label1.Name = "label1";
            label1.Size = new Size(37, 15);
            label1.TabIndex = 0;
            label1.Text = "Date: ";
            // 
            // dtpDate
            // 
            dtpDate.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.Location = new Point(711, 13);
            dtpDate.Name = "dtpDate";
            dtpDate.Size = new Size(96, 23);
            dtpDate.TabIndex = 1;
            // 
            // chkAllDates
            // 
            chkAllDates.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chkAllDates.AutoSize = true;
            chkAllDates.Location = new Point(590, 16);
            chkAllDates.Name = "chkAllDates";
            chkAllDates.Size = new Size(72, 19);
            chkAllDates.TabIndex = 2;
            chkAllDates.Text = "All Dates";
            chkAllDates.UseVisualStyleBackColor = true;
            // 
            // dgvSales
            // 
            dgvSales.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvSales.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSales.Location = new Point(12, 50);
            dgvSales.Name = "dgvSales";
            dgvSales.Size = new Size(873, 467);
            dgvSales.TabIndex = 3;
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Location = new Point(12, 17);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new Size(0, 15);
            lblTotal.TabIndex = 4;
            // 
            // SalesHistoryForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(910, 529);
            Controls.Add(lblTotal);
            Controls.Add(dgvSales);
            Controls.Add(chkAllDates);
            Controls.Add(dtpDate);
            Controls.Add(label1);
            Name = "SalesHistoryForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sales History";
            ((System.ComponentModel.ISupportInitialize)dgvSales).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private DateTimePicker dtpDate;
        private CheckBox chkAllDates;
        private DataGridView dgvSales;
        private Label lblTotal;
    }
}