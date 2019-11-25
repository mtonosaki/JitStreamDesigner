using Tono;
using Tono.Gui.Uwp;

namespace JitStreamDesigner
{
    public class FeatureJitProcess : FeatureSimulatorBase
    {
        private PartsJitProcess CurrentParts = null;

        /// <summary>
        /// Initialize Feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = Pane.Main;
        }

        /// <summary>
        /// Creating new instance
        /// </summary>
        /// <param name="token"></param>
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

        /// <summary>
        /// Moving new instance
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdPositioning, Name = "Process")]
        public void Positioning(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null) return;

            CurrentParts.DesignState = PartsJitBase.DesignStates.Positioning;
            CurrentParts.Location = GetCoderPos(Pane.Target, token.Pointer);
            Redraw();
        }

        /// <summary>
        /// Decide the instance position
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdFinished, Name = "Process")]
        public void Finished(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null) return;

            CurrentParts.DesignState = PartsJitBase.DesignStates.Normal;
            CurrentParts.Location = GetCoderPos(Pane.Main, token.Pointer);
            CurrentParts = null;
            Redraw();
        }

        /// <summary>
        /// Cancel the instance position
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdCancelling, Name = "Process")]
        public void Cancelling(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null) return;

            Parts.Remove(Pane.Target, CurrentParts, LAYER.JitProcess);  // delete temporary parts
            CurrentParts = null;
            Redraw();
        }
    }
}
