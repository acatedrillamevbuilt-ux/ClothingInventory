using ClothingInventory.Models;
using ClothingInventory.Services;

namespace ClothingInventory.Views
{
    public class InventoryView : UserControl
    {
        private readonly ProductService _productService = new ProductService();
        private int? _selectedId;

        private readonly DataGridView dgvProducts = new() { Name = "dgvProducts" };
        private readonly TextBox txtName = new() { Name = "txtName" };
        private readonly TextBox txtCategory = new() { Name = "txtCategory" };
        private readonly TextBox txtSize = new() { Name = "txtSize" };
        private readonly TextBox txtColor = new() { Name = "txtColor" };
        private readonly TextBox txtLocation = new() { Name = "txtLocation" };
        private readonly NumericUpDown nudPrice = new() { Name = "nudPrice", DecimalPlaces = 2, Maximum = 100000 };
        private readonly NumericUpDown nudQuantity = new() { Name = "nudQuantity", Maximum = 100000 };
        private readonly NumericUpDown nudRestock = new() { Name = "nudRestock", Minimum = 1, Maximum = 100000, Value = 1 };
        private readonly Button btnAdd = new() { Name = "btnAdd", Text = "Add item" };
        private readonly Button btnUpdate = new() { Name = "btnUpdate", Text = "Save changes" };
        private readonly Button btnClear = new() { Name = "btnClear", Text = "Clear" };
        private readonly Button btnRestock = new() { Name = "btnRestock", Text = "Restock" };

        public InventoryView()
        {
            Size = new Size(1112, 636);
            BackColor = Theme.Surface;
            Font = Theme.BodyMedium;

            Controls.AddRange(new Control[] { dgvProducts, btnAdd, btnUpdate, btnClear, btnRestock });
            Theme.StyleControls(this);

            const int margin = 24, panelW = 376;
            int panelX = Width - margin - panelW;

            // ----- Table on the left -----
            dgvProducts.SetBounds(margin, margin, panelX - margin - 24, Height - 2 * margin);
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.MultiSelect = false;
            Theme.ShowStockChips(dgvProducts, InventoryService.LowStockLevel);

            // ----- Edit panel on the right -----
            int y = margin;
            y = Theme.Field(this, "Name", txtName, panelX, y, panelW);
            Theme.Field(this, "Category", txtCategory, panelX, y, 180);
            y = Theme.Field(this, "Size", txtSize, panelX + 196, y, 180);
            Theme.Field(this, "Color", txtColor, panelX, y, 180);
            y = Theme.Field(this, "Location", txtLocation, panelX + 196, y, 180);
            Theme.Field(this, "Price", nudPrice, panelX, y, 180);
            y = Theme.Field(this, "Quantity", nudQuantity, panelX + 196, y, 180);

            btnAdd.SetBounds(panelX, y + 8, 110, 40);
            btnUpdate.SetBounds(panelX + 118, y + 8, 130, 40);
            btnClear.SetBounds(panelX + 256, y + 8, 100, 40);

            int restockY = y + 8 + 40 + 24;
            Theme.Field(this, "Restock by", nudRestock, panelX, restockY, 120);
            btnRestock.SetBounds(panelX + 136, restockY + 20, 130, 40);

            // Table grows with the window; the edit panel stays on the right
            dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            foreach (Control c in Controls)
                if (c != dgvProducts)
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnRestock.Click += btnRestock_Click;
            btnClear.Click += (s, e) => ClearForm();

            ClearForm();
            Dock = DockStyle.Fill;
        }

        // Called whenever this view is opened, so it reflects changes made elsewhere
        public void Reload()
        {
            int? keep = _selectedId;
            LoadProducts();

            if (keep != null) SelectRowById(keep.Value);
            else ClearForm();
        }

        private void LoadProducts()
        {
            dgvProducts.DataSource = _productService.GetAll();

            if (dgvProducts.Columns["Id"] != null)
                dgvProducts.Columns["Id"].Visible = false;
            if (dgvProducts.Columns["Price"] != null)
                dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
        }

        private void dgvProducts_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            if (dgvProducts.SelectedRows[0].DataBoundItem is not Product p) return;

            _selectedId = p.Id;
            txtName.Text = p.Name;
            txtCategory.Text = p.Category;
            txtSize.Text = p.Size;
            txtColor.Text = p.Color;
            txtLocation.Text = p.Location;
            nudPrice.Value = p.Price;
            nudQuantity.Value = p.Quantity;

            btnUpdate.Enabled = true;
            btnRestock.Enabled = true;
            nudRestock.Enabled = true;
        }

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            var result = _productService.AddProduct(ReadFields());
            Theme.Snack(this, result.Message, !result.Success);

            if (result.Success)
            {
                LoadProducts();
                ClearForm();
            }
        }

        private void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) return;

            var product = ReadFields();
            product.Id = _selectedId.Value;

            var result = _productService.UpdateProduct(product);
            Theme.Snack(this, result.Message, !result.Success);

            if (result.Success)
            {
                LoadProducts();
                SelectRowById(product.Id);
            }
        }

        private void btnRestock_Click(object? sender, EventArgs e)
        {
            if (_selectedId == null) return;

            int id = _selectedId.Value;
            var result = _productService.Restock(id, (int)nudRestock.Value);
            Theme.Snack(this, result.Message, !result.Success);

            if (result.Success)
            {
                LoadProducts();
                SelectRowById(id);
                nudRestock.Value = 1;
            }
        }

        private Product ReadFields() => new Product
        {
            Name = txtName.Text.Trim(),
            Category = txtCategory.Text.Trim(),
            Size = txtSize.Text.Trim(),
            Color = txtColor.Text.Trim(),
            Location = txtLocation.Text.Trim(),
            Price = nudPrice.Value,
            Quantity = (int)nudQuantity.Value
        };

        private void SelectRowById(int id)
        {
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                if (row.DataBoundItem is Product p && p.Id == id)
                {
                    row.Selected = true;
                    dgvProducts.CurrentCell = row.Cells["Name"];
                    return;
                }
            }
        }

        private void ClearForm()
        {
            _selectedId = null;
            dgvProducts.ClearSelection();

            txtName.Clear();
            txtCategory.Clear();
            txtSize.Clear();
            txtColor.Clear();
            txtLocation.Clear();
            nudPrice.Value = 0;
            nudQuantity.Value = 0;
            nudRestock.Value = 1;

            btnUpdate.Enabled = false;
            btnRestock.Enabled = false;
            nudRestock.Enabled = false;
        }
    }
}