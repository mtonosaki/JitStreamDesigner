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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    public class FeatureProcessLinkConnection : FeatureSimulatorBase, IPointerListener
    {
        public TMenuFlyoutItem CommandDeleteProcessLink { get; set; }
        public MenuFlyout ContextMenu { get; set; }

        /// <summary>
        /// Initialize this feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = PaneJitParts;

            CommandDeleteProcessLink.Visibility = Visibility.Collapsed;
            CommandDeleteProcessLink.Click += CommandDeleteProcessLink_Click;
            ContextMenu.Opening += Menu_Opening;
            ContextMenu.Closed += Menu_Closed;
        }

        [EventCatch(TokenID = TokensGeneral.PartsSelectChanged)]
        public void PartsSelected(EventTokenPartsSelectChangedTrigger token)
        {
            ResetConnectorGripParts(); // Delete the all connector Parts

            // Make connector parts with process click
            var froms = Parts.GetParts<PartsJitProcessLink>(LAYER.JitProcessConnector).ToDictionary(a => a.ProcessFrom.ID);
            var tars = token.PartStates
                .Where(a => a.sw)
                .Where(a => a.parts is PartsJitProcess)
                .Select(a => (PartsJitProcess)a.parts)
                .Where(a => froms.ContainsKey(a.ID) == false);

            foreach (var tarProcParts in tars)
            {
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

        [EventCatch(TokenID = TokensGeneral.PartsMoving)]
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
            foreach (var pcon in token.PartsSet.Where(a => a is PartsConnectGrip).Select(a => (PartsConnectGrip)a))
            {
                var spos = pcon.GetScreenPos(Pane.Target);
                var prs =
                    from p in Parts.GetParts<PartsJitProcess>(LAYER.JitProcess)
                    where pcon.TargetProcess.Equals(p) == false
                    let score = p.SelectingScore(Pane.Target, spos)
                    orderby score ascending
                    select (p, score);
                foreach (var ps in LoopUtil<(PartsJitProcess Process, float Score)>.From(prs, out var loop))
                {
                    ps.Process.IsConnecting = false;
                    isDrawRequested = true;

                    loop.DoFirstTime(() =>
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
                .Select(a => (PartsConnectGrip)a), out var loop))
            {
                pcon.SetLocation(pcon.Angle = pcon.GetAngle()); // Update Angle and Location
                loop.DoFirstTime(() =>
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
            if (confrom != null && conto != null)
            {
                Connect(token, confrom, conto);
            }
        }
        private void ResetConnectorGripParts()
        {
            Parts.RemoveLayereParts(Pane.Target, LAYER.JitProcessConnectorGrip);    // delete the all connector grip parts

            foreach (var pt in Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.IsConnecting))  // reset PartsJitProcess property
            {
                pt.IsConnecting = false;
            }
            Redraw();
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="token"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private void Connect(EventTokenTrigger token, PartsJitProcess from, PartsJitProcess to)
        {
            ResetConnectorGripParts();
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
                ProcessTo = Parts.GetParts<PartsJitProcess>(LAYER.JitProcess, a => a.ID == token.ProcessIDTo).FirstOrDefault(),
                Width = Distance.FromMeter(0.5),
                Height = Distance.FromMeter(0.5),
                PositionerX = DistancePositionerX,
                PositionerY = DistancePositionerY,
                CoderX = DistanceCoderX,
                CoderY = DistanceCoderY,
            };
            Parts.Add(Pane.Target, link, LAYER.JitProcessConnector);
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.RemoveProcessLink)]
        public void RemoveProcessLink(EventTokenProcessLinkTrigger token)
        {
            var links = Parts.GetParts<PartsJitProcessLink>(LAYER.JitProcessConnector, a =>
            {
                return a.ProcessFrom.ID == token.ProcessIDFrom && a.ProcessTo.ID == token.ProcessIDTo;
            });
            foreach (var link in links)
            {
                Parts.Remove(Pane.Target, link, LAYER.JitProcessConnector);
            }
        }

        public void OnPointerPressed(PointerState po)
        {
            _pobak = po.Clone();
        }

        public void OnPointerHold(PointerState po)
        {
        }

        public void OnPointerMoved(PointerState po)
        {
            _pobak = po.Clone();
            if (isMenu) return;

            foreach (var link in LoopUtil<PartsJitProcessLink>
                .From(
                    Parts.GetParts<PartsJitProcessLink>(LAYER.JitProcessConnector)
                    .OrderBy(a => (int)(a.SelectingScore(Pane.Target, po.Position) * 1000))
                , out var loop))
            {
                loop.DoFirstTime(() =>
                {
                    if (link.SelectingScore(Pane.Target, po.Position) <= 1.0f)
                    {
                        link.State = PartsJitProcessLink.States.HOVER;
                    }
                    else
                    {
                        link.State = PartsJitProcessLink.States.LINE;
                    }
                });
                loop.DoSecondTimesAndSubsequent(() =>
                {
                    link.State = PartsJitProcessLink.States.LINE;
                });
            }
        }

        public void OnPointerReleased(PointerState po)
        {
        }

        private bool isMenu = false;
        private PointerState _pobak;

        private void Menu_Opening(object sender, object e)
        {
            Menu_Closed(null, e);
            isMenu = true;
            CommandDeleteProcessLink.Visibility = Visibility.Collapsed;

            var link = Parts.GetParts<PartsJitProcessLink>(LAYER.JitProcessConnector).OrderBy(a => (int)(a.SelectingScore(Pane.Target, _pobak.Position) * 1000)).FirstOrDefault();
            if (link == default) return;
            if (link.SelectingScore(Pane.Target, _pobak.Position) > 1.0f) return;

            CommandDeleteProcessLink.Visibility = Visibility.Visible;
            link.State = PartsJitProcessLink.States.SELECTING;
            CommandDeleteProcessLink.Tag = link;
            Redraw();
        }

        private void Menu_Closed(object sender, object e)
        {
            foreach (var link in LoopUtil<PartsJitProcessLink>.From(Parts.GetParts<PartsJitProcessLink>(LAYER.JitProcessConnector), out var loop))
            {
                link.State = PartsJitProcessLink.States.LINE;
                loop.DoLastOneTime(() =>
                {
                    Redraw();
                });
            }
            isMenu = false;
        }

        /// <summary>
        /// Link Delete Command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandDeleteProcessLink_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var link = ((FrameworkElement)sender).Tag as PartsJitProcessLink;
            if (link == null) return;

            var jacredo =
            $@"
                TheStage
                    ProcLinks
                        remove '{link.ProcessFrom.ID}' -> '{link.ProcessTo.ID}'
                Gui.RemoveProcLink = '{link.ProcessFrom.ID},{link.ProcessTo.ID}'
            ";
            var jacundo =
            $@"
                TheStage
                    ProcLinks
                        add '{link.ProcessFrom.ID}' -> '{link.ProcessTo.ID}'
                Gui.AddProcLink = '{link.ProcessFrom.ID},{link.ProcessTo.ID}'
            ";
            SetNewAction(null, jacredo, jacundo);
        }
    }
}
