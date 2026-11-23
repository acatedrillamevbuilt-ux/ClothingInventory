using System.Drawing.Drawing2D;

namespace ClothingInventory
{
    // A rounded product picture (or a letter placeholder when there is no photo)
    public sealed class PhotoBox : Control
    {
        private Bitmap? _image;
        private bool _owned;

        public PhotoBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            Size = new Size(120, 120);
            TabStop = false;
        }

        public void ShowStored(string? imageName, string label) =>
            Set(ImageStore.Thumb(imageName, label, Math.Min(Width, Height), 12), false);

        public void ShowFile(string path, string label) =>
            Set(ImageStore.ThumbFromFile(path, label, Math.Min(Width, Height), 12), true);

        public void ShowPlaceholder(string label) => ShowStored("", label);

        private void Set(Bitmap bitmap, bool owned)
        {
            if (_owned) _image?.Dispose();
            _image = bitmap;
            _owned = owned;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Parent?.BackColor ?? Theme.Surface);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (_image != null)
                g.DrawImage(_image, new Rectangle(0, 0, Width, Height));

            using var path = Theme.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 12);
            using var pen = new Pen(Theme.OutlineVariant, 1f);
            g.DrawPath(pen, path);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _owned) _image?.Dispose();
            base.Dispose(disposing);
        }
    }
}