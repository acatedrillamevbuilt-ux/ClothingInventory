using ClothingInventory.Models;
using ClothingInventory.Services;

namespace ClothingInventory.Views
{
    public class InventoryView : UserControl
    {
        private readonly ProductService _productService = new ProductService();
        private Product? _selected;
        private string? _pendingSource;     // a photo the user picked but has not saved yet
        private bool _removeImage;          // the user clicked Remove
        private DateTime _lastClick = DateTime.MinValue;

        private readonly DataGridView dgvProducts = new() { Name = "dgvProducts" };
        private readonly Button btnShowDelisted = new() { Name = "btnShowDelisted" };
        private readonly PhotoBox pbPhoto = new() { Name = "pbPhoto" };
        private readonly Button btnChooseImage = new() { Name = "btnChooseImage", Text = "Choose image" };
        private readonly Button btnRemoveImage = new() { Name = "btnRemoveImage", Text = "Remove" };
        private readonly TextBox txtName = new() { Name = "txtName" };
        private readonly DropdownField ddCategory = new() { Name = "ddCategory" };
        private readonly DropdownField ddSize = new() { Name = "ddSize" };
        private readonly DropdownField ddColor = new() { Name = "ddColor" };
        private readonly DropdownField ddLocation = new() { Name = "ddLocation" };
        private readonly NumericUpDown nudPrice = new() { Name = "nudPrice", DecimalPlaces = 2, Maximum = 100000 };
        private readonly NumericUpDown nudQuantity = new() { Name = "nudQuantity", Maximum = 100000 };
        private readonly NumericUpDown nudRestock = new() { Name = "nudRestock", Minimum = 1, Maximum = 100000, Value = 1 };
        private readonly Button btnAdd = new() { Name = "btnAdd", Text = "Add item" };
        private readonly Button btnUpdate = new() { Name = "btnUpdate", Text = "Save changes" };
        private readonly Button btnClear = new() { Name = "btnClear", Text = "Clear" };
        private readonly Button btnRestock = new() { Name = "btnRestock", Text = "Restock" };
        private readonly Button btnDelist = new() { Name = "btnDelist", Text = "Delist item" };
        private readonly Button btnDelete = new() { Name = "btnDelete", Text = "Delete" };

        public InventoryView()
        {
            Size = new Size(1112, 636);
            BackColor = Theme.Surface;
            Font = Theme.BodyMedium;
            AutoScroll = true;
            AutoScrollMinSize = new Size(1000, 660);

            Controls.AddRange(new Control[]
            {
                dgvProducts, btnShowDelisted, pbPhoto, btnChooseImage, btnRemoveImage,
                btnAdd, btnUpdate, btnClear, btnRestock, btnDelist, btnDelete
            });
            Theme.StyleControls(this);

            Theme.StyleFilterChip(btnShowDelisted, "Show delisted", "\uE7B3");
            Theme.StyleButton(btnChooseImage, M3Button.Tonal, "\uE91B");
            Theme.StyleButton(btnRemoveImage, M3Button.Outlined, "\uE711");
            Theme.StyleButton(btnDelist, M3Button.Tonal, "");
            Theme.StyleButton(btnDelete, M3Button.Outlined, "\uE74D");

            txtName.MaxLength = 100;
            ddCategory.SetItems(Options.Categories);
            ddSize.SetItems(Options.Sizes);
            ddColor.SetItems(Options.Colors);
            ddLocation.SetItems(Options.Locations);

            const int margin = 24, panelW = 376;
            int panelX = Width - margin - panelW;

            // ----- Table (with a filter chip above it) -----
            btnShowDelisted.SetBounds(margin, margin, 150, 32);
            dgvProducts.SetBounds(margin, 72, panelX - margin - 24, Height - 72 - margin);
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.MultiSelect = false;
            Theme.ShowStockChips(dgvProducts, InventoryService.LowStockLevel);
            Photos.AddToGrid(dgvProducts);

            // ----- Edit panel: photo row -----
            pbPhoto.SetBounds(panelX, margin, 88, 88);
            btnChooseImage.SetBounds(panelX + 104, margin, 150, 40);
            btnRemoveImage.SetBounds(panelX + 104, margin + 48, 110, 40);

            // ----- Edit panel: fields -----
            int y = margin + 88 + 16;
            y = Theme.Field(this, "Name", txtName, panelX, y, panelW);
            Theme.Field(this, "Category", ddCategory, panelX, y, 180);
            y = Theme.Field(this, "Size", ddSize, panelX + 196, y, 180);
            Theme.Field(this, "Color", ddColor, panelX, y, 180);
            y = Theme.Field(this, "Location", ddLocation, panelX + 196, y, 180);
            Theme.Field(this, "Price", nudPrice, panelX, y, 180);
            y = Theme.Field(this, "Quantity", nudQuantity, panelX + 196, y, 180);

            btnAdd.SetBounds(panelX, y, 110, 40);
            btnUpdate.SetBounds(panelX + 118, y, 130, 40);
            btnClear.SetBounds(panelX + 256, y, 100, 40);

            int restockY = y + 40 + 16;
            Theme.Field(this, "Restock by", nudRestock, panelX, restockY, 120);
            btnRestock.SetBounds(panelX + 136, restockY + 20, 130, 40);

            int dangerY = restockY + 20 + 40 + 16;
            btnDelist.SetBounds(panelX, dangerY, 150, 40);
            btnDelete.SetBounds(panelX + 158, dangerY, 110, 40);

            // Table grows with the window; the edit panel stays on the right
            dgvProducts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            foreach (Control c in Controls)
                if (c != dgvProducts && c != btnShowDelisted)
                    c.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            dgvProducts.SelectionChanged += dgvProducts_SelectionChanged;
            btnShowDelisted.Click += (s, e) =>
            {
                Theme.SetChipChecked(btnShowDelisted, !Theme.IsChipChecked(btnShowDelisted));
                Reload();
            };
            btnChooseImage.Click += btnChooseImage_Click;
            btnRemoveImage.Click += (s, e) =>
            {
                _pendingSource = null;
                _removeImage = true;
                RefreshPhoto();
            };
            txtName.TextChanged += (s, e) => { if (_pendingSource == null) RefreshPhoto(); };
            btnAdd.Click += btnAdd_Click;
            btnUpdate.Click += btnUpdate_Click;
            btnRestock.Click += btnRestock_Click;
            btnDelist.Click += btnDelist_Click;
            btnDelete.Click += btnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();

            ClearForm();
            Dock = DockStyle.Fill;
        }

        // Called whenever this view is opened, so it reflects changes made elsewhere
        public void Reload()
        {
            int? keep = _selected?.Id;
            LoadProducts();

            if (keep != null && SelectRowById(keep.Value)) return;
            ClearForm();
        }

        // Ignores a click that arrives right after another one (double-click or queued click)
        private bool TooSoon()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastClick).TotalMilliseconds < 600) return true;
            _lastClick = now;
            return false;
        }

        private void LoadProducts()
        {
            dgvProducts.DataSource = _productService.GetAll(Theme.IsChipChecked(btnShowDelisted));

            if (dgvProducts.Columns["Id"] != null)
                dgvProducts.Columns["Id"].Visible = false;
            if (dgvProducts.Columns["Price"] != null)
                dgvProducts.Columns["Price"].DefaultCellStyle.Format = "C2";
        }

        private void dgvProducts_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count == 0) return;
            if (dgvProducts.SelectedRows[0].DataBoundItem is not Product p) return;

            _selected = p;
            _pendingSource = null;
            _removeImage = false;

            txtName.Text = p.Name;
            ddCategory.Value = p.Category;
            ddSize.Value = p.Size;
            ddColor.Value = p.Color;
            ddLocation.Value = p.Location;
            nudPrice.Value = p.Price;
            nudQuantity.Value = p.Quantity;

            btnUpdate.Enabled = true;
            btnRestock.Enabled = true;
            nudRestock.Enabled = true;
            btnDelist.Enabled = true;
            btnDelete.Enabled = true;
            btnDelist.Text = p.IsActive ? "Delist item" : "Relist item";

            RefreshPhoto();
        }

        // ---------- Photo ----------
        private void RefreshPhoto()
        {
            string label = txtName.Text;

            if (_pendingSource != null)
                pbPhoto.ShowFile(_pendingSource, label);
            else if (!_removeImage && _selected != null && _selected.ImagePath != "")
                pbPhoto.ShowStored(_selected.ImagePath, label);
            else
                pbPhoto.ShowPlaceholder(label);
        }

        private void btnChooseImage_Click(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Choose a product photo",
                Filter = ImageStore.Filter,
                CheckFileExists = true
            };
            if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

            string? problem = ImageStore.Check(dialog.FileName);
            if (problem != null)
            {
                Theme.Snack(this, problem, true);
                return;
            }

            _pendingSource = dialog.FileName;
            _removeImage = false;
            RefreshPhoto();
        }

        // Works out which image name to save. Returns null if the picked file could not be saved.
        private string? ResolveImage(bool forNewItem, out bool imported)
        {
            imported = false;

            if (_pendingSource != null)
            {
                try
                {
                    string name = ImageStore.Import(_pendingSource);
                    imported = true;
                    return name;
                }
                catch
                {
                    Theme.Snack(this, "The image could not be saved. Please choose another file.", true);
                    return null;
                }
            }

            if (_removeImage || forNewItem) return "";
            return _selected?.ImagePath ?? "";
        }

        // ---------- Buttons ----------
        private void btnAdd_Click(object? sender, EventArgs e)
        {
            if (TooSoon()) return;
            if (!ValidateFields()) return;

            string? image = ResolveImage(true, out bool imported);
            if (image == null) return;

            var product = ReadFields();
            product.ImagePath = image;

            var result = _productService.AddProduct(product);
            Theme.Snack(this, result.Message, !result.Success);

            if (!result.Success)
            {
                if (imported) ImageStore.Delete(image);
                return;
            }

            LoadProducts();
            ClearForm();
        }

        private void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (TooSoon()) return;
            if (_selected == null) return;
            if (!ValidateFields()) return;

            var old = _selected;
            string? image = ResolveImage(false, out bool imported);
            if (image == null) return;

            var product = ReadFields();
            product.Id = old.Id;
            product.ImagePath = image;

            var result = _productService.UpdateProduct(product);
            Theme.Snack(this, result.Message, !result.Success);

            if (!result.Success)
            {
                if (imported) ImageStore.Delete(image);
                return;
            }

            if (old.ImagePath != "" && old.ImagePath != image)
                ImageStore.Delete(old.ImagePath);

            LoadProducts();
            if (!SelectRowById(old.Id)) ClearForm();
        }

        private void btnRestock_Click(object? sender, EventArgs e)
        {
            if (TooSoon()) return;
            if (_selected == null) return;

            int id = _selected.Id;
            var result = _productService.Restock(id, (int)nudRestock.Value);
            Theme.Snack(this, result.Message, !result.Success);

            if (result.Success)
            {
                LoadProducts();
                if (!SelectRowById(id)) ClearForm();
                nudRestock.Value = 1;
            }
        }

        private void btnDelist_Click(object? sender, EventArgs e)
        {
            if (TooSoon()) return;
            if (_selected == null) return;

            var p = _selected;
            (bool Success, string Message) result;

            if (p.IsActive)
            {
                bool yes = Theme.Confirm(FindForm()!, "Delist this item?",
                    $"\"{p.Name}\" ({p.Size}, {p.Color}) will be hidden from customers and from search. " +
                    "Its sales history is kept and you can relist it at any time.", "Delist");
                if (!yes) return;
                result = _productService.Delist(p);
            }
            else
            {
                result = _productService.Relist(p);
            }

            Theme.Snack(this, result.Message, !result.Success);
            if (!result.Success) return;

            LoadProducts();
            if (!SelectRowById(p.Id)) ClearForm();
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (TooSoon()) return;
            if (_selected == null) return;

            var p = _selected;
            bool yes = Theme.Confirm(FindForm()!, "Delete this item permanently?",
                $"\"{p.Name}\" ({p.Size}, {p.Color}) will be removed for good and this can't be undone. " +
                "Items that have been sold can't be deleted; delist those instead.", "Delete");
            if (!yes) return;

            var result = _productService.Delete(p);
            Theme.Snack(this, result.Message, !result.Success);

            if (result.Success)
            {
                LoadProducts();
                ClearForm();
            }
        }

        // ---------- Validation and helpers ----------
        // Checks the fields before saving and marks each problem on its own field
        private bool ValidateFields()
        {
            Control? first = null;
            void Fail(Control input, string message)
            {
                Theme.SetError(input, message);
                first ??= input;
            }

            foreach (var c in new Control[] { txtName, ddCategory, ddSize, ddColor, ddLocation, nudPrice })
                Theme.SetError(c, null);

            if (string.IsNullOrWhiteSpace(txtName.Text)) Fail(txtName, "Name is required");
            if (ddCategory.Value == "") Fail(ddCategory, "Select a category");
            if (ddSize.Value == "") Fail(ddSize, "Select a size");
            if (ddColor.Value == "") Fail(ddColor, "Select a color");
            if (ddLocation.Value == "") Fail(ddLocation, "Select a location");
            if (nudPrice.Value <= 0) Fail(nudPrice, "Price must be above 0");

            if (first == null) return true;

            first.Focus();
            Theme.Snack(this, "Please fix the highlighted fields.", true);
            return false;
        }

        private Product ReadFields() => new Product
        {
            Name = txtName.Text.Trim(),
            Category = ddCategory.Value,
            Size = ddSize.Value,
            Color = ddColor.Value,
            Location = ddLocation.Value,
            Price = nudPrice.Value,
            Quantity = (int)nudQuantity.Value
        };

        private bool SelectRowById(int id)
        {
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                if (row.DataBoundItem is Product p && p.Id == id)
                {
                    row.Selected = true;
                    dgvProducts.CurrentCell = row.Cells["Name"];
                    return true;
                }
            }
            return false;
        }

        private void ClearForm()
        {
            _selected = null;
            _pendingSource = null;
            _removeImage = false;
            dgvProducts.ClearSelection();

            txtName.Clear();
            ddCategory.Value = "";
            ddSize.Value = "";
            ddColor.Value = "";
            ddLocation.Value = "";
            nudPrice.Value = 0;
            nudQuantity.Value = 0;
            nudRestock.Value = 1;

            foreach (var c in new Control[] { txtName, ddCategory, ddSize, ddColor, ddLocation, nudPrice })
                Theme.SetError(c, null);

            btnUpdate.Enabled = false;
            btnRestock.Enabled = false;
            nudRestock.Enabled = false;
            btnDelist.Enabled = false;
            btnDelete.Enabled = false;
            btnDelist.Text = "Delist item";

            RefreshPhoto();
        }
    }
}