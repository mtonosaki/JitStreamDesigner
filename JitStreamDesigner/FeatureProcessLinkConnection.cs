using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    public class FeatureProcessLinkConnection : FeatureSimulatorBase
    {
        /// <summary>
        /// Initialize this feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = PaneJitParts;
        }

        [EventCatch(TokenID = TokensGeneral.PartsSelectChanged)]
        public void PartsSelected(EventTokenPartsSelectChangedTrigger token)
        {
            // Make connector parts with process click
            var tars = token.PartStates
                .Where(a => a.sw)
                .Where(a => a.parts is PartsJitProcess)
                .Select(a => (PartsJitProcess)a.parts);

            foreach (var tarProcParts in LoopUtil<PartsJitProcess>.From(tars, out var lc))
            {
                lc.DoFirstTime(() =>
                {
                    ResetConnectParts(); // Delete the all connector Parts
                });
                var cf = new PartsConnectGrip
                {
                    Design = PartsConnectGrip.Designs.OUT,
                    TargetProcess = tarProcParts,
                    Width = Distance.FromMeter(0.5),
                    Height = Distance.FromMeter(0.5),
                    PositionerX = DistancePositionerX,
                    PositionerY = DistancePositionerY,
                    CoderX = DistanceCoderX,
                    CoderY = DistanceCoderY,
                };
                cf.SetLocation(Angle.Zero);
                Parts.Add(Pane.Target, cf, LAYER.JitProcessConnectorGrip);
            }
        }

        [EventCatch(TokenID =TokensGeneral.PartsMoving)]
        public void PartsMoving(EventTokenPartsMovingTrigger token)
        {
            bool isDrawRequested = false;

            // Update Connector Location by "Process Location Updated"
            foreach (var pr in token.PartsSet.Where(a => a is PartsJitProcess).Select(a => (PartsJitProcess)a))
            {
                foreach (var pcon in Parts
                    .GetParts<PartsConnectGrip>(LAYER.JitProcessConnectorGrip)
                    .Where(a => a.TargetProcess.Equals(pr)))
                {
                    pcon.SetLocation(pcon.Angle);
                    isDrawRequested = true;
                }
            }

            // Change IsConnecting
            foreach(var pcon in token.PartsSet.Where(a => a is PartsConnectGrip).Select(a => (PartsConnectGrip)a))
            {
                var spos = pcon.GetScreenPos(Pane.Target);
                var prs =
                    from p in Parts.GetParts<PartsJitProcess>(LAYER.JitProcess)
                    where pcon.TargetProcess.Equals(p) == false
                    let score = p.SelectingScore(Pane.Target, spos)
                    orderby score ascending
                    select (p, score);
                foreach (var ps in LoopUtil<(PartsJitProcess Process, float Score)>.From(prs, out var lu))
                {
                    ps.Process.IsConnecting = false;
                    isDrawRequested = true;

                    lu.DoFirstTime(() =>
                    {
                        if (ps.Score <= 1.0f)
                        {
                            ps.Process.IsConnecting = true;
                        }
                    });
                }
            }
            if (isDrawRequested)
            {
                Redraw();
            }
        }

        [EventCatch(TokenID = TokensGeneral.PartsMoved)]
        public void PartsMoved(EventTokenPartsMovedTrigger token)
        {
            PartsJitProcess confrom = null;

            // Fix pcon's location by Angle when updated PartsJitProcessConnector location
            foreach (var pcon in LoopUtil<PartsConnectGrip>.From(
                token.PartsSet
                .Where(a => a is PartsConnectGrip)
                .Select(a => (PartsConnectGrip)a), out var lu))
            {
                pcon.SetLocation(pcon.Angle = pcon.GetAngle());
                lu.DoFirstTime(() =>
                {
                    confrom = pcon.TargetProcess;
                });
            }

            // Fix pcon's location by Angle when updated PartsJitProcess position
            if (token.PartsSet.Where(a => a is PartsJitProcess).FirstOrDefault() != default)
            {
                foreach (var pcon in Parts.GetParts<PartsConnectGrip>(LAYER.JitProcessConnectorGrip))
                {
                    pcon.SetLocation(pcon.Angle);
                }
            }
            var conto = Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.IsConnecting).FirstOrDefault();
            if( confrom != null && conto != null)
            {
                Connect(token, confrom, conto);
            }
        }
        private void ResetConnectParts()
        {
            Parts.RemoveLayereParts(Pane.Target, LAYER.JitProcessConnectorGrip);
            foreach (var pt in Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.IsConnecting))
            {
                pt.IsConnecting = false;
            }
            Redraw();
        }
        private void Connect(EventTokenTrigger token, PartsJitProcess from, PartsJitProcess to)
        {
            ResetConnectParts();
            var jacredo =
            $@"
                TheStage
                    ProcLinks
                        add '{from.ID}' -> '{to.ID}'
                Gui.AddProcLink = '{from.ID},{to.ID}'
            ";
            var jacundo =
            $@"
                TheStage
                    ProcLinks
                        remove '{from.ID}' -> '{to.ID}'
                Gui.RemoveProcLink = '{from.ID},{to.ID}'
            ";
            SetNewAction(token, jacredo, jacundo);
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.AddProcessLink)]
        public void AddProcessLink(EventTokenProcessLinkTrigger token)
        {
            var link = new PartsJitProcessLink
            {
                ProcessFrom = Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.ID == token.ProcessIDFrom).FirstOrDefault(),
                ProcessTo   = Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.ID == token.ProcessIDTo).FirstOrDefault(),
                Width = Distance.FromMeter(0.5),
                Height = Distance.FromMeter(0.5),
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
//            Parts.Add(Pane.Target, link, LAYER.JitProcessConnector)
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.RemoveProcessLink)]
        public void RemoveProcessLink(EventTokenProcessLinkTrigger token)
        {
            int a = 1;
        }
    }
}
