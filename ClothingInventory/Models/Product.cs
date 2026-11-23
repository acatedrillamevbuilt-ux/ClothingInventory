namespace ClothingInventory.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public string Size { get; set; } = "";
        public string Color { get; set; } = "";
        public string Location { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; } = "";   // file name inside the images folder
        public bool IsActive { get; set; } = true;     // false = delisted

        public override string ToString() => $"{Name} - Size {Size}, {Color}";
    }
}