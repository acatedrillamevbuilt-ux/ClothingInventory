using ClothingInventory.Services;

namespace ClothingInventory.Views
{
    public class SalesView : UserControl
    {
        private readonly SalesService _salesService = new SalesService();

        private readonly DateTimePicker dtpDate = new() { Name = "dtpDate", Format = DateTimePickerFormat.Short };
        private readonly CheckBox chkAllDates = new() { Name = "chkAllDates", Text = "All dates", AutoSize = true };
        private readonly Label lblTotal = new() { Name = "lblTotal", AutoSize = false };
        private readonly DataGridView dgvSales = new() { Name = "dgvSales" };

        public SalesView()
        {
            Size = new Size(1112, 636);
            BackColor = Theme.Surface;
            Font = Theme.BodyMedium;

            Controls.AddRange(new Control[] { dtpDate, chkAllDates, lblTotal, dgvSales });
            Theme.StyleControls(this);

            const int margin = 24;
            int w = Width;

            dtpDate.SetBounds(margin, 30, 160, 28);
            chkAllDates.Location = new Point(margin + 176, 32);

            lblTotal.SetBounds(margin, 76, w - 2 * margin, 28);
            lblTotal.Font = Theme.TitleMedium;
            lblTotal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            dgvSales.SetBounds(margin, 116, w - 2 * margin, Height - 116 - margin);
            dgvSales.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvSales.ReadOnly = true;
            dgvSales.AllowUserToAddRows = false;
            dgvSales.MultiSelect = false;

            dtpDate.Value = DateTime.Today;
            dtpDate.ValueChanged += (s, e) => LoadSales();
            chkAllDates.CheckedChanged += (s, e) => LoadSales();

            Dock = DockStyle.Fill;
        }

        // Called whenever this view is opened, so it shows the latest sales
        public void Reload() => LoadSales();

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