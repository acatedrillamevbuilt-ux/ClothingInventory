namespace ClothingInventory.Forms
{
    public partial class StartForm : Form
    {
        // For a class project a fixed PIN is fine. A real system would store hashed credentials.
        private const string StaffPin = "1234";

        public StartForm()
        {
            InitializeComponent();

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
            string? pin = PromptForPin();
            if (pin == null) return;   // user cancelled

            if (pin != StaffPin)
            {
                MessageBox.Show("Incorrect PIN.", "Access denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var form = new StaffSearchForm();
            form.ShowDialog(this);
        }

        private string? PromptForPin()
        {
            using var dlg = new Form
            {
                Text = "Staff Login",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(260, 110)
            };

            var lbl = new Label { Text = "Enter staff PIN:", Left = 15, Top = 15, AutoSize = true };
            var txt = new TextBox { Left = 15, Top = 40, Width = 230, UseSystemPasswordChar = true };
            var ok = new Button { Text = "OK", Left = 90, Top = 72, DialogResult = DialogResult.OK };

            dlg.Controls.AddRange(new Control[] { lbl, txt, ok });
            dlg.AcceptButton = ok;

            return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
        }
    }
}