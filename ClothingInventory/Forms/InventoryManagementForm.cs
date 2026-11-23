using ClothingInventory.Models;
using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class InventoryManagementForm : Form
    {
        private readonly ProductService _productService = new ProductService();
        private int? _selectedId;

        public InventoryManagementForm()
        {
            InitializeComponent();

            Text = "Inventory management";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 620);
            Theme.Apply(this);

            lblStatus.Visible = false;
            nudPrice.DecimalPlaces = 2;
            nudPrice.Maximum = 100000;
            nudQuantity.Maximum = 100000;
            nudRestock.Minimum = 1;
            nudRestock.Maximum = 100000;

            const int margin = 24, panelW = 376, top = 88;
            int panelX = ClientSize.Width - margin - panelW;

            // ----- Table on the left -----
            dgvProducts.SetBounds(margin, top, panelX - margin - 24, ClientSize.Height - top - margin);
            dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.MultiSelect = false;
            Theme.ShowStockChips(dgvProducts, InventoryService.LowStockLevel);

            // ----- Edit panel on the right -----
            int y = top;
            y = Theme.Field(this, "Name", txtName, panelX, y, panelW);
            Theme.Field(this, "Category", txtCategory, panelX, y, 180);
            y = Theme.Field(this, "Size", txtSize, panelX + 196, y, 180);
            Theme.Field(this, "Color", txtColor, panelX, y, 180);
            y = Theme.Field(this, "Location", txtLocation, panelX + 196, y, 180);
            Theme.Field(this, "Price", nudPrice, panelX, y, 180);
            y = Theme.Field(this, "Quantity", nudQuantity, panelX + 196, y, 180);

            btnAdd.Text = "Add item";
            btnAdd.SetBounds(panelX, y + 8, 110, 40);
            btnUpdate.Text = "Save changes";
            btnUpdate.SetBounds(panelX + 118, y + 8, 130, 40);
            btnClear.Text = "Clear";
            btnClear.SetBounds(panelX + 256, y + 8, 100, 40);

            int restockY = y + 8 + 40 + 24;
            Theme.Field(this, "Restock by", nudRestock, panelX, restockY, 120);
            btnRestock.Text = "Restock";
            btnRestock.SetBounds(panelX + 136, restockY + 20, 130, 40);

            dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnRestock.Click += btnRestock_Click;
            btnClear.Click += (s, e) => ClearForm();

            Theme.AddAppBar(this, "Inventory management", "Add items, edit details and restock");

            // The edit panel stays on the right when the window is resized
            foreach (Control c in Controls)
                if (c != dgvProducts && c.Dock == DockStyle.None && c.Left >= panelX - 1)
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            Theme.Finish(this);

            LoadProducts();
            ClearForm();
        }

        private void LoadProducts()
        {
            dgvProducts.DataSource = _productService.GetAll();

            if (dgvProducts.Columns["Id"] != null)
                dgvProducts.Columns["Id"].Visible = false;
            if (dgvProducts.Columns["Price"] != null)
                dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
            
            dgvProducts.Refresh();
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