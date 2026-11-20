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

            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.RowHeadersVisible = false;
            dgvProducts.MultiSelect = false;
            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnRestock.Click += btnRestock_Click;
            btnClear.Click += (s, e) => ClearForm();

            LoadProducts();
            ClearForm();
        }

        private void LoadProducts()
        {
            dgvProducts.DataSource = _productService.GetAll();
            if (dgvProducts.Columns["Id"] != null)
                dgvProducts.Columns["Id"].Visible = false;

            if (dgvProducts.Columns["Price"] != null)
                dgvProducts.Columns["Price"].DefaultCellStyle.Format = "N2";
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
            lblStatus.Text = result.Message;

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
            lblStatus.Text = result.Message;

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
            lblStatus.Text = result.Message;

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