// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System.Linq;
using System.Text;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Tono.Jit;

namespace JitStreamDesigner
{
    /// <summary>
    /// Feature : GUI Work
    /// </summary>
    /// <remarks>
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdCreating,    Name = "Work")] EventTokenTriggerToolDragging
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdPositioning, Name = "Work")] EventTokenTriggerToolDragging
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdFinished,    Name = "Work")] EventTokenTriggerToolDragging 
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdCancelling,  Name = "Work")] EventTokenTriggerToolDragging 
    ///
    /// [EventCatch(TokenID = TOKEN.CREATE)] EventTokenCreateWorkPartsTrigger
    /// [EventCatch(TokenID = TOKEN.REMOVE)] EventTokenCreateWorkPartsTrigger
    /// </remarks>
    [FeatureDescription(En = "Put JitWork", Jp = "JitWork GUI編集")]
    public class FeatureJitWork : FeatureSimulatorBase
    {
        public static class TOKEN
        {
            public const string CREATE = "FeatureJitWorkCreate";
            public const string REMOVE = "FeatureJitWorkRemove";
        };
        private PartsJitWork CurrentParts = null;


        /// <summary>
        /// Initialize Feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = PaneJitParts;
        }

        /// <summary>
        /// Creating new instance
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdCreating, Name = "Work")]
        public void Creating(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts != null)
            {
                return;
            }

            CurrentParts = new PartsJitWork
            {
                Location = GetCoderPos(PaneJitParts, token.Pointer),
                Width = Distance.FromMeter(2.0),
                Height = Distance.FromMeter(2.0),
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
            Parts.Add(PaneJitParts, CurrentParts, LAYER.JitWork);
        }

        /// <summary>
        /// Moving new instance
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdPositioning, Name = "Work")]
        public void Positioning(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null)
            {
                return;
            }

            CurrentParts.DesignState = PartsJitBase.DesignStates.Positioning;
            CurrentParts.Location = GetCoderPos(PaneJitParts, token.Pointer);
            Redraw();
        }

        /// <summary>
        /// Cancel the instance position
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdCancelling, Name = "Work")]
        public void Cancelling(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null)
            {
                return;
            }

            Parts.Remove(PaneJitParts, CurrentParts, LAYER.JitWork);  // delete temporary parts
            CurrentParts = null;
            Redraw();
        }

        /// <summary>
        /// Decide the instance position
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdFinished, Name = "Work")]
        public void Finished(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null)
            {
                return;
            }

            CurrentParts.DesignState = PartsJitBase.DesignStates.Normal;
            CurrentParts.Location = GetCoderPos(PaneJitParts, token.Pointer);
            var workID = JacInterpreter.MakeID("Work");
            CurrentParts.ID = workID;

            var jacredo =
            $@"
                TheStage
                    Works
                        add datetime('{Now.ToString(TimeUtil.FormatYMDHMSms)}'):new Work
                            ID = '{workID}'
                            LocationX = {CurrentParts.Location.X.Cx.m}m
                            LocationY = {CurrentParts.Location.Y.Cy.m}m
                            Width = {CurrentParts.Width.m}m
                            Height = {CurrentParts.Height.m}m
                Gui.ClearAllSelection = true
                Gui.CreateWork = {workID}
            ";
            var jacundo =
            $@"
                Gui.RemoveWork = {workID}
                TheStage
                    Works
                        remove {workID}
            ";
            SetNewAction(token, jacredo, jacundo);

            // remove toolbox parts. (Expecting to be created by REDO processor)
            Parts.Remove(PaneJitParts, CurrentParts, LAYER.JitWork);
            CurrentParts = null;
        }

        [EventCatch(TokenID = TOKEN.CREATE)]
        public void CreateWork(EventTokenWorkPartsTrigger token)
        {
            var pt = new PartsJitWork
            {
                ID = token.Work.ID,
                Location = CodePos<Distance, Distance>.From((Distance)token.Work.ChildVriables["LocationX"].Value, (Distance)token.Work.ChildVriables["LocationY"].Value),
                Width = (Distance)token.Work.ChildVriables["Width"].Value,
                Height = (Distance)token.Work.ChildVriables["Height"].Value,
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
            Parts.Add(PaneJitParts, pt, LAYER.JitWork);
            Redraw();
        }

        [EventCatch(TokenID = TOKEN.REMOVE)]
        public void RemoveWork(EventTokenWorkPartsTrigger token)
        {
            foreach (var pt in Parts.GetParts<PartsJitWork>(LAYER.JitWork, a => a.ID == token.Work.ID))
            {
                Parts.Remove(PaneJitParts, pt, LAYER.JitWork);
            }
            Redraw();
        }
    }

    /// <summary>
    /// Work parts control order message
    /// </summary>
    public class EventTokenWorkPartsTrigger : EventTokenTrigger
    {
        public JitWork Work { get; set; }
    }
}
