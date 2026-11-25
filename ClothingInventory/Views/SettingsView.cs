using ClothingInventory.Services;

namespace ClothingInventory.Views
{
    public class SettingsView : UserControl
    {
        private readonly PinService _pins = new PinService();

        private readonly Label lblHeading = new() { Name = "lblHeading", Text = "Staff PIN", AutoSize = false };
        private readonly Label lblHelp = new()
        {
            Name = "lblHelp",
            AutoSize = false,
            Text = "The PIN protects the Staff area. Use 4 to 12 digits and avoid obvious ones like 1234 or 0000. " +
                   "After 5 wrong attempts sign-in is locked for a short time."
        };
        private readonly TextBox txtCurrent = new() { Name = "txtCurrentPin", UseSystemPasswordChar = true, MaxLength = 12 };
        private readonly TextBox txtNew = new() { Name = "txtNewPin", UseSystemPasswordChar = true, MaxLength = 12 };
        private readonly TextBox txtConfirm = new() { Name = "txtConfirmPin", UseSystemPasswordChar = true, MaxLength = 12 };
        private readonly Button btnChangePin = new() { Name = "btnChangePin", Text = "Change PIN" };

        public SettingsView()
        {
            Size = new Size(1112, 636);
            BackColor = Theme.Surface;
            Font = Theme.BodyMedium;

            Controls.AddRange(new Control[] { lblHeading, lblHelp, btnChangePin });
            Theme.StyleControls(this);

            lblHeading.Font = Theme.TitleMedium;
            lblHeading.SetBounds(24, 24, 600, 28);
            lblHelp.ForeColor = Theme.OnSurfaceVariant;
            lblHelp.SetBounds(24, 56, 520, 44);

            int y = 116;
            y = Theme.Field(this, "Current PIN", txtCurrent, 24, y, 320);
            y = Theme.Field(this, "New PIN", txtNew, 24, y, 320);
            y = Theme.Field(this, "Confirm new PIN", txtConfirm, 24, y, 320);
            btnChangePin.SetBounds(24, y, 140, 40);

            btnChangePin.Click += btnChangePin_Click;
            Dock = DockStyle.Fill;
        }

        // Called whenever this view is opened: never leave PINs sitting in the boxes
        public void Reload()
        {
            foreach (var box in new[] { txtCurrent, txtNew, txtConfirm })
            {
                box.Clear();
                Theme.SetError(box, null);
            }
        }

        private void btnChangePin_Click(object? sender, EventArgs e)
        {
            foreach (var box in new[] { txtCurrent, txtNew, txtConfirm })
                Theme.SetError(box, null);

            var locked = _pins.LockRemaining();
            if (locked != null)
            {
                Theme.Snack(this,
                    $"Too many wrong attempts. Try again in {PinService.Describe(locked.Value)}.", true);
                return;
            }

            Control? first = null;
            void Fail(Control c, string message)
            {
                Theme.SetError(c, message);
                first ??= c;
            }

            if (txtCurrent.Text == "") Fail(txtCurrent, "Enter the current PIN");

            string? rule = PinService.CheckRules(txtNew.Text);
            if (rule != null) Fail(txtNew, rule);
            if (txtConfirm.Text != txtNew.Text) Fail(txtConfirm, "PINs don't match");

            if (first != null)
            {
                first.Focus();
                return;
            }

            var result = _pins.Verify(txtCurrent.Text, out int attemptsLeft, out TimeSpan lockFor);

            if (result == PinResult.Locked)
            {
                Theme.Snack(this, $"Too many wrong attempts. Locked for {PinService.Describe(lockFor)}.", true);
                return;
            }

            if (result == PinResult.Wrong)
            {
                Theme.SetError(txtCurrent, "Incorrect PIN");
                txtCurrent.Focus();
                Theme.Snack(this, $"{attemptsLeft} attempt(s) left before a short lock.", true);
                return;
            }

            var saved = _pins.SetPin(txtNew.Text);
            Theme.Snack(this, saved.Success ? "PIN changed." : saved.Message, !saved.Success);
            if (saved.Success) Reload();
        }
    }
}