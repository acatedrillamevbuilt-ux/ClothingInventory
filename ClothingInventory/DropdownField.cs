using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ClothingInventory
{
    // Material 3 "exposed dropdown": an outlined field with a chevron that opens a menu
    public sealed class DropdownField : Control
    {
        private const TextFormatFlags LeftMiddle =
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis;

        private readonly List<string> _items = new();
        private string _value = "";
        private string? _error;
        private bool _hasFocus, _hover, _open;
        private long _closedAt;
        private ToolStripDropDown? _popup;

        internal Label? ErrorLabel;

        public event EventHandler? ValueChanged;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Placeholder { get; set; } = "Select";

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? Error => _error;

        // The selected text. A value that isn't in the list yet is added, so old records still show.
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Value
        {
            get => _value;
            set
            {
                string v = (value ?? "").Trim();
                if (v != "")
                {
                    int i = _items.FindIndex(x => string.Equals(x, v, StringComparison.OrdinalIgnoreCase));
                    if (i >= 0) v = _items[i];
                    else _items.Add(v);
                }

                if (v == _value) return;
                _value = v;
                if (_error != null) SetError(null);
                Invalidate();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DropdownField()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            Cursor = Cursors.Hand;
            Size = new Size(180, 40);
            Font = Theme.BodyMedium;
        }

        public void SetItems(IEnumerable<string> items)
        {
            _items.Clear();
            _items.AddRange(items);
            Invalidate();
        }

        // Red outline plus a message under the field; pass null to clear it
        public void SetError(string? message)
        {
            _error = message;
            if (ErrorLabel != null)
            {
                ErrorLabel.Text = message ?? "";
                ErrorLabel.Visible = message != null;
            }
            Invalidate();
        }

        // ---------- Painting ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = Parent?.BackColor ?? Theme.Surface;
            g.Clear(bg);

            bool hasError = _error != null;
            bool focused = _hasFocus || _open;

            Color line = !Enabled ? Theme.Blend(bg, Theme.OnSurface, 0.12)
                       : hasError ? Theme.Error
                       : focused ? Theme.Primary
                       : _hover ? Theme.OnSurface
                       : Theme.Outline;

            var r = new Rectangle(1, 1, Width - 3, Height - 3);
            using (var path = Theme.RoundedRect(r, 4))
            using (var pen = new Pen(line, hasError || focused ? 2f : 1f))
                g.DrawPath(pen, path);

            string text = _value != "" ? _value : Placeholder;
            Color textColor = !Enabled ? Theme.Blend(bg, Theme.OnSurface, 0.38)
                            : _value != "" ? Theme.OnSurface
                            : Theme.OnSurfaceVariant;
            TextRenderer.DrawText(g, text, Theme.BodyMedium,
                new Rectangle(12, 0, Width - 44, Height), textColor, LeftMiddle);

            // Chevron (points up while the menu is open)
            int cx = Width - 22, cy = Height / 2, d = _open ? -1 : 1;
            Color chevron = Enabled ? Theme.OnSurfaceVariant : Theme.Blend(bg, Theme.OnSurface, 0.38);
            using var chevronPen = new Pen(chevron, 2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round,
                LineJoin = LineJoin.Round
            };
            g.DrawLines(chevronPen, new[]
            {
                new Point(cx - 5, cy - 2 * d),
                new Point(cx, cy + 2 * d),
                new Point(cx + 5, cy - 2 * d)
            });
        }

        // ---------- Mouse and keyboard ----------
        protected override void OnGotFocus(EventArgs e) { base.OnGotFocus(e); _hasFocus = true; Invalidate(); }
        protected override void OnLostFocus(EventArgs e) { base.OnLostFocus(e); _hasFocus = false; Invalidate(); }
        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _hover = true; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _hover = false; Invalidate(); }
        protected override void OnEnabledChanged(EventArgs e) { base.OnEnabledChanged(e); Invalidate(); }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            if (e.Button == MouseButtons.Left) Toggle();
        }

        protected override bool IsInputKey(Keys keyData) =>
            keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode is Keys.Space or Keys.Enter || (e.Alt && e.KeyCode == Keys.Down))
            {
                e.SuppressKeyPress = true;
                Toggle();
            }
            else if (e.KeyCode == Keys.Down) Step(1);
            else if (e.KeyCode == Keys.Up) Step(-1);
        }

        private void Step(int direction)
        {
            if (_items.Count == 0) return;

            int i = _items.FindIndex(x => x == _value);
            i = i < 0 ? (direction > 0 ? 0 : _items.Count - 1)
                      : Math.Clamp(i + direction, 0, _items.Count - 1);
            Value = _items[i];
        }

        // ---------- The menu ----------
        private void Toggle()
        {
            if (_open) { _popup?.Close(); return; }
            if (Environment.TickCount64 - _closedAt < 250) return;   // the click that just closed it
            if (!Enabled || _items.Count == 0) return;
            ShowPopup();
        }

        private void ShowPopup()
        {
            const int itemHeight = 40;
            int visible = Math.Min(_items.Count, 7);

            var list = new ListBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Theme.SurfaceContainerLow,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = itemHeight,
                IntegralHeight = false,
                Size = new Size(Width, visible * itemHeight),
                Font = Theme.BodyMedium
            };
            list.Items.AddRange(_items.Cast<object>().ToArray());

            int current = _items.FindIndex(x => x == _value);
            int hover = -1;
            if (current > 3) list.TopIndex = current - 3;

            list.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                var g = e.Graphics;

                Color fill = e.Index == current ? Theme.SecondaryContainer
                           : e.Index == hover ? Theme.Blend(list.BackColor, Theme.OnSurface, 0.08)
                           : list.BackColor;
                using (var back = new SolidBrush(fill))
                    g.FillRectangle(back, e.Bounds);

                TextRenderer.DrawText(g, _items[e.Index], Theme.BodyMedium,
                    new Rectangle(e.Bounds.X + 16, e.Bounds.Y, e.Bounds.Width - 32, e.Bounds.Height),
                    e.Index == current ? Theme.OnSecondaryContainer : Theme.OnSurface, LeftMiddle);
            };

            list.MouseMove += (s, e) =>
            {
                int i = list.IndexFromPoint(e.Location);
                if (i != hover) { hover = i; list.Invalidate(); }
            };
            list.MouseLeave += (s, e) => { hover = -1; list.Invalidate(); };
            list.MouseUp += (s, e) =>
            {
                int i = list.IndexFromPoint(e.Location);
                if (e.Button == MouseButtons.Left && i >= 0)
                {
                    Value = _items[i];
                    _popup?.Close();
                }
            };

            var host = new ToolStripControlHost(list)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = list.Size
            };

            var popup = new ToolStripDropDown
            {
                Padding = Padding.Empty,
                AutoClose = true,
                DropShadowEnabled = true,
                BackColor = Theme.SurfaceContainerLow
            };
            popup.Items.Add(host);
            popup.Closed += (s, e) =>
            {
                _open = false;
                _popup = null;
                _closedAt = Environment.TickCount64;
                Invalidate();
                BeginInvoke(new Action(popup.Dispose));
            };

            _popup = popup;
            _open = true;
            Invalidate();
            popup.Show(this, new Point(0, Height + 2));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _popup?.Dispose();
            base.Dispose(disposing);
        }
    }
}