using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class SalesHistoryForm : Form
    {
        private readonly SalesService _salesService = new SalesService();

        public SalesHistoryForm()
        {
            InitializeComponent();

            dgvSales.ReadOnly = true;
            dgvSales.AllowUserToAddRows = false;
            dgvSales.RowHeadersVisible = false;
            dgvSales.MultiSelect = false;
            dgvSales.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSales.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dtpDate.Value = DateTime.Today;

            dtpDate.ValueChanged += (s, e) => LoadSales();
            chkAllDates.CheckedChanged += (s, e) => LoadSales();

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
                dgvSales.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
            if (dgvSales.Columns["Total"] != null)
                dgvSales.Columns["Total"].DefaultCellStyle.Format = "N2";

            decimal total = sales.Sum(s => s.Total);
            int items = sales.Sum(s => s.Quantity);
            lblTotal.Text = sales.Count == 0
                ? "No sales for this selection."
                : $"{sales.Count} sale(s), {items} item(s) sold. Total: {total:C}";
        }
    }
}