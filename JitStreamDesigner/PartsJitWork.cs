// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Tono.Jit;
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
    public class PartsJitWork : PartsJitBase, IGuiPartsControlCommon
    {
        /// <summary>
        /// Change Base Color
        /// </summary>
        public override Color BaseColor => Colors.Cyan;

        public JitVariable.ChildValueDic ChildVriables => throw new System.NotImplementedException();

        private float SelectR = 0;

        /// <summary>
        /// Selecting Next by GUI
        /// </summary>
        public bool IsSelectingLocation { get; set; }

        public override bool IsSelected
        {
            get => base.IsSelected;
            set
            {
                if (value && value != base.IsSelected)
                {
                    IsSelectingLocation = true;
                    IsCancellingMove = false;
                    ClearOriginalPosition();
                }
                base.IsSelected = value;
            }
        }

        public static readonly float HysteresisLen = 8.0f;

        /// <summary>
        /// Drawing main
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            var sc = GetScreenPos(dp.Pane);
            var lsiz = LayoutSize.From(PositionerX(CodeX<Distance>.From(Width), null), PositionerY(null, CodeY<Distance>.From(Height)));
            var ssiz = ScreenSize.From(dp.Pane, lsiz);
            SelectR = ssiz.Width + 4;

            if (OriginalPosition != null && IsSelected && IsSelectingLocation)
            {
                var col = Color.FromArgb(48, 255, 255, 255);
                var style = new CanvasStrokeStyle
                {
                    DashStyle = CanvasDashStyle.Dash,
                };
                var sc0 = GetScreenPos(dp.Pane, OriginalPosition);
                var len = sc0.LengthTo(sc);
                if (len > HysteresisLen)
                {
                    var sc00 = GeoEu.Position((sc0.X, sc0.Y), sc0.AngleTo(sc), ssiz.Width + 4);
                    var sc10 = GeoEu.Position((sc.X, sc.Y), sc.AngleTo(sc0), ssiz.Width + 8);
                    dp.Graphics.DrawLine((float)sc00.X, (float)sc00.Y, (float)sc10.X, (float)sc10.Y, col, 1.0f, style);
                }
                dp.Graphics.DrawCircle(sc0.X, sc0.Y, ssiz.Width, new CanvasSolidColorBrush(dp.Canvas, col), 1.0f, style);
            }

            if (IsSelected)
            {
                dp.Graphics.FillCircle(_(sc), SelectR, SelectingColor);
            }
            dp.Graphics.DrawCircle(_(sc), ssiz.Width, BaseColor, 1.0f);
        }

        public override float SelectingScore(IDrawArea pane, ScreenPos pos)
        {
            return (float)GetScreenPos(pane).LengthTo(pos) / SelectR;
        }
    }
}
