namespace ClothingInventory
{
    // The choices shown in the dropdowns. Edit these lists to add new options.
    public static class Options
    {
        public static readonly string[] Categories =
            { "Tops", "Bottoms", "Outerwear", "Dresses", "Footwear", "Accessories", "Sportswear", "Underwear" };

        public static readonly string[] Sizes =
            { "XS", "S", "M", "L", "XL", "XXL", "One Size", "28", "30", "32", "34", "36", "38", "40" };

        public static readonly string[] Colors =
            { "Black", "White", "Grey", "Navy", "Blue", "Red", "Green", "Yellow", "Orange", "Pink", "Purple", "Brown", "Beige" };

        public static readonly string[] Locations =
            Enumerable.Range(1, 10).Select(i => $"Aisle {i}").Append("Storeroom").ToArray();
    }
}