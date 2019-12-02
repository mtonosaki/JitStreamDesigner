// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    /// <summary>
    /// Process Parts
    /// </summary>
    /// <remarks>
    /// Location = Center
    /// </remarks>
    public class PartsJitProcess : PartsJitBase
    {
        /// <summary>
        /// Process ID same with Jac
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Change Base Color
        /// </summary>
        public override Color BaseColor => base.BaseColor;

        /// <summary>
        /// Drawing main
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            var sc = GetScreenPos(dp.Pane);
            var lsiz = LayoutSize.From(PositionerX(CodeX<Distance>.From(Width), null), PositionerY(null, CodeY<Distance>.From(Height)));
            var ssiz = ScreenSize.From(dp.Pane, lsiz);
            var sr = ScreenRect.FromCWH(sc, ssiz.Width, ssiz.Height);

            dp.Graphics.DrawRectangle(_(sr), GetColor(dp));
        }
    }
}
