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
            var tars = token.PartStates
                .Where(a => a.sw)
                .Where(a => a.parts is PartsJitProcess)
                .Select(a => (PartsJitProcess)a.parts);

            foreach (var tarProcParts in LoopUtil<PartsJitProcess>.From(tars, out var lc))
            {
                lc.DoFirstOneTime(() =>
                {
                    Parts.RemoveLayereParts(Pane.Target, LAYER.JitProcessConnector);    // Delete the all connector Parts
                });
                var cf = new PartsJitProcessConnector
                {
                    Kind = PartsJitProcessConnector.Kinds.OUT,
                    TargetProcess = tarProcParts,
                    Width = Distance.FromMeter(0.5),
                    Height = Distance.FromMeter(0.5),
                    PositionerX = DistancePositionerX,
                    PositionerY = DistancePositionerY,
                    CoderX = DistanceCoderX,
                    CoderY = DistanceCoderY,
                };
                cf.SetLocation(Angle.Zero);
                Parts.Add(Pane.Target, cf, LAYER.JitProcessConnector);
            }
        }

        [EventCatch(TokenID = TokensGeneral.PartsMoved)]
        public void PartsMoved(EventTokenPartsMovedTrigger token)
        {
            foreach (var pf in token.Parts
                .Where(a => a is PartsJitProcessConnector)
                .Select(a => (PartsJitProcessConnector)a))
            {
                pf.SetLocation(pf.GetAngle());
            }
        }
    }
}
