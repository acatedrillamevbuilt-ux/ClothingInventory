using ClothingInventory.Services;

namespace ClothingInventory.Forms
{
    public partial class StartForm : Form
    {
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
            using var form = new CustomerForm();
            form.ShowDialog(this);
        }

        private void btnStaff_Click(object? sender, EventArgs e)
        {
            var pins = new PinService();

            if (!pins.IsPinSet())
            {
                if (!CreatePin(pins)) return;
            }
            else
            {
                var locked = pins.LockRemaining();
                if (locked != null)
                {
                    Theme.Snack(this,
                        $"Staff sign-in is locked. Try again in {PinService.Describe(locked.Value)}.", true);
                    return;
                }

                string? pin = Theme.PromptPassword(this, "Staff sign-in",
                    "Enter the staff PIN to continue.", "Sign in");
                if (pin == null) return;

                var result = pins.Verify(pin, out int attemptsLeft, out TimeSpan lockFor);

                if (result == PinResult.Locked)
                {
                    Theme.Snack(this,
                        $"Too many wrong attempts. Locked for {PinService.Describe(lockFor)}.", true);
                    return;
                }

                if (result == PinResult.Wrong)
                {
                    Theme.Snack(this,
                        $"Incorrect PIN. {attemptsLeft} attempt(s) left before a short lock.", true);
                    return;
                }
            }

            using var form = new StaffForm();
            form.ShowDialog(this);
        }

        // First use: the owner chooses the staff PIN
        private bool CreatePin(PinService pins)
        {
            var texts = Theme.PromptSecrets(this, "Create a staff PIN",
                "No staff PIN has been set yet. Choose one now; staff will use it to open the Staff area.",
                "Save PIN", new[] { "New PIN", "Confirm PIN" },
                t =>
                {
                    string? rule = PinService.CheckRules(t[0]);
                    if (rule != null) return (0, rule);
                    if (t[0] != t[1]) return (1, "PINs don't match");
                    return null;
                });
            if (texts == null) return false;

            var saved = pins.SetPin(texts[0]);
            if (!saved.Success)
            {
                Theme.Snack(this, saved.Message, true);
                return false;
            }

            Theme.Snack(this, "Staff PIN saved.");
            return true;
        }
    }
}