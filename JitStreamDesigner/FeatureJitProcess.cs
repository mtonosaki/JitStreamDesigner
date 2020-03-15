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
    /// Feature : GUI Process
    /// </summary>
    /// <remarks>
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdCreating,    Name = "Process")] EventTokenTriggerToolDragging
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdPositioning, Name = "Process")] EventTokenTriggerToolDragging
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdFinished,    Name = "Process")] EventTokenTriggerToolDragging 
    /// [EventCatch(TokenID = FeatureToolbox.TokenIdCancelling,  Name = "Process")] EventTokenTriggerToolDragging 
    ///
    /// [EventCatch(TokenID = TOKEN.CREATE)] EventTokenCreateProcessPartsTrigger
    /// [EventCatch(TokenID = TOKEN.REMOVE)] EventTokenCreateProcessPartsTrigger
    /// </remarks>
    [FeatureDescription(En = "Put JitProcess", Jp = "JitProcess GUI編集")]
    public class FeatureJitProcess : FeatureSimulatorBase
    {
        public static class TOKEN
        {
            public const string CREATE = "FeatureJitProcessCreate";
            public const string REMOVE = "FeatureJitProcessRemove";
        };
        private PartsJitProcess CurrentParts = null;


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
        [EventCatch(TokenID = FeatureToolbox.TokenIdCreating, Name = "Process")]
        public void Creating(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts != null)
            {
                return;
            }

            CurrentParts = new PartsJitProcess
            {
                Location = GetCoderPos(PaneJitParts, token.Pointer),
                Width = Distance.FromMeter(2.0),
                Height = Distance.FromMeter(2.0),
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
            Parts.Add(PaneJitParts, CurrentParts, LAYER.JitProcess);
        }

        /// <summary>
        /// Moving new instance
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdPositioning, Name = "Process")]
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
        [EventCatch(TokenID = FeatureToolbox.TokenIdCancelling, Name = "Process")]
        public void Cancelling(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null)
            {
                return;
            }

            Parts.Remove(PaneJitParts, CurrentParts, LAYER.JitProcess);  // delete temporary parts
            CurrentParts = null;
            Redraw();
        }

        /// <summary>
        /// Decide the instance position
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdFinished, Name = "Process")]
        public void Finished(EventTokenTriggerToolDragging token)
        {
            if (CurrentParts == null)
            {
                return;
            }

            CurrentParts.DesignState = PartsJitBase.DesignStates.Normal;
            CurrentParts.Location = GetCoderPos(PaneJitParts, token.Pointer);
            var processID = JacInterpreter.MakeID("Process");
            CurrentParts.ID = processID;

            var jacredo =
            $@"
                TheStage
                    Procs
                        add new Process
                            ID = '{processID}'
                            LocationX = {CurrentParts.Location.X.Cx.m}m
                            LocationY = {CurrentParts.Location.Y.Cy.m}m
                            Width = {CurrentParts.Width.m}m
                            Height = {CurrentParts.Height.m}m
                Gui.ClearAllSelection = true
                Gui.CreateProcess = {processID}
            ";
            var jacundo =
            $@"
                Gui.RemoveProcess = {processID}
                TheStage
                    Procs
                        remove {processID}
            ";
            SetNewAction(token, jacredo, jacundo);

            // remove toolbox parts. (Expecting to be created by REDO processor)
            Parts.Remove(PaneJitParts, CurrentParts, LAYER.JitProcess);
            CurrentParts = null;
        }

        [EventCatch(TokenID = TOKEN.CREATE)]
        public void CreateProcess(EventTokenProcessPartsTrigger token)
        {
            var pt = new PartsJitProcess
            {
                ID = token.Process.ID,
                Location = CodePos<Distance, Distance>.From((Distance)token.Process.ChildVriables["LocationX"].Value, (Distance)token.Process.ChildVriables["LocationY"].Value),
                Width = (Distance)token.Process.ChildVriables["Width"].Value,
                Height = (Distance)token.Process.ChildVriables["Height"].Value,
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
            Parts.Add(PaneJitParts, pt, LAYER.JitProcess);
            Redraw();
        }

        [EventCatch(TokenID = TOKEN.REMOVE)]
        public void RemoveProcess(EventTokenProcessPartsTrigger token)
        {
            foreach (var pt in Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.ID == token.Process.ID))
            {
                Parts.Remove(PaneJitParts, pt, LAYER.JitProcess);
            }
            Redraw();
        }

        [EventCatch(TokenID = TokensGeneral.PartsMoved)]
        public void PartsMoved(EventTokenPartsMovedTrigger token)
        {
            var jacUndo = new StringBuilder();
            var jacRedo = new StringBuilder();
            jacUndo.AppendLine("Gui.ClearAllSelection = true");
            jacRedo.AppendLine("Gui.ClearAllSelection = true");
            var n = 0;
            foreach (PartsJitProcess pt in token.PartsSet.Where(a => a is PartsJitProcess))
            {
                n++;
                jacRedo.AppendLine($@"{pt.ID}.LocationX = {pt.Location.X.Cx.m}m");
                jacRedo.AppendLine($@"{pt.ID}.LocationY = {pt.Location.Y.Cy.m}m");
                jacRedo.AppendLine($@"Gui.UpdateLocation = {pt.ID}");

                jacUndo.AppendLine($@"{pt.ID}.LocationX = {pt.OriginalPosition.X.Cx.m}m");
                jacUndo.AppendLine($@"{pt.ID}.LocationY = {pt.OriginalPosition.Y.Cy.m}m");
                jacUndo.AppendLine($@"Gui.UpdateLocation = {pt.ID}");
            }
            if( n > 0)
            {
                SetNewAction(token, jacRedo.ToString(), jacUndo.ToString());
            }
        }

        [EventCatch(TokenID = TokensGeneral.PartsSelectChanged)]
        public void PartsSelected(EventTokenPartsSelectChangedTrigger token)
        {
            var tars = token.PartStates.Where(a => a.sw).Where(a => a.parts is PartsJitProcess).Select(a => (PartsJitProcess)a.parts);
            foreach (var pt in tars)
            {
                Token.Link(token, new EventTokenTriggerPropertyOpen
                {
                    Target = Hot.ActiveTemplate.Jac[pt.ID],
                    Sender = this,
                    Remarks = "At parts selected",
                });
            }
        }
    }

    /// <summary>
    /// Process parts control order message
    /// </summary>
    public class EventTokenProcessPartsTrigger : EventTokenTrigger
    {
        public JitProcess Process { get; set; }
    }
}
