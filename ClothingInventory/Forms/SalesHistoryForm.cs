using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class SalesHistoryForm : Form
    {
        private readonly SalesService _salesService = new SalesService();

        public SalesHistoryForm()
        {
            InitializeComponent();

            Text = "Sales history";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(960, 580);
            Theme.Apply(this);

            const int margin = 24, top = 88;

            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.SetBounds(margin, top + 6, 160, 28);
            chkAllDates.Text = "All dates";
            chkAllDates.AutoSize = true;
            chkAllDates.Location = new Point(margin + 176, top + 8);

            lblTotal.AutoSize = false;
            lblTotal.Font = Theme.TitleMedium;
            lblTotal.SetBounds(margin, top + 52, ClientSize.Width - 2 * margin, 28);

            dgvSales.SetBounds(margin, top + 92, ClientSize.Width - 2 * margin,
                ClientSize.Height - (top + 92) - margin);
            dgvSales.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvSales.ReadOnly = true;
            dgvSales.AllowUserToAddRows = false;
            dgvSales.MultiSelect = false;

            dtpDate.Value = DateTime.Today;
            dtpDate.ValueChanged += (s, e) => LoadSales();
            chkAllDates.CheckedChanged += (s, e) => LoadSales();

            Theme.AddAppBar(this, "Sales history", "Review recorded sales");
            Theme.Finish(this);

            LoadSales();
        }

        private void LoadSales()
        {
            dtpDate.Enabled = !chkAllDates.Checked;

            DateTime? day = chkAllDates.Checked ? null : dtpDate.Value.Date;
            var sales = _salesService.GetSales(day);

            dgvSales.DataSource = sales;

            if (dgvSales.Columns["SaleId"] != null)
                dgvSales.Columns["SaleId"].Visible = false;
            if (dgvSales.Columns["SoldAt"] != null)
                dgvSales.Columns["SoldAt"].HeaderText = "Date / Time";
            if (dgvSales.Columns["UnitPrice"] != null)
                dgvSales.Columns["UnitPrice"].DefaultCellStyle.Format = "C2";
            if (dgvSales.Columns["Total"] != null)
                dgvSales.Columns["Total"].DefaultCellStyle.Format = "C2";

            decimal total = sales.Sum(s => s.Total);
            int items = sales.Sum(s => s.Quantity);
            lblTotal.Text = sales.Count == 0
                ? "No sales for this selection."
                : $"{sales.Count} sale(s), {items} item(s) sold. Total: {total:C}";
        }
    }
}