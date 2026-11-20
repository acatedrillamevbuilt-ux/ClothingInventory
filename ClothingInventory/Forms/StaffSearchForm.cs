using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class StaffSearchForm : Form
    {
        private readonly ProductService _productService = new ProductService();

        public StaffSearchForm()
        {
            InitializeComponent();

            AcceptButton = btnSearch;   // pressing Enter triggers Search
            dgvResults.ReadOnly = true;
            dgvResults.AllowUserToAddRows = false;
            dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            btnSearch.Click += btnSearch_Click;
        }

        private void btnSearch_Click(object? sender, EventArgs e)
        {
            string name = txtSearch.Text.Trim();

            if (name == "")
            {
                dgvResults.DataSource = null;
                lblStatus.Text = "Please enter an item name.";
                return;
            }

            var results = _productService.SearchByName(name);
            dgvResults.DataSource = results;

            if (dgvResults.Columns["Id"] != null)
                dgvResults.Columns["Id"].Visible = false;

            lblStatus.Text = results.Count == 0
                ? "No items found."
                : $"{results.Count} item(s) found.";
        }

        private void btnManage_Click(object? sender, EventArgs e)
        {
            using var form = new InventoryManagementForm();
            form.ShowDialog(this);

            // Refresh the results in case items were added or changed
            if (txtSearch.Text.Trim() != "")
                btnSearch_Click(null, EventArgs.Empty);
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            using var form = new SalesHistoryForm();
            form.ShowDialog(this);
        }
    }
}