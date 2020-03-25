// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;

namespace JitStreamDesigner
{
    /// <summary>
    /// JIT Parts Base class
    /// </summary>
    public abstract class PartsJitBase : PartsBase<Distance, Distance>, ISelectableParts, IMovableParts
    {
        private bool isSelected = false;

        /// <summary>
        /// Process/Work ID same with Jac
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Parts Select State
        /// </summary>
        public virtual bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
            }
        }

        /// <summary>
        /// to set as ignore move with Redo/Undo mechanism
        /// </summary>
        public bool IsCancellingMove { get; set; }

        /// <summary>
        /// Selecting Background Color
        /// </summary>
        protected Color SelectingColor { get; } = Color.FromArgb(96, 255, 0, 0);

        /// <summary>
        /// Connecting Background Color
        /// </summary>
        protected Color ConnectingColor { get; } = Color.FromArgb(96, 0, 255, 255);


        public enum DesignStates
        {
            Positioning,
            Normal,
        };

        /// <summary>
        /// Design State
        /// </summary>
        public DesignStates DesignState { get; set; } = DesignStates.Normal;

        /// <summary>
        /// Parts Size (Horizontal)
        /// </summary>
        public Distance Width { get; set; }

        /// <summary>
        /// Parts Size (Vertical)
        /// </summary>
        public Distance Height { get; set; }

        /// <summary>
        /// Parts Color
        /// </summary>
        public virtual Color BaseColor => Colors.White;

        /// <summary>
        /// Produce main parts color considering parts creating, selecting, etc...
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public virtual Color GetColor(DrawProperty dp)
        {
            if (DesignState == DesignStates.Positioning)
            {
                return ColorUtil.ChangeAlpha(BaseColor, 0.3f);
            }
            else
            {
                return BaseColor;
            }
        }

        /// <summary>
        /// Parts move origin location
        /// </summary>
        public CodePos<Distance, Distance> OriginalPosition { get; private set; } = null;

        public void SaveLocationAsOrigin()
        {
            OriginalPosition = Location;
        }

        public void ClearOriginalPosition()
        {
            OriginalPosition = null;
        }

        public bool IsMoved()
        {
            if (OriginalPosition == null) return false;

            return !OriginalPosition.Equals(Location);
        }

        public void Move(IDrawArea pane, ScreenSize offset)
        {
            if (OriginalPosition == null) return;
            var spos0 = GetScreenPos(pane, OriginalPosition);
            var spos1 = spos0 + offset;
            var lpos = LayoutPos.From(pane, spos1);
            var x = CoderX(lpos.X, lpos.Y);
            var y = CoderY(lpos.X, lpos.Y);
            Location = CodePos<Distance, Distance>.From(x, y);
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
            var rect = ScreenRect.FromCS(sp, SelectableSize);
            if (rect.IsIn(pos))
            {
                var len = GeoEu.Length(sp, pos);
                var vol = len / GeoEu.Length(SelectableSize.ToDoubles());
                return (float)vol;
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
    }
}
