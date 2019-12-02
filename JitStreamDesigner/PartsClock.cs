// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;

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
            }
        }
    }
}
