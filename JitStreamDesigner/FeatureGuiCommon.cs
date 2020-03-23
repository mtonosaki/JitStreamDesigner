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
    /// Gui Common Interface : implement to a PartsJit*****
    /// </summary>
    public interface IGuiPartsControlCommon
    {
        string ID { get; }
        CodePos<Distance, Distance> Location { get; }
        CodePos<Distance, Distance> OriginalPosition { get; }
    }

    /// <summary>
    /// Feature : GUI Common
    /// </summary>
    [FeatureDescription(En = "Jit Parts Select/Move", Jp = "Jitパーツ GUI編集共通処理")]
    public class FeatureGuiCommon : FeatureSimulatorBase
    {
        /// <summary>
        /// Initialize Feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = PaneJitParts;
        }

        [EventCatch(TokenID = TokensGeneral.PartsMoved)]
        public void PartsMoved(EventTokenPartsMovedTrigger token)
        {
            var jacUndo = new StringBuilder();
            var jacRedo = new StringBuilder();
            jacUndo.AppendLine("Gui.ClearAllSelection = true");
            jacRedo.AppendLine("Gui.ClearAllSelection = true");
            var n = 0;
            foreach (IGuiPartsControlCommon pt in token.PartsSet.Where(a => a is IGuiPartsControlCommon))
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
            var tars = token.PartStates.Where(a => a.sw).Where(a => a.parts is IGuiPartsControlCommon).Select(a => (IGuiPartsControlCommon)a.parts);
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
}
