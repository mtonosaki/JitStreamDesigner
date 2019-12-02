// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Tono;
using Tono.Gui.Uwp;
using Windows.UI;

namespace JitStreamDesigner
{
    /// <summary>
    /// JIT Parts Base class
    /// </summary>
    public abstract class PartsJitBase : PartsBase<Distance, Distance>
    {
        /// <summary>
        /// Selecting Background Color
        /// </summary>
        protected Color SelectingColor { get; } = Color.FromArgb(160, 255, 0, 0);

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
    }
}
