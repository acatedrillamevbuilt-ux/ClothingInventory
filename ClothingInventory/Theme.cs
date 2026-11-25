using System.Drawing.Drawing2D;
using System.Reflection;
using ClothingInventory.Models;

namespace ClothingInventory
{
    public enum M3Button { Filled, Tonal, Outlined, Text, Icon }

    public static class Theme
    {
        // ---------- Colour roles: Material 3 baseline, light scheme ----------
        private static Color Hex(string h) => ColorTranslator.FromHtml(h);

        public static readonly Color Primary = Hex("#6750A4");
        public static readonly Color OnPrimary = Hex("#FFFFFF");
        public static readonly Color PrimaryContainer = Hex("#EADDFF");
        public static readonly Color OnPrimaryContainer = Hex("#21005D");
        public static readonly Color SecondaryContainer = Hex("#E8DEF8");
        public static readonly Color OnSecondaryContainer = Hex("#1D192B");
        public static readonly Color Error = Hex("#B3261E");
        public static readonly Color ErrorContainer = Hex("#F9DEDC");
        public static readonly Color OnErrorContainer = Hex("#410E0B");
        // Custom roles (M3 allows extra colours): success and warning
        public static readonly Color Success = Hex("#146C2E");
        public static readonly Color Warning = Hex("#7A5900");
        public static readonly Color WarningContainer = Hex("#FFDF9E");
        public static readonly Color OnWarningContainer = Hex("#261A00");
        public static readonly Color Surface = Hex("#FEF7FF");
        public static readonly Color SurfaceContainerLow = Hex("#F7F2FA");
        public static readonly Color SurfaceContainerHigh = Hex("#ECE6F0");
        public static readonly Color OnSurface = Hex("#1D1B20");
        public static readonly Color OnSurfaceVariant = Hex("#49454F");
        public static readonly Color Outline = Hex("#79747E");
        public static readonly Color OutlineVariant = Hex("#CAC4D0");
        public static readonly Color InverseSurface = Hex("#322F35");
        public static readonly Color InverseOnSurface = Hex("#F5EFF7");

        // ---------- Typography scale (Segoe UI stands in for Roboto) ----------
        public static readonly Font BodyMedium = new("Segoe UI", 10F);
        public static readonly Font BodyLarge = new("Segoe UI", 11F);
        public static readonly Font LabelMedium = new("Segoe UI Semibold", 9F);
        public static readonly Font LabelSmall = new("Segoe UI", 8F);
        public static readonly Font LabelLarge = new("Segoe UI Semibold", 10F);
        public static readonly Font TitleMedium = new("Segoe UI Semibold", 11F);
        public static readonly Font TitleLarge = new("Segoe UI", 16F);

        // ---------- Icons (Segoe MDL2 Assets ships with Windows 10 and 11) ----------
        private static readonly bool HasIcons =
            FontFamily.Families.Any(f => f.Name == "Segoe MDL2 Assets");
        private static readonly Font IconSmall = new("Segoe MDL2 Assets", 11F);
        private static readonly Font IconLarge = new("Segoe MDL2 Assets", 24F);

        private const TextFormatFlags Center =
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis;
        private const TextFormatFlags LeftMiddle =
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.EndEllipsis;

        // ======================================================================
        //  Helpers
        // ======================================================================
        public static Color Blend(Color back, Color over, double amount)
        {
            int r = (int)(back.R + (over.R - back.R) * amount);
            int g = (int)(back.G + (over.G - back.G) * amount);
            int b = (int)(back.B + (over.B - back.B) * amount);
            return Color.FromArgb(r, g, b);
        }

        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = Math.Min(radius * 2, Math.Min(r.Width, r.Height));
            var p = new GraphicsPath();
            if (d <= 0) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        private static void DoubleBuffer(Control c) =>
            typeof(Control).GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(c, true);

        // ======================================================================
        //  Applying the theme to a form
        // ======================================================================
        public static void Apply(Form form, Color? background = null)
        {
            form.AutoScaleMode = AutoScaleMode.None;
            form.Font = BodyMedium;
            form.BackColor = background ?? Surface;
            form.ForeColor = OnSurface;
            HideDesignerLabels(form);
            StyleChildren(form);
        }

        // For UserControls and panels: styles every control inside them
        public static void StyleControls(Control root) => StyleChildren(root);

        // The unnamed captions from the designer (label1, label2...) are replaced by M3 captions
        private static void HideDesignerLabels(Control root)
        {
            foreach (Control c in root.Controls)
            {
                if (c is Label l && l.Name.StartsWith("label", StringComparison.OrdinalIgnoreCase))
                    l.Visible = false;
                else if (c.HasChildren)
                    HideDesignerLabels(c);
            }
        }

        private static void StyleChildren(Control parent)
        {
            foreach (Control c in parent.Controls.Cast<Control>().ToList())
            {
                switch (c)
                {
                    case Button b: StyleButton(b); break;
                    case DataGridView g: StyleGrid(g); break;
                    case ListBox l: StyleList(l); break;
                    case GroupBox gb: StyleCard(gb); break;
                    case CheckBox cb:
                        cb.FlatStyle = FlatStyle.Standard;
                        cb.Font = BodyMedium;
                        cb.ForeColor = OnSurface;
                        break;
                    case DateTimePicker dtp: dtp.Font = BodyMedium; break;
                    case Label lbl:
                        lbl.Font = BodyMedium;
                        lbl.ForeColor = lbl.Name == "lblStatus" ? OnSurfaceVariant : OnSurface;
                        break;
                }

                if (c.HasChildren) StyleChildren(c);
            }
        }

        // ======================================================================
        //  Buttons: filled, tonal, outlined, text, icon (+ state layers)
        // ======================================================================
        private sealed class ButtonState
        {
            public M3Button Variant = M3Button.Filled;
            public string Glyph = "";
            public bool Hover, Down, KeyFocus;

            public bool Chip, Checked;
            // Action-card mode (Start screen)
            public bool Card;
            public string Title = "", Subtitle = "";
            public Color Container, OnContainer;
        }

        private static readonly Dictionary<Button, ButtonState> States = new();

        private static ButtonState StateOf(Button b)
        {
            if (States.TryGetValue(b, out var existing)) return existing;

            var st = new ButtonState();
            States[b] = st;

            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            DoubleBuffer(b);

            b.MouseEnter += (s, e) => { st.Hover = true; b.Invalidate(); };
            b.MouseLeave += (s, e) => { st.Hover = false; st.Down = false; b.Invalidate(); };
            b.MouseDown += (s, e) => { st.Down = true; st.KeyFocus = false; b.Invalidate(); };
            b.MouseUp += (s, e) => { st.Down = false; b.Invalidate(); };
            b.EnabledChanged += (s, e) => b.Invalidate();
            b.GotFocus += (s, e) => { st.KeyFocus = Control.MouseButtons == MouseButtons.None; b.Invalidate(); };
            b.LostFocus += (s, e) => { st.KeyFocus = false; b.Invalidate(); };
            b.Paint += (s, e) => PaintButton(b, st, e.Graphics);
            b.Disposed += (s, e) => States.Remove(b);
            return st;
        }

        public static void StyleButton(Button b, M3Button? variant = null, string? glyph = null)
        {
            var st = StateOf(b);
            st.Variant = variant ?? VariantFor(b.Name);
            st.Glyph = glyph ?? GlyphFor(b.Name);
            if (st.Variant != M3Button.Icon && b.Height < 40) b.Height = 40;
            b.Invalidate();
        }

        // A large rounded card that works as a button (used on the Start screen)
        public static void StyleActionCard(Button b, string glyph, string title, string subtitle,
                                           Color container, Color onContainer)
        {
            var st = StateOf(b);
            st.Card = true;
            st.Glyph = glyph;
            st.Title = title;
            st.Subtitle = subtitle;
            st.Container = container;
            st.OnContainer = onContainer;
            b.Text = title;
            b.Invalidate();
        }

        // Filter chip: a toggle that filters a list. Call after StyleControls.
        public static void StyleFilterChip(Button b, string text, string glyph)
        {
            var st = StateOf(b);
            st.Chip = true;
            st.Glyph = glyph;
            b.Text = text;
            b.Height = 32;
            b.Invalidate();
        }

        public static bool IsChipChecked(Button b) =>
            States.TryGetValue(b, out var st) && st.Checked;

        public static void SetChipChecked(Button b, bool on)
        {
            if (States.TryGetValue(b, out var st))
            {
                st.Checked = on;
                b.Invalidate();
            }
        }

        private static M3Button VariantFor(string name)
        {
            string n = name.ToLowerInvariant();
            if (n.Contains("cancel") || n.Contains("clear")) return M3Button.Outlined;
            if (n.Contains("restock") || n.Contains("manage") || n.Contains("sales") ||
                n.Contains("lowstock")) return M3Button.Tonal;
            return M3Button.Filled;
        }

        private static string GlyphFor(string name)
        {
            string n = name.ToLowerInvariant();
            if (n.Contains("lowstock")) return "\uE7BA";
            if (n.Contains("manage")) return "\uE713";
            if (n.Contains("sales")) return "\uE81C";
            if (n.Contains("search")) return "\uE721";
            if (n.Contains("restock")) return "\uE710";
            if (n.Contains("confirm")) return "\uE73E";
            if (n.Contains("cancel")) return "\uE711";
            if (n.Contains("clear")) return "\uE894";
            if (n.Contains("update") || n.Contains("save")) return "\uE74E";
            if (n.Contains("add")) return "\uE710";
            return "";
        }

        private static void PaintButton(Button b, ButtonState st, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = b.Parent?.BackColor ?? Surface;
            g.Clear(bg);

            if (st.Card) { PaintCard(b, st, g); return; }
            if (st.Chip) { PaintChip(b, st, g, bg); return; }

            bool enabled = b.Enabled;
            Color fill, content, border = Color.Empty;

            switch (st.Variant)
            {
                case M3Button.Filled: fill = Primary; content = OnPrimary; break;
                case M3Button.Tonal: fill = SecondaryContainer; content = OnSecondaryContainer; break;
                case M3Button.Outlined: fill = bg; content = Primary; border = Outline; break;
                case M3Button.Text: fill = bg; content = Primary; break;
                default: fill = bg; content = OnSurfaceVariant; break;
            }

            if (!enabled)
            {
                fill = st.Variant is M3Button.Filled or M3Button.Tonal ? Blend(bg, OnSurface, 0.12) : bg;
                content = Blend(bg, OnSurface, 0.38);
                if (st.Variant == M3Button.Outlined) border = Blend(bg, OnSurface, 0.12);
            }
            else if (st.Down) fill = Blend(fill, content, 0.12);
            else if (st.Hover) fill = Blend(fill, content, 0.08);

            var r = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
            using (var path = RoundedRect(r, r.Height / 2))
            using (var brush = new SolidBrush(fill))
            {
                g.FillPath(brush, path);
                if (border != Color.Empty)
                {
                    using var pen = new Pen(border, 1f);
                    g.DrawPath(pen, path);
                }
            }

            if (st.KeyFocus && enabled)
            {
                var ring = Rectangle.Inflate(r, -3, -3);
                using var rp = RoundedRect(ring, Math.Max(2, ring.Height / 2));
                using var pen = new Pen(st.Variant == M3Button.Filled ? OnPrimary : Primary, 2f);
                g.DrawPath(pen, rp);
            }

            string glyph = HasIcons ? st.Glyph : "";

            if (st.Variant == M3Button.Icon)
            {
                TextRenderer.DrawText(g, glyph, IconSmall, r, content, Center);
                return;
            }

            if (glyph == "")
            {
                TextRenderer.DrawText(g, b.Text, LabelLarge, r, content, Center);
                return;
            }

            const int iconW = 20, gap = 8;
            int textW = TextRenderer.MeasureText(g, b.Text, LabelLarge,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            int x = (b.Width - (iconW + gap + textW)) / 2;

            TextRenderer.DrawText(g, glyph, IconSmall, new Rectangle(x, 0, iconW, b.Height), content, Center);
            TextRenderer.DrawText(g, b.Text, LabelLarge,
                new Rectangle(x + iconW + gap, 0, textW + 6, b.Height), content, LeftMiddle);
        }

        private static void PaintCard(Button b, ButtonState st, Graphics g)
        {
            Color fill = st.Container;
            if (st.Down) fill = Blend(fill, st.OnContainer, 0.12);
            else if (st.Hover) fill = Blend(fill, st.OnContainer, 0.08);

            var r = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
            using (var path = RoundedRect(r, 28))
            using (var brush = new SolidBrush(fill))
                g.FillPath(brush, path);

            if (HasIcons)
                TextRenderer.DrawText(g, st.Glyph, IconLarge, new Rectangle(24, 20, 48, 48),
                    st.OnContainer, LeftMiddle);

            TextRenderer.DrawText(g, st.Title, TitleLarge,
                new Rectangle(24, b.Height - 82, b.Width - 48, 32), st.OnContainer, LeftMiddle);
            TextRenderer.DrawText(g, st.Subtitle, BodyMedium,
                new Rectangle(24, b.Height - 48, b.Width - 48, 24), st.OnContainer, LeftMiddle);
        }

        private static void PaintChip(Button b, ButtonState st, Graphics g, Color bg)
        {
            bool on = st.Checked;
            Color fill = on ? SecondaryContainer : bg;
            Color content = on ? OnSecondaryContainer : OnSurfaceVariant;
            Color border = on ? Color.Empty : Outline;

            if (st.Down) fill = Blend(fill, content, 0.12);
            else if (st.Hover) fill = Blend(fill, content, 0.08);

            var r = new Rectangle(0, 0, b.Width - 1, b.Height - 1);
            using (var path = RoundedRect(r, 8))
            using (var brush = new SolidBrush(fill))
            {
                g.FillPath(brush, path);
                if (border != Color.Empty)
                {
                    using var pen = new Pen(border, 1f);
                    g.DrawPath(pen, path);
                }
            }

            if (st.KeyFocus)
            {
                var ring = Rectangle.Inflate(r, -3, -3);
                using var rp = RoundedRect(ring, 6);
                using var pen = new Pen(Primary, 2f);
                g.DrawPath(pen, rp);
            }

            // Check mark when the filter is on, otherwise the chip's own icon
            string glyph = HasIcons ? (on ? "\uE73E" : st.Glyph) : "";
            if (glyph == "")
            {
                TextRenderer.DrawText(g, b.Text, LabelLarge, r, content, Center);
                return;
            }

            const int iconW = 18, gap = 8;
            int textW = TextRenderer.MeasureText(g, b.Text, LabelLarge,
                new Size(int.MaxValue, int.MaxValue), TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
            int x = (b.Width - (iconW + gap + textW)) / 2;

            TextRenderer.DrawText(g, glyph, IconSmall, new Rectangle(x, 0, iconW, b.Height), content, Center);
            TextRenderer.DrawText(g, b.Text, LabelLarge,
                new Rectangle(x + iconW + gap, 0, textW + 6, b.Height), content, LeftMiddle);
        }

        // ======================================================================
        //  Outlined text fields (wraps a TextBox or NumericUpDown)
        // ======================================================================
        private sealed class InputHost : Panel
        {
            public Control? Input;
            public bool HasFocus;
            public string? Error;
            public Label? ErrorLabel;
        }

        public static Control OutlineInput(Control input)
        {
            if (input.Parent is InputHost existing) return existing;
            var parent = input.Parent;
            if (parent == null) return input;

            int index = parent.Controls.GetChildIndex(input);
            var host = new InputHost
            {
                Input = input,
                Name = input.Name + "Host",
                Bounds = new Rectangle(input.Left, input.Top, Math.Max(input.Width, 120), 40),
                TabIndex = input.TabIndex,
                BackColor = parent.BackColor
            };
            DoubleBuffer(host);

            input.Font = BodyMedium;
            input.BackColor = parent.BackColor;
            if (input is TextBox tb) tb.BorderStyle = BorderStyle.None;
            if (input is NumericUpDown nud) nud.BorderStyle = BorderStyle.None;

            parent.Controls.Add(host);
            parent.Controls.SetChildIndex(host, index);
            host.Controls.Add(input);                       // moves the input out of its old parent
            input.SetBounds(12, (host.Height - input.Height) / 2, host.Width - 24, input.Height);
            input.Anchor = AnchorStyles.Left | AnchorStyles.Right;

            input.Enter += (s, e) => { host.HasFocus = true; host.Invalidate(); };
            input.Leave += (s, e) => { host.HasFocus = false; host.Invalidate(); };
            input.EnabledChanged += (s, e) => host.Invalidate();
            host.Click += (s, e) => input.Focus();
            host.Paint += (s, e) => PaintInput(host, e.Graphics);

            if (input is NumericUpDown spinner) spinner.ValueChanged += (s, e) => ClearError(host);
            else input.TextChanged += (s, e) => ClearError(host);

            return host;
        }

        private static void ClearError(InputHost host)
        {
            if (host.Error != null && host.Input != null) SetError(host.Input, null);
        }

        // Red outline plus a small message under the field; pass null to clear it
        public static void SetError(Control input, string? message)
        {
            if (input is DropdownField dropdown)
            {
                dropdown.SetError(message);
                return;
            }

            if (input.Parent is not InputHost host) return;

            host.Error = message;
            if (host.ErrorLabel != null)
            {
                host.ErrorLabel.Text = message ?? "";
                host.ErrorLabel.Visible = message != null;
            }
            host.Invalidate();
        }

        private static void PaintInput(InputHost host, Graphics g)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color bg = host.Parent?.BackColor ?? Surface;
            g.Clear(bg);

            bool enabled = host.Input?.Enabled ?? true;
            bool hasError = host.Error != null;

            Color line = !enabled ? Blend(bg, OnSurface, 0.12)
                       : hasError ? Error
                       : host.HasFocus ? Primary
                       : Outline;
            float thickness = hasError || host.HasFocus ? 2f : 1f;

            var r = new Rectangle(1, 1, host.Width - 3, host.Height - 3);
            using var path = RoundedRect(r, 4);
            using var pen = new Pen(line, thickness);
            g.DrawPath(pen, path);
        }

        // Caption above an outlined input; returns the y position for the next field
        public static int Field(Control parent, string caption, Control input, int x, int y, int width)
        {
            parent.Controls.Add(input);
            parent.Controls.Add(new Label
            {
                Text = caption,
                Font = LabelMedium,
                ForeColor = OnSurfaceVariant,
                AutoSize = false,
                TextAlign = ContentAlignment.BottomLeft,
                Bounds = new Rectangle(x, y, width, 18)
            });

            // Dropdowns draw their own outline; text boxes and number boxes are wrapped
            Control host = input is DropdownField ? input : OutlineInput(input);
            host.Bounds = new Rectangle(x, y + 20, width, 40);

            // Supporting text under the field, shown only when there is an error
            var error = new Label
            {
                Font = LabelSmall,
                ForeColor = Error,
                AutoSize = false,
                Visible = false,
                Bounds = new Rectangle(x + 4, y + 62, width - 4, 14)
            };
            parent.Controls.Add(error);

            if (host is InputHost inputHost) inputHost.ErrorLabel = error;
            else if (host is DropdownField dropdown) dropdown.ErrorLabel = error;

            return y + 82;
        }

        // ======================================================================
        //  Cards, lists and data tables
        // ======================================================================
        private static void StyleCard(GroupBox gb)
        {
            string title = gb.Text;
            gb.Text = "";
            gb.FlatStyle = FlatStyle.Standard;
            gb.BackColor = SurfaceContainerLow;
            DoubleBuffer(gb);

            gb.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(gb.Parent?.BackColor ?? Surface);

                var r = new Rectangle(0, 0, gb.Width - 1, gb.Height - 1);
                using (var path = RoundedRect(r, 12))
                using (var brush = new SolidBrush(gb.BackColor))
                    g.FillPath(brush, path);

                TextRenderer.DrawText(g, title, LabelMedium, new Point(20, 16),
                    OnSurfaceVariant, TextFormatFlags.NoPadding);
            };
        }

        private static void StyleList(ListBox lst)
        {
            lst.BorderStyle = BorderStyle.None;
            lst.BackColor = Surface;
            lst.DrawMode = DrawMode.OwnerDrawFixed;
            lst.ItemHeight = 68;
            lst.IntegralHeight = false;
            DoubleBuffer(lst);

            lst.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var back = new SolidBrush(lst.BackColor))
                    g.FillRectangle(back, e.Bounds);

                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                var product = lst.Items[e.Index] as Product;
                string headline = product?.Name ?? lst.Items[e.Index]?.ToString() ?? "";
                string support = product == null ? "" :
                    $"Size {product.Size}  \u00B7  {product.Color}  \u00B7  {product.Price:C}";

                if (selected)
                {
                    using var path = RoundedRect(Rectangle.Inflate(e.Bounds, -6, -4), 16);
                    using var brush = new SolidBrush(SecondaryContainer);
                    g.FillPath(brush, path);
                }

                int textX = e.Bounds.X + 22;
                if (product != null)
                {
                    g.DrawImage(ImageStore.Thumb(product, 48, 10),
                        e.Bounds.X + 16, e.Bounds.Y + (e.Bounds.Height - 48) / 2, 48, 48);
                    textX = e.Bounds.X + 16 + 48 + 14;
                }

                Color main = selected ? OnSecondaryContainer : OnSurface;
                Color sub = selected ? OnSecondaryContainer : OnSurfaceVariant;
                int textW = e.Bounds.Right - textX - 16;

                TextRenderer.DrawText(g, headline, BodyLarge,
                    new Rectangle(textX, e.Bounds.Y + 15, textW, 24), main, LeftMiddle);
                TextRenderer.DrawText(g, support, BodyMedium,
                    new Rectangle(textX, e.Bounds.Y + 38, textW, 20), sub, LeftMiddle);
            };
        }

        private static void StyleGrid(DataGridView g)
        {
            DoubleBuffer(g);
            g.BorderStyle = BorderStyle.None;
            g.BackgroundColor = Surface;
            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            var hs = g.ColumnHeadersDefaultCellStyle;
            hs.BackColor = SurfaceContainerHigh;
            hs.ForeColor = OnSurface;
            hs.Font = LabelLarge;
            hs.SelectionBackColor = SurfaceContainerHigh;
            hs.SelectionForeColor = OnSurface;
            hs.Padding = new Padding(16, 0, 8, 0);
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersHeight = 48;

            g.RowHeadersVisible = false;
            g.RowTemplate.Height = 52;
            g.AllowUserToResizeRows = false;
            g.CellBorderStyle = DataGridViewCellBorderStyle.None;   // we draw the dividers ourselves
            g.GridColor = OutlineVariant;

            var ds = g.DefaultCellStyle;
            ds.BackColor = Surface;
            ds.ForeColor = OnSurface;
            ds.Font = BodyMedium;
            ds.SelectionBackColor = SecondaryContainer;
            ds.SelectionForeColor = OnSecondaryContainer;
            ds.Padding = new Padding(16, 0, 8, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Surface;

            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // One light divider under every row
            g.RowPostPaint += (s, e) =>
            {
                using var pen = new Pen(OutlineVariant);
                int y = e.RowBounds.Bottom - 1;
                e.Graphics.DrawLine(pen, e.RowBounds.Left, y, e.RowBounds.Right, y);
            };
        }

        // Shows "Out of stock" (red) and "N left" (amber) as chips in a column named Quantity
        // Draws "Out of stock" (red) and "N left" (amber) chips over a column named Quantity
        public static void ShowStockChips(DataGridView g, int lowLevel = 5)
        {
            g.RowPostPaint += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count) return;
                if (!g.Columns.Contains("Quantity")) return;

                var column = g.Columns["Quantity"];
                if (column == null || !column.Visible) return;
                if (g.Rows[e.RowIndex].Cells[column.Index].Value is not int q || q > lowLevel) return;
                if (g.Rows[e.RowIndex].DataBoundItem is Product delistedCheck && !delistedCheck.IsActive) return;

                var cell = g.GetCellDisplayRectangle(column.Index, e.RowIndex, false);
                if (cell.Width <= 0 || cell.Height <= 0) return;

                bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
                Color rowColor = selected ? SecondaryContainer : Surface;
                Color container = q <= 0 ? ErrorContainer : WarningContainer;
                Color on = q <= 0 ? OnErrorContainer : OnWarningContainer;
                string text = q <= 0 ? "Out of stock" : $"{q} left";

                var size = TextRenderer.MeasureText(text, LabelMedium);
                var chip = new Rectangle(cell.X + 12, cell.Y + (cell.Height - 28) / 2, size.Width + 24, 28);

                var gr = e.Graphics;
                var saved = gr.Save();
                gr.SmoothingMode = SmoothingMode.AntiAlias;

                // Cover the plain number with the row colour (the 1px divider stays visible)
                using (var back = new SolidBrush(rowColor))
                    gr.FillRectangle(back, cell.X, cell.Y, cell.Width, cell.Height - 1);

                using (var path = RoundedRect(chip, 8))
                using (var brush = new SolidBrush(container))
                    gr.FillPath(brush, path);
                TextRenderer.DrawText(gr, text, LabelMedium, chip, on, Center);

                gr.Restore(saved);
            };
        }

        // ======================================================================
        //  Top app bar, snackbar, dialogs
        // ======================================================================
        public static void AddAppBar(Form form, string title, string subtitle = "", bool back = true)
        {
            var bar = new Panel { Name = "m3AppBar", Dock = DockStyle.Top, Height = 64, BackColor = Surface };
            DoubleBuffer(bar);

            int left = 24;
            if (back)
            {
                var btn = new Button
                {
                    Name = "btnBack",
                    Text = "Back",
                    AccessibleName = "Back",
                    Size = new Size(40, 40),
                    Location = new Point(12, 12)
                };
                bar.Controls.Add(btn);
                StyleButton(btn, M3Button.Icon, "\uE72B");
                btn.Click += (s, e) => form.Close();
                left = 64;
            }

            bar.Controls.Add(new Label
            {
                Text = title,
                Font = TitleLarge,
                ForeColor = OnSurface,
                AutoSize = true,
                Location = new Point(left, subtitle == "" ? 17 : 8)
            });

            if (subtitle != "")
                bar.Controls.Add(new Label
                {
                    Text = subtitle,
                    Font = BodyMedium,
                    ForeColor = OnSurfaceVariant,
                    AutoSize = true,
                    Location = new Point(left + 1, 38)
                });

            bar.Paint += (s, e) =>
            {
                using var pen = new Pen(OutlineVariant);
                e.Graphics.DrawLine(pen, 0, bar.Height - 1, bar.Width, bar.Height - 1);
            };

            form.Controls.Add(bar);
        }

        // Short message at the bottom of the window; disappears after a few seconds
        public static void Snack(Control from, string message, bool isError = false)
        {
            var form = from.FindForm();
            if (form == null) return;

            foreach (Control old in form.Controls.Find("m3Snackbar", false))
            {
                form.Controls.Remove(old);
                old.Dispose();
            }

            int textLeft = HasIcons ? 46 : 16;
            int maxW = Math.Max(280, form.ClientSize.Width - 48);
            var measured = TextRenderer.MeasureText(message, BodyMedium,
                new Size(maxW - textLeft - 16, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            int w = Math.Min(maxW, Math.Max(280, measured.Width + textLeft + 16));
            int h = Math.Max(48, measured.Height + 28);

            var bar = new Panel
            {
                Name = "m3Snackbar",
                Size = new Size(w, h),
                BackColor = InverseSurface,
                Anchor = AnchorStyles.Bottom
            };
            bar.Location = new Point((form.ClientSize.Width - w) / 2, form.ClientSize.Height - h - 24);
            bar.Region = new Region(RoundedRect(new Rectangle(0, 0, w, h), 6));
            DoubleBuffer(bar);

            bar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                if (HasIcons)
                {
                    Color accent = isError ? Hex("#FFB4AB") : Hex("#A6F4B3");
                    TextRenderer.DrawText(g, isError ? "\uE783" : "\uE73E", IconSmall,
                        new Rectangle(12, 0, 26, h), accent, Center);
                }
                TextRenderer.DrawText(g, message, BodyMedium,
                    new Rectangle(textLeft, 0, w - textLeft - 14, h), InverseOnSurface,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix |
                    TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            };

            form.Controls.Add(bar);
            bar.BringToFront();

            var timer = new System.Windows.Forms.Timer { Interval = 3500 };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();
                if (!bar.IsDisposed)
                {
                    form.Controls.Remove(bar);
                    bar.Dispose();
                }
            };
            timer.Start();
        }

        public static bool Confirm(IWin32Window owner, string title, string message,
                           string confirmText = "OK", string cancelText = "Cancel")
        {
            using var dlg = NewDialog(title, message, confirmText, cancelText,
                                      Array.Empty<string>(), out _, null);
            return dlg.ShowDialog(owner) == DialogResult.OK;
        }

        public static string? PromptPassword(IWin32Window owner, string title, string message,
                                             string confirmText = "OK")
        {
            using var dlg = NewDialog(title, message, confirmText, "Cancel",
                                      new[] { "PIN" }, out var boxes, null);
            return dlg.ShowDialog(owner) == DialogResult.OK ? boxes[0].Text : null;
        }

        // Several hidden-text fields. validate returns (fieldIndex, message) for the first problem, or null when OK.
        public static string[]? PromptSecrets(IWin32Window owner, string title, string message,
                                              string confirmText, string[] captions,
                                              Func<string[], (int Field, string Message)?> validate)
        {
            using var dlg = NewDialog(title, message, confirmText, "Cancel", captions, out var boxes, validate);
            return dlg.ShowDialog(owner) == DialogResult.OK
                ? boxes.Select(b => b.Text).ToArray()
                : null;
        }

        private static Form NewDialog(string title, string message, string confirmText, string? cancelText,
                                      string[] captions, out TextBox[] boxes,
                                      Func<string[], (int Field, string Message)?>? validate)
        {
            const int pad = 24, width = 400;

            var dlg = new Form
            {
                Text = "Clothing Store Inventory",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleMode = AutoScaleMode.None,
                Font = BodyMedium,
                BackColor = SurfaceContainerHigh,
                ForeColor = OnSurface,
                ClientSize = new Size(width, 200)
            };

            int y = pad;
            dlg.Controls.Add(new Label
            {
                Text = title,
                Font = TitleLarge,
                AutoSize = false,
                Bounds = new Rectangle(pad, y, width - 2 * pad, 32)
            });
            y += 40;

            var size = TextRenderer.MeasureText(message, BodyMedium,
                new Size(width - 2 * pad, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            dlg.Controls.Add(new Label
            {
                Text = message,
                ForeColor = OnSurfaceVariant,
                AutoSize = false,
                Bounds = new Rectangle(pad, y, width - 2 * pad, size.Height + 4)
            });
            y += size.Height + 20;

            var list = new List<TextBox>();
            foreach (string caption in captions)
            {
                var box = new TextBox { UseSystemPasswordChar = true, MaxLength = 32 };
                list.Add(box);
                y = Field(dlg, caption, box, pad, y, width - 2 * pad);
            }
            if (captions.Length == 0) y += 4;

            int okWidth = Math.Max(88, TextRenderer.MeasureText(confirmText, LabelLarge).Width + 36);
            var ok = new Button
            {
                Name = "btnDialogOk",
                Text = confirmText,
                Size = new Size(okWidth, 40),
                Location = new Point(width - pad - okWidth, y)
            };
            dlg.Controls.Add(ok);
            StyleButton(ok, M3Button.Text, "");

            if (cancelText != null)
            {
                int cancelWidth = Math.Max(88, TextRenderer.MeasureText(cancelText, LabelLarge).Width + 36);
                var cancel = new Button
                {
                    Name = "btnDialogCancel",
                    Text = cancelText,
                    DialogResult = DialogResult.Cancel,
                    Size = new Size(cancelWidth, 40),
                    Location = new Point(ok.Left - 8 - cancelWidth, y)
                };
                dlg.Controls.Add(cancel);
                StyleButton(cancel, M3Button.Text, "");
                dlg.CancelButton = cancel;
            }
            else dlg.CancelButton = ok;

            ok.Click += (s, e) =>
            {
                if (validate != null)
                {
                    foreach (var b in list) SetError(b, null);

                    var problem = validate(list.Select(b => b.Text).ToArray());
                    if (problem != null)
                    {
                        SetError(list[problem.Value.Field], problem.Value.Message);
                        list[problem.Value.Field].Focus();
                        return;
                    }
                }

                dlg.DialogResult = DialogResult.OK;
            };

            dlg.AcceptButton = ok;
            dlg.ClientSize = new Size(width, y + 40 + pad - 8);
            dlg.Shown += (s, e) => list.FirstOrDefault()?.Focus();

            boxes = list.ToArray();
            return dlg;
        }

        private static Form NewDialog(string title, string message, string confirmText,
                                      bool withInput, out TextBox? box)
        {
            const int pad = 24, width = 400;

            var dlg = new Form
            {
                Text = "Clothing Store Inventory",
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false,
                AutoScaleMode = AutoScaleMode.None,
                Font = BodyMedium,
                BackColor = SurfaceContainerHigh,
                ForeColor = OnSurface,
                ClientSize = new Size(width, 200)
            };

            int y = pad;
            dlg.Controls.Add(new Label
            {
                Text = title,
                Font = TitleLarge,
                AutoSize = false,
                Bounds = new Rectangle(pad, y, width - 2 * pad, 32)
            });
            y += 40;

            var size = TextRenderer.MeasureText(message, BodyMedium,
                new Size(width - 2 * pad, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            dlg.Controls.Add(new Label
            {
                Text = message,
                ForeColor = OnSurfaceVariant,
                AutoSize = false,
                Bounds = new Rectangle(pad, y, width - 2 * pad, size.Height + 4)
            });
            y += size.Height + 20;

            TextBox? input = null;
            if (withInput)
            {
                input = new TextBox { UseSystemPasswordChar = true };
                dlg.Controls.Add(input);
                OutlineInput(input).Bounds = new Rectangle(pad, y, width - 2 * pad, 40);
                y += 40 + 24;
            }
            else y += 4;

            var cancel = new Button
            {
                Name = "btnDialogCancel",
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Size = new Size(88, 40)
            };
            int okWidth = Math.Max(88, TextRenderer.MeasureText(confirmText, LabelLarge).Width + 36);
            var ok = new Button
            {
                Name = "btnDialogOk",
                Text = confirmText,
                DialogResult = DialogResult.OK,
                Size = new Size(okWidth, 40)
            };
            ok.Location = new Point(width - pad - ok.Width, y);
            cancel.Location = new Point(ok.Left - 8 - cancel.Width, y);
            dlg.Controls.Add(cancel);
            dlg.Controls.Add(ok);
            StyleButton(cancel, M3Button.Text, "");
            StyleButton(ok, M3Button.Text, "");

            dlg.AcceptButton = ok;
            dlg.CancelButton = cancel;
            dlg.ClientSize = new Size(width, y + 40 + pad - 8);
            dlg.Shown += (s, e) => input?.Focus();

            box = input;
            return dlg;
        }

        // Esc closes the window, and the window cannot shrink below its opening size
        public static void Finish(Form form, bool escapeCloses = true)
        {
            form.KeyPreview = true;
            if (escapeCloses)
                form.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) form.Close(); };
            form.Shown += (s, e) => form.MinimumSize = form.Size;
        }
    }
}