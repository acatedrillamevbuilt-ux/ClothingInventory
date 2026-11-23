using ClothingInventory.Models;
using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class CustomerForm : Form
    {
        private readonly ProductService _productService = new ProductService();
        private readonly InventoryService _inventoryService = new InventoryService();
        private readonly SalesService _salesService = new SalesService();
        private readonly PhotoBox pbPhoto = new() { Name = "pbPhoto" };
        private Product? _selected;

        public CustomerForm()
        {
            InitializeComponent();

            Text = "Customer";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1000, 640);
            grpDetails.Text = "Item details";
            Theme.Apply(this);

            const int margin = 24, gap = 24, top = 88, leftW = 420, photoSize = 200;
            int rightX = margin + leftW + gap;
            int rightW = ClientSize.Width - rightX - margin;

            // ----- Left column: search, status line, results -----
            txtSearch.PlaceholderText = "Search by item name";
            Theme.OutlineInput(txtSearch).Bounds = new Rectangle(margin, top, leftW - 112, 40);

            btnSearch.Text = "Search";
            btnSearch.SetBounds(margin + leftW - 100, top, 100, 40);

            lblStatus.AutoSize = false;
            lblStatus.SetBounds(margin, top + 48, leftW, 20);

            lstResults.SetBounds(margin, top + 80, leftW, ClientSize.Height - (top + 80) - margin);
            lstResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;

            // ----- Right column: details card with the photo on its right -----
            grpDetails.SetBounds(rightX, top, rightW, 260);
            grpDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            grpDetails.Controls.Add(pbPhoto);
            pbPhoto.SetBounds(rightW - 20 - photoSize, 44, photoSize, photoSize);

            int textW = rightW - 40 - photoSize - 16;
            foreach (var l in new[] { lblName, lblSize, lblColor, lblLocation, lblPrice, lblStock })
            {
                grpDetails.Controls.Add(l);
                l.AutoSize = false;
                l.AutoEllipsis = true;
            }

            lblName.SetBounds(20, 44, textW, 32);
            lblName.Font = Theme.TitleLarge;

            int y = 88;
            foreach (var l in new[] { lblSize, lblColor, lblLocation, lblPrice })
            {
                l.SetBounds(20, y, textW, 24);
                l.Font = Theme.BodyLarge;
                y += 28;
            }

            lblStock.SetBounds(20, y + 8, textW, 28);
            lblStock.Font = Theme.TitleMedium;

            // ----- Right column: quantity and buttons -----
            Theme.Field(this, "Quantity", nudQuantity, rightX, 364, 120);

            btnConfirm.Text = "Confirm purchase";
            btnConfirm.SetBounds(rightX + 136, 384, 170, 40);
            btnCancel.Text = "Cancel";
            btnCancel.SetBounds(rightX + 136 + 170 + 8, 384, 100, 40);

            AcceptButton = btnSearch;
            ClearDetails();

            btnSearch.Click += btnSearch_Click;
            lstResults.SelectedIndexChanged += lstResults_SelectedIndexChanged;
            btnConfirm.Click += btnConfirm_Click;
            btnCancel.Click += btnCancel_Click;

            Theme.AddAppBar(this, "Customer", "Find an item, check availability and buy");
            Theme.Finish(this);
            Shown += (s, e) => txtSearch.Focus();
        }

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string name = txtSearch.Text.Trim();
            lstResults.Items.Clear();
            ClearDetails();

            if (name == "")
            {
                lblStatus.Text = "Type an item name to search.";
                return;
            }

            var results = _productService.SearchByName(name);
            lstResults.Items.AddRange(results.Cast<object>().ToArray());

            lblStatus.Text = results.Count == 0
                ? "No items found."
                : $"{results.Count} item(s) found. Select one to see its details.";
        }

        private void lstResults_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selected = lstResults.SelectedItem as Product;

            if (_selected == null)
            {
                ClearDetails();
                return;
            }

            lblName.ForeColor = Theme.OnSurface;
            lblName.Text = _selected.Name;
            lblSize.Text = $"Size: {_selected.Size}";
            lblColor.Text = $"Color: {_selected.Color}";
            lblLocation.Text = $"Location: {_selected.Location}";
            lblPrice.Text = $"Price: {_selected.Price:C}";
            lblStock.Text = _inventoryService.GetStockStatus(_selected);

            lblStock.ForeColor = !_inventoryService.IsAvailable(_selected) ? Theme.Error
                               : _selected.Quantity <= InventoryService.LowStockLevel ? Theme.Warning
                               : Theme.Success;

            pbPhoto.ShowStored(_selected.ImagePath, _selected.Name);

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
            decimal total = _selected.Price * qty;

            bool yes = Theme.Confirm(this, "Confirm purchase",
                $"Buy {qty} x {_selected.Name} ({_selected.Size}, {_selected.Color}) for {total:C}?",
                "Buy");
            if (!yes) return;

            var result = _salesService.RecordSale(_selected, qty);

            btnSearch_Click(null, EventArgs.Empty);   // refresh the stock numbers
            Theme.Snack(this, result.Message, !result.Success);
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            lstResults.ClearSelected();
            ClearDetails();
            Theme.Snack(this, "Purchase cancelled.");
        }

        private void ClearDetails()
        {
            _selected = null;
            lblName.ForeColor = Theme.OnSurfaceVariant;
            lblName.Text = "No item selected";
            lblSize.Text = lblColor.Text = lblLocation.Text = lblPrice.Text = lblStock.Text = "";
            pbPhoto.ShowPlaceholder("?");
            btnConfirm.Enabled = false;
            btnCancel.Enabled = false;
            nudQuantity.Enabled = false;
            nudQuantity.Value = 1;
        }
    }
}