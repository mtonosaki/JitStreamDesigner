using System;
using System.Diagnostics;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    public class PartsJitProcessLink : PartsJitBase
    {
        public enum States
        {
            LINE, HOVER, SELECTING,
        };
        public States State { get; set; } = States.LINE;

        public PartsJitProcess ProcessFrom { get; set; }

        public PartsJitProcess ProcessTo { get; set; }

        private Angle _angle0 = Angle.Zero;
        private ScreenPos _p0, _p1;

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

            _angle0 = GeoEu.Angle(sc0.X.Sx, sc0.Y.Sy, sc1.X.Sx, sc1.Y.Sy);
            var k0 = GeoEu.GetLocationOfInscribedSquareInCircle(_angle0);
            var sr0 = ScreenRect.FromCS(ScreenPos.From(sc0.X + k0.X * (spsz0.Width + scsz.Width) / MathUtil.Root2, sc0.Y + -k0.Y * (spsz0.Height + scsz.Height) / MathUtil.Root2), scsz);

            var a1 = GeoEu.Angle(sc1.X.Sx, sc1.Y.Sy, sc0.X.Sx, sc0.Y.Sy);
            var k1 = GeoEu.GetLocationOfInscribedSquareInCircle(a1);
            var sr1 = ScreenRect.FromCS(ScreenPos.From(sc1.X + k1.X * (spsz1.Width + scsz.Width) / MathUtil.Root2, sc1.Y + -k1.Y * (spsz1.Height + scsz.Height) / MathUtil.Root2), scsz);
            _p0 = sr0.C + ScreenPos.From(k0.X * sr0.Width / MathUtil.Root2, -k0.Y * sr0.Height / MathUtil.Root2);
            _p1 = sr1.C;

            // from: on the grip edge
            switch (State)
            {
                case States.SELECTING:
                    dp.Graphics.DrawLine(_(_p0), _(_p1), Colors.Red);
                    break;
                case States.HOVER:
                    dp.Graphics.DrawLine(_(_p0), _(_p1), Colors.Cyan);
                    break;
                default:
                    dp.Graphics.DrawLine(_(_p0), _(_p1), Colors.White);
                    break;
            }
            dp.Graphics.DrawRectangle(_(sr0), Colors.White);
            dp.Graphics.FillRectangle(_(sr1), Colors.White);
        }

        public override float SelectingScore(IDrawArea pane, ScreenPos pos)
        {
            var width = 6.0f;
            var l = _p0.LengthTo(_p1);
            var a = _p0.AngleTo(_p1);
            var ml = _p0.LengthTo(pos);
            var ma = _p0.AngleTo(pos);
            GeoEu.Position(0, 0, ma - a, ml, out var mpx, out var mpy);
            
            var ret =  (float)(Math.Abs(mpy) / width);
            if (ret <= 1.0f)
            {
                if( mpx < 0 || mpx > l)
                {
                    ret = float.PositiveInfinity;
                }
            }
            return ret;
        }

        /// <summary>
        /// In start point or end point
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="sr"></param>
        /// <returns></returns>
        public override bool IsIn(IDrawArea pane, ScreenRect sr)
        {
            return sr.IsIn(_p0) | sr.IsIn(_p1);
        }

        public override ScreenPos GetScreenPos(IDrawArea pane, CodePos<Distance, Distance> codepos)
        {
            throw new NotSupportedException();
        }
    }
}
