// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas;
using System;
using System.Threading.Tasks;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    /// <summary>
    /// Tool Button
    /// </summary>
    public abstract class PartsToolButton : PartsBase<ScreenX, ScreenY>, ITooltipResponse, ISelectableParts
    {
        public string Name { get; set; }
        public string ToolTipUid { get; set; }
        public ScreenSize Size { get; set; }
        public bool IsSelected { get; set; }
        public ScreenRect Rect { get; private set; }

        /// <summary>
        /// Draw background
        /// </summary>
        /// <param name="dp"></param>
        public override void Draw(DrawProperty dp)
        {
            var spos = ScreenPos.From(Location);
            Rect = ScreenRect.From(spos, Size);
            var br = Rect.Clone();
            br.RB = ScreenPos.From(br.R, br.B - 1);

            if (IsSelected)
            {
                dp.Graphics.FillRectangle(_(br), Color.FromArgb(96, 255, 255, 255));
            }
            else
            {
                dp.Graphics.FillRectangle(_(br), Color.FromArgb(16, 255, 255, 255));
            }
        }

        public virtual async Task Load(TGuiView owner)
        {
            await Task.Run(() => { });  // dummy task
        }

        public float SelectingScore(IDrawArea pane, ScreenPos pos)
        {
            return Rect.IsIn(pos) ? 0f : float.PositiveInfinity;
        }
    }

    /// <summary>
    /// Tool Button Base Class
    /// </summary>
    public abstract class PartsToolButtonImageBase : PartsToolButton
    {
        protected CanvasBitmap Bitmap = null;

        public override void Draw(DrawProperty dp)
        {
            base.Draw(dp);  // draw background
            if (Bitmap != null)
            {
                dp.Graphics.DrawImage(Bitmap, _(Rect));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class PartsToolButtonProcess : PartsToolButtonImageBase
    {
        public override async Task Load(TGuiView owner)
        {
            Bitmap = await CanvasBitmap.LoadAsync(owner.Canvas, new Uri("ms-appx:///Assets/tbProcess.png"));
        }
    }
    public class PartsToolButtonProcessPriorityJoin : PartsToolButtonImageBase
    {
        public override async Task Load(TGuiView owner)
        {
            Bitmap = await CanvasBitmap.LoadAsync(owner.Canvas, new Uri("ms-appx:///Assets/tbProcessPriorityJoin.png"));
        }
    }
    public class PartsToolButtonWork : PartsToolButtonImageBase
    {
        public override async Task Load(TGuiView owner)
        {
            Bitmap = await CanvasBitmap.LoadAsync(owner.Canvas, new Uri("ms-appx:///Assets/tbWork.png"));
        }
    }

    public class PartsToolButtonKanban : PartsToolButtonImageBase
    {
        public override async Task Load(TGuiView owner)
        {
            Bitmap = await CanvasBitmap.LoadAsync(owner.Canvas, new Uri("ms-appx:///Assets/tbKanban.png"));
        }
    }
    public class PartsToolBox : PartsBase<ScreenX, ScreenY>
    {
        public override void Draw(DrawProperty dp)
        {
            dp.Graphics.FillRectangle(_(dp.PaneRect), Colors.Black);
        }
    }
}
