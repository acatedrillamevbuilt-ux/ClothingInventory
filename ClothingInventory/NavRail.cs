using System.Drawing.Drawing2D;
using System.ComponentModel;
namespace ClothingInventory
{
    // Material 3 navigation rail: icon in a pill, label underneath
    public sealed class NavRail : Control
    {
        private const int ItemHeight = 76, TopPad = 16;

        private static readonly bool HasIcons =
            FontFamily.Families.Any(f => f.Name == "Segoe MDL2 Assets");
        private static readonly Font IconFont = new("Segoe MDL2 Assets", 13F);

        private const TextFormatFlags Flags =
            TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine |
            TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis;

        private readonly List<(string Glyph, string Label)> _items = new();
        private int _selected = -1;
        private int _hover = -1;

        public event EventHandler? SelectedIndexChanged;

        public NavRail()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Width = 88;
            Dock = DockStyle.Left;
            BackColor = Theme.Surface;
            Cursor = Cursors.Hand;
        }

        public void AddItem(string glyph, string label)
        {
            _items.Add((glyph, label));
            Invalidate();
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => _selected;
            set
            {
                if (value == _selected || value < 0 || value >= _items.Count) return;
                _selected = value;
                Invalidate();
                SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private int IndexAt(int y)
        {
            if (y < TopPad - 8) return -1;
            int i = (y - (TopPad - 8)) / ItemHeight;
            return i < _items.Count ? i : -1;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int i = IndexAt(e.Y);
            if (i != _hover) { _hover = i; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = -1;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
            int i = IndexAt(e.Y);
            if (i >= 0) SelectedIndex = i;
        }

        protected override bool IsInputKey(Keys keyData) =>
            keyData is Keys.Up or Keys.Down || base.IsInputKey(keyData);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Down && _selected < _items.Count - 1) SelectedIndex = _selected + 1;
            else if (e.KeyCode == Keys.Up && _selected > 0) SelectedIndex = _selected - 1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            for (int i = 0; i < _items.Count; i++)
            {
                int top = TopPad + i * ItemHeight;
                var pill = new Rectangle((Width - 56) / 2, top, 56, 32);
                bool selected = i == _selected;

                Color fill = Color.Empty;
                if (selected) fill = Theme.SecondaryContainer;
                if (i == _hover)
                    fill = Theme.Blend(selected ? Theme.SecondaryContainer : BackColor, Theme.OnSurface, 0.08);

                if (fill != Color.Empty)
                {
                    using var path = Theme.RoundedRect(pill, 16);
                    using var brush = new SolidBrush(fill);
                    g.FillPath(brush, path);
                }

                Color iconColor = selected ? Theme.OnSecondaryContainer : Theme.OnSurfaceVariant;
                Color textColor = selected ? Theme.OnSurface : Theme.OnSurfaceVariant;

                if (HasIcons)
                    TextRenderer.DrawText(g, _items[i].Glyph, IconFont, pill, iconColor, Flags);

                TextRenderer.DrawText(g, _items[i].Label, Theme.LabelMedium,
                    new Rectangle(0, top + 36, Width, 20), textColor, Flags);
            }

            using var pen = new Pen(Theme.OutlineVariant);
            g.DrawLine(pen, Width - 1, 0, Width - 1, Height);
        }
    }
}