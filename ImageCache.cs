using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace The_Mansion
{
    /*
     * Every picture the game shows goes through here, so each one is read
     * and decoded once and then reused: scene backgrounds, sprites,
     * characters, portraits, item icons, buttons and dice.
     *
     * Pictures are decoded straight away (OnLoad) and frozen, which makes
     * them cheap to share. The cache keeps the most recently used ones;
     * when it is full, the least recently used picture is dropped (big
     * backgrounds would otherwise pile up as you explore).
     */
    internal static class ImageCache
    {
        private const int Capacity = 40;

        private static readonly Dictionary<string, LinkedListNode<(string Key, BitmapImage Image)>> Lookup =
            new(StringComparer.OrdinalIgnoreCase);

        // Most recently used first.
        private static readonly LinkedList<(string Key, BitmapImage Image)> Order = new();

        /*
         * The picture at a path inside the game, e.g. "Assets/UI/look.png".
         * decodeHeight > 0 decodes a big picture at that height instead of its
         * full size (saves memory for character art shown smaller than drawn).
         * Throws if the picture doesn't exist - callers report that.
         */
        public static BitmapImage Get(string path, int decodeHeight = 0)
        {
            string key = decodeHeight > 0 ? $"{path}@{decodeHeight}" : path;

            if (Lookup.TryGetValue(key, out var node))
            {
                Order.Remove(node);
                Order.AddFirst(node);
                return node.Value.Image;
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri($"pack://application:,,,/{path.TrimStart('/')}", UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            if (decodeHeight > 0)
            {
                image.DecodePixelHeight = decodeHeight;
            }
            image.EndInit();
            image.Freeze();

            Lookup[key] = Order.AddFirst((key, image));

            if (Order.Count > Capacity)
            {
                Lookup.Remove(Order.Last!.Value.Key);
                Order.RemoveLast();
            }

            return image;
        }

        // Loads a picture ahead of time (e.g. the scenes next door); errors are ignored here.
        public static void Preload(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                Get(path);
            }
            catch (Exception)
            {
                // Reported properly if and when the picture is actually shown.
            }
        }
    }

    /*
     * Frozen colours. WPF brushes that are frozen can't change, so WPF can
     * share them freely without tracking changes - cheaper than a fresh,
     * live brush every time.
     */
    internal static class Paint
    {
        public static SolidColorBrush Rgb(byte r, byte g, byte b) => Argb(0xFF, r, g, b);

        public static SolidColorBrush Argb(byte a, byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}
