using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    public class FeatureProcessLinkConnection : FeatureSimulatorBase
    {
        /// <summary>
        /// Initialize this feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = PaneJitParts;
        }

        [EventCatch(TokenID = TokensGeneral.PartsSelectChanged)]
        public void PartsSelected(EventTokenPartsSelectChangedTrigger token)
        {
            var tars = token.PartStates
                .Where(a => a.sw)
                .Where(a => a.parts is PartsJitProcess)
                .Select(a => (PartsJitProcess)a.parts);

            foreach (var tarProcParts in LoopUtil<PartsJitProcess>.From(tars, out var lc))
            {
                lc.DoFirstOneTime(() =>
                {
                    Parts.RemoveLayereParts(Pane.Target, LAYER.JitProcessConnectorFrom, LAYER.JitProcessConnectorTo);    // Delete the all connector Parts
                });
                var cf = new PartsConnectorFrom
                {
                    PositionerX = ConnectorPositionerX,
                    PositionerY = ConnectorPositionerY,
                };
                cf.CoderX = cf.ConnectorCoderX;
                cf.CoderY = cf.ConnectorCoderY;
                cf.Location = CodePos<PartsJitProcess, Angle>.From(tarProcParts, Angle.Zero);
                Parts.Add(Pane.Target, cf, LAYER.JitProcessConnectorFrom);
            }
        }

        const double Root2 = 1.414213562373095048801688724209;

        public LayoutX ConnectorPositionerX(CodeX<PartsJitProcess> x, CodeY<Angle> y)
        {
            var P = x.Cx;   // PartsJitProcess
            var A = y.Cy;   // Angle
            var lw = P.PositionerX(CodeX<Distance>.From(P.Width / 2 + PartsConnectorFrom.Width / 2), CodeY<Distance>.From(P.Height / 2 + PartsConnectorFrom.Height / 2));
            var lpos = P.GetLayoutPos();    // Parts Location
            var lois = GeoEu.GetLocationOfInscribedSquareInCircle(A); // Connector Location Unit (0.7 times)

            return
                lpos.X          // Parts Location
                + lw * Root2    // R (Circle of the four vertices）
                * lois.X;       // Location of inscribed square in circle
        }

        public LayoutY ConnectorPositionerY(CodeX<PartsJitProcess> x, CodeY<Angle> y)
        {
            var P = x.Cx;   // PartsJitProcess
            var A = y.Cy;   // Angle
            var lh = P.PositionerY(CodeX<Distance>.From(P.Width / 2 + PartsConnectorFrom.Width / 2), CodeY<Distance>.From(P.Height / 2 + PartsConnectorFrom.Height / 2));
            var lpos = P.GetLayoutPos();    // Parts Location
            var lois = GeoEu.GetLocationOfInscribedSquareInCircle(A);   // Connector Location Unit (0.7 times)

            return
                lpos.Y          // Parts Location
                + lh * Root2      // R (Circle of the four vertices）
                * lois.Y;       // Location of inscribed square in circle
        }
    }

    /// <summary>
    /// Process Link Connector (From)
    /// </summary>
    public class PartsConnectorFrom : PartsBase<PartsJitProcess, Angle>, ISelectableParts, IMovableParts
    {
        /// <summary>
        /// Parts Size (Horizontal)
        /// </summary>
        public static Distance Width { get; set; } = Distance.FromMeter(0.5);

        /// <summary>
        /// Parts Size (Vertical)
        /// </summary>
        public static Distance Height { get; set; } = Distance.FromMeter(0.5);

        /// <summary>
        /// Parts Select State
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Parts move origin location
        /// </summary>
        public CodePos<PartsJitProcess, Angle> OriginalPosition { get; private set; } = null;

        public CodeX<PartsJitProcess> ConnectorCoderX(LayoutX x, LayoutY y)
        {
            return Location.X;
        }

        public CodeY<Angle> ConnectorCoderY(LayoutX x, LayoutY y)
        {
            var lpos = GetLayoutPos(); // Parts Location
            return new CodeY<Angle>
            {
                Cy = GeoEu.Angle(0.0, 0.0, x.Lx - lpos.X.Lx, -(y.Ly - lpos.Y.Ly)),
            };
        }


        public void SaveLocationAsOrigin()
        {
            OriginalPosition = Location;
        }

        public bool IsMoved()
        {
            return !OriginalPosition.Equals(Location);
        }

        public void Move(IDrawArea pane, ScreenSize offset)
        {
            Debug.WriteLine(offset);
            var spos0 = GetScreenPos(pane, OriginalPosition);
            var spos1 = spos0 + offset;
            var lpos = LayoutPos.From(pane, spos1);
            var x = CoderX(lpos.X, lpos.Y);
            var y = CoderY(lpos.X, lpos.Y);
            Location = CodePos<PartsJitProcess, Angle>.From(x, y);
        }

        /// <summary>
        /// Parts Selectable size
        /// </summary>
        public ScreenSize SelectableSize { get; protected set; }

        /// <summary>
        /// Calclate score of select area as a Rectangle
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public virtual float SelectingScore(IDrawArea pane, ScreenPos pos)
        {
            var sp = GetScreenPos(pane);
            var len = GeoEu.Length(sp.X.Sx, sp.Y.Sy, pos.X.Sx, pos.Y.Sy) / SelectableSize.Width;
            if (len < 1.0)
            {
                return (float)len;
            }
            else
            {
                return float.PositiveInfinity;
            }
        }

        public override bool IsIn(IDrawArea pane, ScreenRect sr)
        {
            return sr.IsIn(GetScreenPos(pane));
        }

        /// <summary>
        /// Draw Connector From design
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            var spos = GetScreenPos(dp.Pane);
            var P = Location.X.Cx;
            SelectableSize = P.GetScreenPos(dp.Pane, CodePos<Distance, Distance>.From(P.Location.X.Cx + Width, P.Location.Y.Cy + Height)) - P.GetScreenPos(dp.Pane);
            var srect = ScreenRect.FromCS(spos, SelectableSize);

            dp.Graphics.FillRectangle(_(srect), IsSelected ? Colors.Red : Colors.White);
        }
    }
}
