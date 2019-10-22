using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    /// <summary>
    /// Toolbox
    /// </summary>
    public class FeatureToolbox : FeatureSimulatorBase
    {
        /// <summary>
        /// Tool Button Collection
        /// </summary>
        public List<PartsToolButton> Buttons { get; set; } = new List<PartsToolButton>();

        /// <summary>
        /// Initialize feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = Pane["ToolBox"];

            Parts.Add(Pane.Target, new PartsToolBox{}, LAYER.ToolButtonBox);

            var x = ScreenX.From(0);
            var y = ScreenY.From(48);
            var btnSize = ScreenSize.From(24, 24);

            for( var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];
                btn.Size = btnSize;
                btn.Location = CodePos<ScreenX, ScreenY>.From(x, y);
                Parts.Add(Pane.Target, btn, LAYER.ToolButtons);
                y += btnSize.Height;
            }
        }
    }

    public class PartsToolBox : PartsBase<ScreenX, ScreenY>
    {
        public override void Draw(DrawProperty dp)
        {
            dp.Graphics.FillRectangle(_(dp.PaneRect), Colors.Black);
        }
    }

    /// <summary>
    /// Tool Button
    /// </summary>
    public class PartsToolButton : PartsBase<ScreenX, ScreenY>
    {
        public string Name { get; set; }
        public ScreenSize Size { get; set; }
        protected ScreenRect Rect { get; private set; }

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
            dp.Graphics.FillRectangle(_(br), Color.FromArgb(24, 255,255,255));
        }

        public virtual async Task Load(TGuiView owner)
        {
            await Task.Run(() => { });  // dummy task
        }
    }

    public class PartsToolButtonProcess : PartsToolButton
    {
        private CanvasBitmap Bitmap = null;

        /// <summary>
        /// Load bitmap
        /// </summary>
        /// <param name="owner"></param>
        public override async Task Load(TGuiView owner)
        {
            Bitmap = await CanvasBitmap.LoadAsync(owner.Canvas, new Uri("ms-appx:///Assets/tbProcess.png"));
        }

        public override void Draw(DrawProperty dp)
        {
            base.Draw(dp);  // draw background
            if (Bitmap != null)
            {
                dp.Graphics.DrawImage(Bitmap, _(Rect));
            }
        }
    }
}
