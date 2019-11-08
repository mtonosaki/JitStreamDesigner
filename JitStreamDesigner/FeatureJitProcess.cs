using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    public class FeatureJitProcess : FeatureSimulatorBase
    {
        private PartsJitProcess CurrentParts = null;

        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = Pane.Main;

        }

        [EventCatch(TokenID = FeatureToolbox.TokenIdCreating, Name = "Process")]
        public void Creating(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts != null)
            {
                return;
            }

            CurrentParts = new PartsJitProcess
            {
                Location = GetCoderPos(Pane.Target, token.Pointer),
                Width = Distance.FromMeter(2.0),
                Height = Distance.FromMeter(2.0),
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
            Parts.Add(Pane.Target, CurrentParts, LAYER.JitProcess);
        }

        [EventCatch(TokenID = FeatureToolbox.TokenIdPositioning, Name = "Process")]
        public void Positioning(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null) return;

            CurrentParts.Location = GetCoderPos(Pane.Target, token.Pointer);
            Redraw();
        }

        [EventCatch(TokenID = FeatureToolbox.TokenIdFinished, Name = "Process")]
        public void Finished(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null) return;

            CurrentParts.Location = GetCoderPos(Pane.Target, token.Pointer);
            CurrentParts = null;
            Redraw();
        }
    }

    /// <summary>
    /// Process Parts
    /// </summary>
    /// <remarks>
    /// Location = Center
    /// </remarks>
    public class PartsJitProcess : PartsBase<Distance, Distance>
    {
        public Distance Width { get; set; }
        public Distance Height { get; set; }

        public override void Draw(DrawProperty dp)
        {
            var sc = GetScreenPos(dp.Pane);
            var lsiz = LayoutSize.From(PositionerX(CodeX<Distance>.From(Width), null), PositionerY(null, CodeY<Distance>.From(Height)));
            // TODO: Tono.Gui更新待ち 
            //var ssiz = ScreenSize.From(dp.Pane, lsiz);
            var ssiz = new ScreenSize
            {
                Width = ScreenX.From(lsiz.Width.Lx * dp.Pane.ZoomX),
                Height = ScreenY.From(lsiz.Height.Ly * dp.Pane.ZoomY),
            };
            var sr = ScreenRect.FromCWH(sc, ssiz.Width, ssiz.Height);

            dp.Graphics.DrawRectangle(_(sr), Colors.White);
        }
    }
}
