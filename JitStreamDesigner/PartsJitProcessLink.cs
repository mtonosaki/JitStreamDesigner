using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    public class PartsJitProcessLink : PartsJitBase
    {
        public PartsJitProcess ProcessFrom { get; set; }

        public PartsJitProcess ProcessTo { get; set; }

        /// <summary>
        /// Visualize
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            // Connector grip size
            var lcsz = LayoutSize.From(PositionerX(CodeX<Distance>.From(Width), null), PositionerY(null, CodeY<Distance>.From(Height)));
            var scsz = ScreenSize.From(dp.Pane, lcsz);

            // Process Size
            var spsz0 = ScreenSize.From(dp.Pane, LayoutSize.From(PositionerX(CodeX<Distance>.From(ProcessFrom.Width), null), PositionerY(null, CodeY<Distance>.From(ProcessFrom.Height))));
            var spsz1 = ScreenSize.From(dp.Pane, LayoutSize.From(PositionerX(CodeX<Distance>.From(ProcessTo.Width), null), PositionerY(null, CodeY<Distance>.From(ProcessTo.Height))));
            var sc0 = ProcessFrom.GetScreenPos(dp.Pane);
            var sc1 = ProcessTo.GetScreenPos(dp.Pane);

            var a0 = GeoEu.Angle(sc0.X.Sx, sc0.Y.Sy, sc1.X.Sx, sc1.Y.Sy);
            var k0 = GeoEu.GetLocationOfInscribedSquareInCircle(a0);
            var sr0 = ScreenRect.FromCS(ScreenPos.From(sc0.X + k0.X * (spsz0.Width + scsz.Width) / MathUtil.Root2, sc0.Y + -k0.Y * (spsz0.Height + scsz.Height) / MathUtil.Root2), scsz);

            var a1 = GeoEu.Angle(sc1.X.Sx, sc1.Y.Sy, sc0.X.Sx, sc0.Y.Sy);
            var k1 = GeoEu.GetLocationOfInscribedSquareInCircle(a1);
            var sr1 = ScreenRect.FromCS(ScreenPos.From(sc1.X + k1.X * (spsz1.Width + scsz.Width) / MathUtil.Root2, sc1.Y + -k1.Y * (spsz1.Height + scsz.Height) / MathUtil.Root2), scsz);

            dp.Graphics.DrawLine(_(sr0.C + ScreenPos.From(k0.X * sr0.Width / MathUtil.Root2, -k0.Y * sr0.Height / MathUtil.Root2)), _(sr1.C), Colors.White);
            dp.Graphics.DrawRectangle(_(sr0), Colors.White);
            dp.Graphics.FillRectangle(_(sr1), Colors.White);
        }
    }
}
