using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ClothingInventory.Models;

namespace ClothingInventory
{
    // Saves product photos in an "images" folder next to the database and makes rounded thumbnails
    public static class ImageStore
    {
        public const string Filter = "Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
        private const long MaxSourceBytes = 10 * 1024 * 1024;

        private static readonly string Folder = Path.Combine(AppContext.BaseDirectory, "images");
        private static readonly Dictionary<string, Bitmap> Cache = new();

        private static string PathOf(string name) => Path.Combine(Folder, Path.GetFileName(name));

        // Returns a problem description, or null if the picked file can be used
        public static string? Check(string path)
        {
            try
            {
                var info = new FileInfo(path);
                if (!info.Exists) return "That file no longer exists.";
                if (info.Length > MaxSourceBytes) return "That image is too large (maximum 10 MB).";

                string ext = info.Extension.ToLowerInvariant();
                if (ext is not (".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif"))
                    return "Please choose a JPG, PNG, BMP or GIF image.";

                using var test = LoadCopy(path);   // proves it is a real image
                return null;
            }
            catch
            {
                return "That file couldn't be read as an image.";
            }
        }

        // Copies a picked file into the images folder (resized, saved as JPEG) and returns its new name
        public static string Import(string sourcePath)
        {
            Directory.CreateDirectory(Folder);

            using var original = LoadCopy(sourcePath);
            using var fitted = Fit(original, 800);

            string name = Guid.NewGuid().ToString("N") + ".jpg";
            var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            using var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 88L);

            fitted.Save(PathOf(name), codec, parameters);
            return name;
        }

        public static void Delete(string? name)
        {
            if (string.IsNullOrEmpty(name)) return;

            try
            {
                foreach (var key in Cache.Keys.Where(k => k.Contains(name)).ToList())
                    Cache.Remove(key);

                string path = PathOf(name);
                if (File.Exists(path)) File.Delete(path);
            }
            catch
            {
                // A leftover file is harmless
            }
        }

        // ---------- Thumbnails ----------
        public static Bitmap Thumb(Product p, int size, int radius = 8) =>
            Thumb(p.ImagePath, p.Name, size, radius);

        // A square rounded picture, or a coloured letter when there is no photo
        public static Bitmap Thumb(string? imageName, string label, int size, int radius = 8)
        {
            string initial = Initial(label);
            string key = string.IsNullOrEmpty(imageName)
                ? $"ph|{initial}|{size}|{radius}"
                : $"img|{imageName}|{size}|{radius}";

            if (Cache.TryGetValue(key, out var cached)) return cached;

            Bitmap result;
            Bitmap? photo = string.IsNullOrEmpty(imageName) ? null : TryLoadStored(imageName);
            if (photo != null)
            {
                result = Cover(photo, size, radius);
                photo.Dispose();
            }
            else
            {
                result = Placeholder(initial, size, radius);
            }

            Cache[key] = result;
            return result;
        }

        // Preview of a file that has not been saved yet (not cached; the caller disposes it)
        public static Bitmap ThumbFromFile(string path, string label, int size, int radius = 8)
        {
            try
            {
                using var photo = LoadCopy(path);
                return Cover(photo, size, radius);
            }
            catch
            {
                return Placeholder(Initial(label), size, radius);
            }
        }

        // ---------- Internals ----------
        private static string Initial(string label) =>
            string.IsNullOrWhiteSpace(label) ? "?" : label.Trim().Substring(0, 1).ToUpperInvariant();

        private static Bitmap? TryLoadStored(string name)
        {
            try
            {
                string path = PathOf(name);
                return File.Exists(path) ? LoadCopy(path) : null;
            }
            catch
            {
                return null;
            }
        }

        // Loads an image without keeping the file locked
        private static Bitmap LoadCopy(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var image = Image.FromStream(stream, false, true);
            return new Bitmap(image);
        }

        private static Bitmap Fit(Image source, int max)
        {
            double scale = Math.Min(1.0, (double)max / Math.Max(source.Width, source.Height));
            int w = Math.Max(1, (int)(source.Width * scale));
            int h = Math.Max(1, (int)(source.Height * scale));

            var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, w, h);
            return bmp;
        }

        // Crops to a centred square and rounds the corners
        private static Bitmap Cover(Bitmap source, int size, int radius)
        {
            int side = Math.Min(source.Width, source.Height);
            var crop = new Rectangle((source.Width - side) / 2, (source.Height - side) / 2, side, side);

            using var square = new Bitmap(size, size);
            using (var sg = Graphics.FromImage(square))
            {
                sg.InterpolationMode = InterpolationMode.HighQualityBicubic;
                sg.PixelOffsetMode = PixelOffsetMode.HighQuality;
                sg.DrawImage(source, new Rectangle(0, 0, size, size), crop, GraphicsUnit.Pixel);
            }

            var result = new Bitmap(size, size);
            using var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = Theme.RoundedRect(new Rectangle(0, 0, size - 1, size - 1), radius);
            using var brush = new TextureBrush(square);
            g.FillPath(brush, path);
            return result;
        }

        private static Bitmap Placeholder(string initial, int size, int radius)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            using (var path = Theme.RoundedRect(new Rectangle(0, 0, size - 1, size - 1), radius))
            using (var fill = new SolidBrush(Theme.SecondaryContainer))
                g.FillPath(fill, path);

            using var font = new Font("Segoe UI Semibold", size * 0.4f, FontStyle.Regular, GraphicsUnit.Pixel);
            using var text = new SolidBrush(Theme.OnSecondaryContainer);
            using var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            g.DrawString(initial, font, text, new RectangleF(0, 0, size, size), format);
            return bmp;
        }
    }
}