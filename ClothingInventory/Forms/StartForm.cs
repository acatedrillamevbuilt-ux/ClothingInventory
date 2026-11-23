namespace ClothingInventory.Forms
{
    public partial class StartForm : Form
    {
        private const string StaffPin = "1234";

        public StartForm()
        {
            InitializeComponent();

            Text = "Clothing Store Inventory";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            ClientSize = new Size(640, 330);
            Theme.Apply(this);

            lblTitle.Visible = false;   // the app bar replaces this label

            const int margin = 24, gap = 16, top = 96, cardHeight = 200;
            int cardWidth = (ClientSize.Width - 2 * margin - gap) / 2;

            btnCustomer.SetBounds(margin, top, cardWidth, cardHeight);
            btnStaff.SetBounds(margin + cardWidth + gap, top, cardWidth, cardHeight);

            Theme.StyleActionCard(btnCustomer, "\uE7BF", "Customer", "Find items and buy",
                Theme.PrimaryContainer, Theme.OnPrimaryContainer);
            Theme.StyleActionCard(btnStaff, "\uE77B", "Staff", "Search, restock and sales",
                Theme.SecondaryContainer, Theme.OnSecondaryContainer);

            Theme.AddAppBar(this, "Clothing Store Inventory", "Choose how you want to continue", back: false);
            Theme.Finish(this, escapeCloses: false);

            btnCustomer.Click += btnCustomer_Click;
            btnStaff.Click += btnStaff_Click;
        }

        private void btnCustomer_Click(object? sender, EventArgs e)
        {
            //throw new InvalidOperationException("test"); to check if validation works

            using var form = new CustomerForm();
            form.ShowDialog(this);
        }

        private void btnStaff_Click(object? sender, EventArgs e)
        {
            string? pin = Theme.PromptPassword(this, "Staff sign-in",
                "Enter the staff PIN to continue.", "Sign in");
            if (pin == null) return;

            if (pin != StaffPin)
            {
                Theme.Snack(this, "Incorrect PIN. Please try again.", true);
                return;
            }

            using var form = new StaffForm();
            form.ShowDialog(this);
        }
    }
}