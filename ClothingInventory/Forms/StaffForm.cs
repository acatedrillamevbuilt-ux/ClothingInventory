using ClothingInventory.Views;

namespace ClothingInventory.Forms
{
    public partial class StaffForm : Form
    {
        private readonly Panel _content = new() { Dock = DockStyle.Fill };
        private readonly NavRail _rail = new();
        private readonly SearchView _search = new();
        private readonly InventoryView _inventory = new();
        private readonly SalesView _sales = new();
        private readonly SettingsView _settings = new();

        public StaffForm()
        {
            InitializeComponent();

            Text = "Staff";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(1200, 730);
            Theme.Apply(this);

            _content.BackColor = Theme.Surface;
            _content.Controls.Add(_search);
            _content.Controls.Add(_inventory);
            _content.Controls.Add(_sales);
            _content.Controls.Add(_settings);

            _rail.AddItem("\uE721", "Search");
            _rail.AddItem("\uE8F1", "Inventory");
            _rail.AddItem("\uE81C", "Sales");
            _rail.AddItem("\uE713", "Settings");

            // Leaving Inventory with unsaved edits asks first
            _rail.CanLeave = target => _rail.SelectedIndex != 1 || target == 1 || _inventory.ConfirmLeave();
            _rail.SelectedIndexChanged += (s, e) => ShowView(_rail.SelectedIndex);

            // Add order matters for docking: content first, then the rail, then the app bar
            Controls.Add(_content);
            Controls.Add(_rail);
            Theme.AddAppBar(this, "Staff", "Search stock, manage items and review sales");
            _content.BringToFront();

            // Closing the window with unsaved edits asks first
            FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.WindowsShutDown) return;
                if (!_inventory.ConfirmLeave()) e.Cancel = true;
            };

            Theme.Finish(this, escapeCloses: false);
            Shown += (s, e) => _search.FocusSearch();

            _rail.SelectedIndex = 0;
        }

        private void ShowView(int index)
        {
            _search.Visible = index == 0;
            _inventory.Visible = index == 1;
            _sales.Visible = index == 2;
            _settings.Visible = index == 3;

            // Refresh the view that was just opened
            switch (index)
            {
                case 0: _search.Reload(); break;
                case 1: _inventory.Reload(); break;
                case 2: _sales.Reload(); break;
                case 3: _settings.Reload(); break;
            }
        }
    }
}