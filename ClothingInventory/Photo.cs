using ClothingInventory.Models;

namespace ClothingInventory
{
    public static class Photos
    {
        // Adds a thumbnail column to a grid of products, hides the technical columns,
        // and fades delisted rows. Call it once, before the grid gets its data.
        public static void AddToGrid(DataGridView g)
        {
            g.RowTemplate.Height = 60;

            g.DataBindingComplete += (s, e) =>
            {
                foreach (var name in new[] { "ImagePath", "IsActive" })
                {
                    var column = g.Columns[name];
                    if (column != null) column.Visible = false;
                }

                if (g.Columns.Count == 0 || g.Columns.Contains("Photo")) return;

                var photo = new DataGridViewImageColumn
                {
                    Name = "Photo",
                    HeaderText = "",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                    Width = 76,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                    ReadOnly = true
                };
                photo.DefaultCellStyle.Padding = new Padding(12, 6, 4, 6);
                g.Columns.Insert(0, photo);
            };

            g.CellFormatting += (s, e) =>
            {
                if (e.RowIndex < 0 || e.RowIndex >= g.Rows.Count || e.ColumnIndex < 0) return;
                if (g.Rows[e.RowIndex].DataBoundItem is not Product p) return;

                if (g.Columns[e.ColumnIndex].Name == "Photo")
                {
                    e.Value = ImageStore.Thumb(p.ImagePath, p.Name, 48, 10);
                    e.FormattingApplied = true;
                    return;
                }

                if (!p.IsActive && e.CellStyle != null)
                    e.CellStyle.ForeColor = Theme.Outline;   // delisted rows look faded
            };
        }
    }
}