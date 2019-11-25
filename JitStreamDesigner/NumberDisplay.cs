using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui;
using Tono.Gui.Uwp;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    /// <summary>
    /// An original bitmap numbers drawing program
    /// </summary>
    public class NumberDisplay
    {
        /// <summary>
        /// Filen name prefix of image file
        /// </summary>
        public string FileNamePrefix => "s13-";

        public event EventHandler Loaded;
        public bool IsLoaded { get => LoadStatus == 1; }

        public TGuiView View { get; set; }

        private static readonly Dictionary<char, CanvasBitmap> Bitmaps = new Dictionary<char, CanvasBitmap>();
        private static int LoadStatus = -1;

        public async Task Load7Seg()
        {
            if (LoadStatus != -1)
            {
                return;
            }
            else
            {
                LoadStatus = 0;
            }
            var seq = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ',', ':', ';', '*' };
            var tasks = new List<Task<CanvasBitmap>>{
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}0.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}1.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}2.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}3.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}4.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}5.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}6.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}7.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}8.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}9.png")).AsTask(),
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}D.png")).AsTask(),       // Dot
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}DDark.png")).AsTask(),   // Dot(Dark)
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}C.png")).AsTask(),       // Colon
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}CDark.png")).AsTask(),   // Colon(Dark)
                    CanvasBitmap.LoadAsync(View.Canvas, new Uri($"ms-appx:///Assets/{FileNamePrefix}Dark.png")).AsTask(),    // All (Dark)
            };
            await Task.WhenAll(tasks);
            for (var i = 0; i < tasks.Count; i++)
            {
                Bitmaps[seq[i]] = tasks[i].Result;
            }
            LoadStatus = 1;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private static readonly Dictionary<char, double> SpaceAdjust = new Dictionary<char, double>
        {
            ['.'] = ScreenX.From(-1.0),
            [','] = ScreenX.From(-1.0),
            ['!'] = ScreenX.From(-0.5),
            ['\b'] = ScreenX.From(-1.0 - 1.0 / 4.0),
        };

        /// <summary>
        /// Draw 7-seg image
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="format"></param>
        /// <remarks>
        /// Format:
        /// 0-9 ... draw number
        /// *   ... draw Dark 8
        /// .   ... draw Dot at last position
        /// ,   ... draw Dot at last position (dark)
        /// :   ... draw colon
        /// ;   ... draw colon(dark)
        /// !   ... draw half space
        /// _   ... draw space
        /// \b  ... back space (1/4)
        /// </remarks>
        public void Draw(DrawProperty dp, string format, ScreenPos pos, ScreenY height)
        {
            if (LoadStatus != 1)
            {
                return;
            }
            var x = ScreenX.From(0);
            var sz0 = ScreenSize.From(Bitmaps['*'].SizeInPixels.Width, Bitmaps['*'].SizeInPixels.Height);
            var z = height / sz0.Height;
            var sz = sz0 * z;
            var sr = ScreenRect.FromLTWH(0, 0, sz.Width, sz.Height);

            foreach (var c in format)
            {
                if (SpaceAdjust.TryGetValue(c, out var adjust))
                {
                    x += sz.Width * adjust;
                }
                var bmp = Bitmaps.GetValueOrDefault(c, null);
                if (bmp != null)
                {
                    dp.Graphics.DrawImage(bmp, _(sr + pos + x));
                }
                x += sz.Width;
            }
        }
    }
}
