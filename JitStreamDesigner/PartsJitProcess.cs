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
    public class PartsJitProcess : PartsJitBase, ISelectableParts, IMovableParts
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
        /// Parts Select State
        /// </summary>
        public bool IsSelected { get; set; }

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

            if (IsSelected)
            {
                dp.Graphics.DrawRectangle(_(sr), SelectingColor, 4f);
            }
            dp.Graphics.DrawRectangle(_(sr), GetColor(dp));

            SelectableSize = sr.ToSize();
        }

        private CodePos<Distance, Distance> PosBak = null;

        public void SaveLocationAsOrigin()
        {
            PosBak = Location;
        }

        public bool IsMoved()
        {
            return !PosBak.Equals(Location);
        }

        public void Move(IDrawArea pane, ScreenSize offset)
        {
            var spos0 = GetScreenPos(pane, PosBak);
            var spos1 = spos0 + offset;
            var lpos = LayoutPos.From(pane, spos1);
            var x = CoderX(lpos.X, lpos.Y);
            var y = CoderY(lpos.X, lpos.Y);
            Location = CodePos<Distance, Distance>.From(x, y);
        }

        public ScreenSize SelectableSize { get; private set; }

        public float SelectingScore(IDrawArea pane, ScreenPos pos)
        {
            var rect = ScreenRect.FromCS(GetScreenPos(pane), SelectableSize);
            if (rect.IsIn(pos))
            {
                var len = GeoEu.Length(GetScreenPos(pane), pos);
                var vol = len / GeoEu.Length(SelectableSize.ToDoubles());
                return (float)vol;
            }
            else
            {
                return float.PositiveInfinity;
            }
        }
    }
}
