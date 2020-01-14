// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas.Text;
using System;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using Windows.UI.Text;

namespace JitStreamDesigner
{
    /// <summary>
    /// Clock position visualize parts
    /// </summary>
    public class PartsClock : PartsBase<ScreenX, ScreenY>
    {
        public ScreenY Height { get; set; } = ScreenY.From(32);
        public FeatureClock Parent { get; set; }

        /// <summary>
        /// should be set instance by owner feature
        /// </summary>
        public NumberDisplay Seg7 { get; set; }

        public override void Draw(DrawProperty dp)
        {
            if (Seg7?.IsLoaded == false)
            {
                return;
            }

            var pos = dp.PaneRect.RB - ScreenSize.From(420, 40);
            var now = Parent.Now;

            Seg7.Draw(dp, "****,!**,!**_**\b;\b**\b;\b**", pos, Height);
            if (now != DateTime.MinValue)
            {
                var timestr = $"{now.Year}.!{StrUtil.Right($"_{now.Month}", 2)}.!{StrUtil.Right($"_{now.Day}", 2)}_{StrUtil.Right($"_{now.Hour}", 2)}\b:\b{StrUtil.Right($"0{now.Minute}", 2)}\b:\b{StrUtil.Right($"0{now.Second}", 2)}";
                Seg7.Draw(dp, timestr, pos, Height);

                // Sim Time Caption
                dp.Graphics.DrawText($"Sim time :", pos.X - ScreenX.From(8), pos.Y + ScreenY.From(20), Colors.Cyan, new CanvasTextFormat
                {
                    FontFamily = "Tahoma",
                    FontSize = 11f,
                    FontWeight = FontWeights.Normal,
                    HorizontalAlignment = CanvasHorizontalAlignment.Right,
                });
            }

            // Current Local Time
            dp.Graphics.DrawText($"Actual time : {DateTime.Now.ToString(TimeUtil.FormatYMDHMS)}", dp.PaneRect.R - ScreenX.From(64), pos.Y - ScreenY.From(24), Colors.DarkGray, new CanvasTextFormat
            {
                FontFamily = "Tahoma",
                FontSize = 11f,
                FontWeight = FontWeights.Normal,
                HorizontalAlignment = CanvasHorizontalAlignment.Right,
            });
        }
    }
}
