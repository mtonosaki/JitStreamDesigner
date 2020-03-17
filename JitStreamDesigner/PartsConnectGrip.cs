using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{

    /// <summary>
    /// A temporary GUI parts of Process Link Connector Grip
    /// </summary>
    public class PartsConnectGrip : PartsJitBase
    {
        public enum Designs
        {
            OUT, IN,
        }

        /// <summary>
        /// Linkage kind IN/OUT
        /// </summary>
        public Designs Design { get; set; }

        public Angle Angle { get; set; }

        /// <summary>
        /// Target Process Parts
        /// </summary>
        public PartsJitProcess TargetProcess { get; set; }

        /// <summary>
        /// Drawing main
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            var lpos = GetLayoutPos();
            var sc = ScreenPos.From(dp.Pane, lpos);
            var lsiz = LayoutSize.From(PositionerX(CodeX<Distance>.From(Width), null), PositionerY(null, CodeY<Distance>.From(Height)));
            var ssiz = ScreenSize.From(dp.Pane, lsiz);
            var sr = ScreenRect.FromCWH(sc, ssiz.Width, ssiz.Height);

            if (IsSelected)
            {
                // draw the requested position indicator (on the inscribed square in the circle)
                var A = GetAngle(lpos);
                var lposIn = GetLayoutPos(A);
                var scIn = ScreenPos.From(dp.Pane, lposIn);
                var srIn = ScreenRect.FromCWH(scIn, ssiz.Width, ssiz.Height);
                dp.Graphics.DrawLine(_(scIn), _(sc), ConnectingColor);
                if( Design == Designs.IN)
                {
                    dp.Graphics.FillRectangle(_(srIn), Colors.DarkGray);
                } else
                {
                    dp.Graphics.DrawRectangle(_(srIn), Colors.DarkGray);
                }

                // draw selected color
                dp.Graphics.DrawRectangle(_(sr), SelectingColor, 4f);
                dp.Graphics.FillRectangle(_(sr), SelectingColor);
            }
            if (Design == Designs.IN)
            {
                dp.Graphics.FillRectangle(_(sr), GetColor(dp));
            }
            else
            {
                dp.Graphics.DrawRectangle(_(sr), GetColor(dp));
            }

            SelectableSize = sr.ToSize();
        }

        public LayoutPos GetLayoutPos(Angle A)
        {
            var lw = TargetProcess.PositionerX(CodeX<Distance>.From(TargetProcess.Width / 2 + Width / 2), CodeY<Distance>.From(TargetProcess.Height / 2 + Height / 2));
            var lh = TargetProcess.PositionerY(CodeX<Distance>.From(TargetProcess.Width / 2 + Width / 2), CodeY<Distance>.From(TargetProcess.Height / 2 + Height / 2));
            var lpos = TargetProcess.GetLayoutPos();    // Parts Location
            var lois = GeoEu.GetLocationOfInscribedSquareInCircle(A); // Connector Location Unit (0.7 times)
            return LayoutPos.From(
                lpos.X                      // Parts Location
                    + lw * MathUtil.Root2   // R (Circle of the four vertices）
                    * lois.X,               // Location of inscribed square in circle
                lpos.Y                      // Parts Location
                    + lh * MathUtil.Root2   // R (Circle of the four vertices）
                    * lois.Y                // Location of inscribed square in circle
            );
        }

        public Angle GetAngle(LayoutPos lpos)
        {
            var lpos0 = TargetProcess.GetLayoutPos(); // Parts Location
            return GeoEu.Angle(0.0, 0.0, lpos.X.Lx - lpos0.X.Lx, -(lpos.Y.Ly - lpos0.Y.Ly));
        }

        public Angle GetAngle()
        {
            return GetAngle(GetLayoutPos());
        }

        public CodePos<Distance, Distance> SetLocation(Angle A)
        {
            var lpos = GetLayoutPos(A);
            Location = CodePos<Distance, Distance>.From(TargetProcess.CoderX(lpos.X, lpos.Y), TargetProcess.CoderY(lpos.X, lpos.Y));
            return Location;
        }
    }
}
