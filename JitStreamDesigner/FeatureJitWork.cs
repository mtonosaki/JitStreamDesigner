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
        private PartsJitWork CreatingParts = null;


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
            if (CreatingParts != null)
            {
                return;
            }

            CreatingParts = new PartsJitWork
            {
                Location = GetCoderPos(PaneJitParts, token.Pointer),
                Width = Distance.FromMeter(0.3),
                Height = Distance.FromMeter(0.3),
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
            Parts.Add(PaneJitParts, CreatingParts, LAYER.JitWork);
        }

        /// <summary>
        /// Moving new instance
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdPositioning, Name = "Work")]
        public void Positioning(EventTokenTriggerToolDragging token)
        {
            if (CreatingParts == null)
            {
                return;
            }

            CreatingParts.DesignState = PartsJitBase.DesignStates.Positioning;
            CreatingParts.Location = GetCoderPos(PaneJitParts, token.Pointer);
            Redraw();
        }

        /// <summary>
        /// Cancel the instance position
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdCancelling, Name = "Work")]
        public void Cancelling(EventTokenTriggerToolDragging token)
        {
            if (CreatingParts == null)
            {
                return;
            }

            Parts.Remove(PaneJitParts, CreatingParts, LAYER.JitWork);  // delete temporary parts
            CreatingParts = null;
            Redraw();
        }

        /// <summary>
        /// Decide the instance position
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = FeatureToolbox.TokenIdFinished, Name = "Work")]
        public void Finished(EventTokenTriggerToolDragging token)
        {
            if (CreatingParts == null)
            {
                return;
            }

            CreatingParts.DesignState = PartsJitBase.DesignStates.Normal;
            CreatingParts.Location = GetCoderPos(PaneJitParts, token.Pointer);
            var workID = JacInterpreter.MakeID("Work");
            CreatingParts.ID = workID;

            var jacredo =
            $@"
                TheStage
                    Works
                        add datetime('{Now.ToString(TimeUtil.FormatYMDHMSms)}'):new Work
                            ID = '{workID}'
                            LocationX = {CreatingParts.Location.X.Cx.m}m
                            LocationY = {CreatingParts.Location.Y.Cy.m}m
                            Width = {CreatingParts.Width.m}m
                            Height = {CreatingParts.Height.m}m
                            Current = new Location
                                Stage = TheStage
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
            Parts.Remove(PaneJitParts, CreatingParts, LAYER.JitWork);
            CreatingParts = null;
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

        private PartsJitWork movingWorkParts = null;

        [EventCatch(TokenID = TokensGeneral.PartsMoving)]
        public void OnPartsMoving(EventTokenPartsMovingTrigger token)
        {
            var col0 =
                from p in token.PartsSet
                let a = p as PartsJitWork
                where a != null
                where a.OriginalPosition != null
                where a.IsSelectingLocation
                where a.IsMoved()
                select a;
            var col = col0.ToArray();

            if (token.PartsSet.Count() > 1)
            {
                foreach (var a in col)
                {
                    a.IsSelectingLocation = false;
                }
                return;
            }

            if (col.FirstOrDefault() is PartsJitWork p2)
            {
                movingWorkParts = p2;
            }

            if (movingWorkParts != null)
            {
                var scw = movingWorkParts.GetScreenPos(Pane.Target);
                foreach (var pt in Parts.GetParts<PartsJitProcess>(LAYER.JitProcess))
                {
                    var scp = pt.GetScreenPos(Pane.Target);
                    var srp = ScreenRect.FromCS(scp, pt.SelectableSize);
                    pt.IsConnecting = movingWorkParts.IsIn(Pane.Target, srp);
                }
            }
        }

        [EventCatch(TokenID = TokensGeneral.PartsMoved)]
        public void OnPartsMoved(EventTokenPartsMovedTrigger token)
        {
            if (movingWorkParts?.OriginalPosition != null)
            {
                try
                {
                    var procs = Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.IsConnecting).ToArray();
                    if (procs.Length == 0)
                    {
                        // Just Move work
                        return;
                    }
                    else if (procs.Length == 1)  // Set Work.Next
                    {
                        var pw = Hot.ActiveTemplate.Jac.GetWork(movingWorkParts.ID);
                        var pp = Hot.ActiveTemplate.Jac.GetProcess(procs[0].ID);
                        if (pw.Next?.Process?.ID != procs[0].ID)
                        {
                            var jacredo =
                            $@"
                                {movingWorkParts.ID}
                                    Next = new Location
                                        Stage = TheStage
                                        Path = '\'
                                        Process = {procs[0].ID}
                                Gui.UpdateCassetteValue = {movingWorkParts.ID}
                                Gui.SelectParts = {movingWorkParts.ID}
                            ";
                            string jacundo = "";
                            if (pw.Next == null)
                            {
                                jacundo = $@"
                                    {movingWorkParts.ID}
                                        Next = null
                                ";
                            }
                            else
                            {
                                jacundo = $@"
                                    {movingWorkParts.ID}
                                        Next = new Location
                                            Stage = TheStage
                                            Path = '\'
                                            Process = {(pw.Next.Process?.ID ?? "null")}
                                ";
                            }
                            {
                                jacundo += $@"
                                    Gui.UpdateCassetteValue = {movingWorkParts.ID}
                                    Gui.SelectParts = {movingWorkParts.ID}
                                ";
                            }
                            SetNewAction(token, jacredo, jacundo);
                            LOG.WriteLine(LLV.WAR, $"The Work.Next = {pp.Name}"); // TODO: Support Multi language with LOG.AddMes (VS2019 is now not work Resources.resw editor)
                        }
                        else
                        {
                            LOG.WriteLine(LLV.WAR, "The process is already set to this work."); // TODO: Support Multi language with LOG.AddMes (VS2019 is now not work Resources.resw editor)
                        }
                    }
                    else
                    {
                        LOG.WriteLine(LLV.WAR, "Need to select ONE process to set work destination."); // TODO: Support Multi language with LOG.AddMes (VS2019 is now not work Resources.resw editor)
                    }
                    movingWorkParts.Location = movingWorkParts.OriginalPosition;
                    movingWorkParts.IsSelected = false;
                    movingWorkParts.IsCancellingMove = true;
                    movingWorkParts.IsSelectingLocation = false;
                }
                finally
                {
                    foreach (var pp in Parts.GetParts<PartsJitProcess>(LAYER.JitProcess))
                    {
                        pp.IsConnecting = false;
                    }
                    movingWorkParts.IsSelectingLocation = false;
                    movingWorkParts.IsSelected = false;
                    movingWorkParts = null;
                    Redraw();
                }
            }
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
