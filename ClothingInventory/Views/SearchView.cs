using ClothingInventory.Models;
using ClothingInventory.Services;

namespace ClothingInventory.Views
{
    public class SearchView : UserControl
    {
        private readonly ProductService _productService = new ProductService();

        private readonly TextBox txtSearch = new() { Name = "txtSearch", PlaceholderText = "Search by item name" };
        private readonly Button btnSearch = new() { Name = "btnSearch", Text = "Search" };
        private readonly Button btnLowStock = new() { Name = "btnLowStock" };
        private readonly Label lblStatus = new() { Name = "lblStatus", AutoSize = false };
        private readonly DataGridView dgvResults = new() { Name = "dgvResults" };

        private string _lastQuery = "";

        public SearchView()
        {
            Size = new Size(1112, 636);
            BackColor = Theme.Surface;
            Font = Theme.BodyMedium;

            Controls.AddRange(new Control[] { txtSearch, btnSearch, btnLowStock, lblStatus, dgvResults });
            Theme.StyleControls(this);

            const int margin = 24;
            int w = Width;

            // ----- Row 1: search field and button -----
            Theme.OutlineInput(txtSearch).Bounds = new Rectangle(margin, 24, 360, 40);
            btnSearch.SetBounds(margin + 372, 24, 110, 40);

            // ----- Row 2: filter chip and status text -----
            Theme.StyleFilterChip(btnLowStock, "Low stock", "\uE7BA");
            btnLowStock.SetBounds(margin, 76, 130, 32);

            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            lblStatus.SetBounds(margin + 146, 76, w - 2 * margin - 146, 32);
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            // ----- Results table -----
            dgvResults.SetBounds(margin, 120, w - 2 * margin, Height - 120 - margin);
            dgvResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvResults.ReadOnly = true;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.MultiSelect = false;
            Theme.ShowStockChips(dgvResults, InventoryService.LowStockLevel);

            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSearch.PerformClick();
                }
            };

            btnSearch.Click += (s, e) =>
            {
                _lastQuery = txtSearch.Text.Trim();
                ShowCurrent();
            };

            btnLowStock.Click += (s, e) =>
            {
                Theme.SetChipChecked(btnLowStock, !Theme.IsChipChecked(btnLowStock));
                ShowCurrent();
            };

            ShowCurrent();
            Dock = DockStyle.Fill;
        }

        public void FocusSearch() => txtSearch.Focus();

        // Called whenever this view is opened, so it reflects changes made elsewhere
        public void Reload() => ShowCurrent();

        private void ShowCurrent()
        {
            bool lowOnly = Theme.IsChipChecked(btnLowStock);
            string query = _lastQuery;

            // Nothing to show yet
            if (query == "" && !lowOnly)
            {
                dgvResults.DataSource = null;
                lblStatus.Text = "Type an item name to search, or turn on Low stock.";
                return;
            }

            List<Product> items;
            if (query == "")
            {
                items = _productService.GetLowStock();
            }
            else
            {
                items = _productService.SearchByName(query);
                if (lowOnly)
                    items = items.Where(p => p.Quantity <= InventoryService.LowStockLevel).ToList();
            }

            if (items.Count == 0)
            {
                lblStatus.Text = !lowOnly ? "No items found."
                    : query == "" ? "No items are low on stock."
                    : $"No low-stock items match \"{query}\".";
            }
            else
            {
                lblStatus.Text = !lowOnly ? $"{items.Count} item(s) found."
                    : query == "" ? $"{items.Count} item(s) need restocking."
                    : $"{items.Count} low-stock item(s) match \"{query}\".";
            }

            dgvResults.DataSource = items;

            if (dgvResults.Columns["Id"] != null)
                dgvResults.Columns["Id"].Visible = false;
            if (dgvResults.Columns["Price"] != null)
                dgvResults.Columns["Price"].DefaultCellStyle.Format = "C2";
        }
    }
}