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
    public class PartsJitProcess : PartsJitBase
    {
        public bool IsConnecting { get; set; }

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
            SelectableSize = sr.ToSize();

            if (IsConnecting)
            {
                dp.Graphics.DrawRectangle(_(sr), ConnectingColor, 4f);
                dp.Graphics.FillRectangle(_(sr), ConnectingColor);
                return;
            }
            if (IsSelected)
            {
                dp.Graphics.DrawRectangle(_(sr), SelectingColor, 4f);
                dp.Graphics.FillRectangle(_(sr), SelectingColor);
                return;
            }
            dp.Graphics.DrawRectangle(_(sr), GetColor(dp));
        }
    }
}
