using ClothingInventory.Models;
using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class CustomerForm : Form
    {
        private readonly ProductService _productService = new ProductService();
        private readonly InventoryService _inventoryService = new InventoryService();
        private readonly SalesService _salesService = new SalesService();
        private Product? _selected;

        public CustomerForm()
        {
            InitializeComponent();

            AcceptButton = btnSearch;
            ClearDetails();

            btnSearch.Click += btnSearch_Click;
            lstResults.SelectedIndexChanged += lstResults_SelectedIndexChanged;
            btnConfirm.Click += btnConfirm_Click;
            btnCancel.Click += btnCancel_Click;
        }

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string name = txtSearch.Text.Trim();
            lstResults.Items.Clear();
            ClearDetails();

            if (name == "")
            {
                lblStatus.Text = "Please enter an item name.";
                return;
            }

            var results = _productService.SearchByName(name);
            lstResults.Items.AddRange(results.Cast<object>().ToArray());

            lblStatus.Text = results.Count == 0
                ? "No items found."
                : "Select an item to see its details.";
        }

        private void lstResults_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selected = lstResults.SelectedItem as Product;

            if (_selected == null)
            {
                ClearDetails();
                return;
            }

            lblName.Text = $"Name: {_selected.Name}";
            lblSize.Text = $"Size: {_selected.Size}";
            lblColor.Text = $"Color: {_selected.Color}";
            lblLocation.Text = $"Location: {_selected.Location}";
            lblPrice.Text = $"Price: {_selected.Price:C}";
            lblStock.Text = $"Availability: {_inventoryService.GetStockStatus(_selected)}";

            bool available = _inventoryService.IsAvailable(_selected);
            btnConfirm.Enabled = available;
            btnCancel.Enabled = true;
            nudQuantity.Enabled = available;
            nudQuantity.Maximum = Math.Max(1, _selected.Quantity);
            nudQuantity.Value = 1;
        }

        private void btnConfirm_Click(object? sender, EventArgs e)
        {
            if (_selected == null) return;

            int qty = (int)nudQuantity.Value;
            var result = _salesService.RecordSale(_selected, qty);

            // Refresh the list so the stock numbers are up to date
            btnSearch_Click(null, EventArgs.Empty);
            lblStatus.Text = result.Message;
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            lstResults.ClearSelected();
            ClearDetails();
            lblStatus.Text = "Purchase cancelled.";
        }

        private void ClearDetails()
        {
            _selected = null;
            lblName.Text = lblSize.Text = lblColor.Text = "";
            lblLocation.Text = lblPrice.Text = lblStock.Text = "";
            btnConfirm.Enabled = false;
            btnCancel.Enabled = false;
            nudQuantity.Enabled = false;
            nudQuantity.Value = 1;
        }
    }
}