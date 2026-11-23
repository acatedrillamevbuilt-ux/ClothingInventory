using ClothingInventory.Models;
using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class StaffSearchForm : Form
    {
        private readonly ProductService _productService = new ProductService();

        public StaffSearchForm()
        {
            InitializeComponent();

            Text = "Staff";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1100, 640);
            Theme.Apply(this);

            const int margin = 24, top = 88;
            int right = ClientSize.Width - margin;

            // ----- Search row -----
            txtSearch.PlaceholderText = "Search by item name";
            Theme.OutlineInput(txtSearch).Bounds = new Rectangle(margin, top, 360, 40);

            btnSearch.Text = "Search";
            btnSearch.SetBounds(margin + 372, top, 110, 40);

            btnSales.Text = "Sales history";
            btnSales.SetBounds(right - 150, top, 150, 40);
            btnManage.Text = "Manage inventory";
            btnManage.SetBounds(right - 150 - 8 - 180, top, 180, 40);
            btnLowStock.Text = "Low stock";
            btnLowStock.SetBounds(right - 150 - 8 - 180 - 8 - 130, top, 130, 40);
            foreach (var b in new[] { btnSales, btnManage, btnLowStock })
                b.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // ----- Status line and results grid -----
            lblStatus.AutoSize = false;
            lblStatus.SetBounds(margin, top + 48, ClientSize.Width - 2 * margin, 20);

            dgvResults.SetBounds(margin, top + 80, ClientSize.Width - 2 * margin,
                ClientSize.Height - (top + 80) - margin);
            dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResults.ReadOnly = true;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.MultiSelect = false;
            Theme.ShowStockChips(dgvResults, InventoryService.LowStockLevel);

            AcceptButton = btnSearch;
            btnSearch.Click += btnSearch_Click;

            Theme.AddAppBar(this, "Staff", "Look up items and check stock levels");
            Theme.Finish(this);
            Shown += (s, e) => txtSearch.Focus();
        }

        private void ShowResults(List<Product> items)
        {
            dgvResults.DataSource = items;

            if (dgvResults.Columns["Id"] != null)
                dgvResults.Columns["Id"].Visible = false;
            if (dgvResults.Columns["Price"] != null)
                dgvResults.Columns["Price"].DefaultCellStyle.Format = "C2";
        }

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string name = txtSearch.Text.Trim();

            if (name == "")
            {
                dgvResults.DataSource = null;
                lblStatus.Text = "Type an item name to search.";
                return;
            }

            var results = _productService.SearchByName(name);
            ShowResults(results);
            lblStatus.Text = results.Count == 0 ? "No items found." : $"{results.Count} item(s) found.";
        }

        private void btnLowStock_Click(object? sender, EventArgs e)
        {
            var items = _productService.GetLowStock();
            ShowResults(items);
            lblStatus.Text = items.Count == 0
                ? "No items are low on stock."
                : $"{items.Count} item(s) need restocking.";
        }

        private void btnManage_Click(object? sender, EventArgs e)
        {
            using var form = new InventoryManagementForm();
            form.ShowDialog(this);

            // Refresh the results in case items were added or changed
            if (txtSearch.Text.Trim() != "")
                btnSearch_Click(null, EventArgs.Empty);
        }

        private void btnSales_Click(object? sender, EventArgs e)
        {
            using var form = new SalesHistoryForm();
            form.ShowDialog(this);
        }
    }
}