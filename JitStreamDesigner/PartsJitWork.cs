// (c) 2020 Manabu Tonosaki
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
    public class PartsJitWork : PartsJitBase
    {
        /// <summary>
        /// Change Base Color
        /// </summary>
        public override Color BaseColor => Colors.Cyan;

        /// <summary>
        /// Drawing main
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            var sc = GetScreenPos(dp.Pane);
            var lsiz = LayoutSize.From(PositionerX(CodeX<Distance>.From(Width), null), PositionerY(null, CodeY<Distance>.From(Height)));
            var ssiz = ScreenSize.From(dp.Pane, lsiz);
            SelectableSize = ssiz;

            if (IsSelected)
            {
                dp.Graphics.FillCircle(_(sc), ssiz.Width, SelectingColor);
                dp.Graphics.DrawCircle(_(sc), ssiz.Width, SelectingColor, 1.0f);
                return;
            }
            dp.Graphics.DrawCircle(_(sc), ssiz.Width, BaseColor, 1.0f);
        }
    }
}
