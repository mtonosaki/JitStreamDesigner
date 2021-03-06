﻿// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Tono.Jit;

namespace JitStreamDesigner
{
    /// <summary>
    /// Simulator utility
    /// </summary>
    public abstract class FeatureSimulatorBase : FeatureBase
    {
        /// <summary>
        /// common cold data context
        /// </summary>
        public DataCold Cold => base.DataCold as DataCold;

        /// <summary>
        /// common hot data context
        /// </summary>
        public DataHot Hot => base.DataHot as DataHot;

        /// <summary>
        /// Simulator clock current time
        /// </summary>
        public DateTime Now
        {
            get => Hot.Now;
            set => Hot.Now = value;
        }

        /// <summary>
        /// Set JIT model edit action and send Jac to FeatureUndoRedo
        /// </summary>
        /// <param name="redoJac"></param>
        /// <param name="undoJac"></param>
        public void SetNewAction(EventToken from, string redoJac, string undoJac)
        {
            Token.Link(from, new EventSetUndoRedoTokenTrigger
            {
                TokenID = FeatureUndoRedo.TOKEN.SET,
                JacRedo = redoJac,
                JacUndo = undoJac,
                TemplateChip = Hot.ActiveTemplate,
                Sender = this,
                Remarks = $"{DateTime.Now}",
            });
        }

        /// <summary>
        /// Reset Jac
        /// </summary>
        /// <param name="te"></param>
        protected void ResetJac(TemplateTipModel te)
        {
            if (te.Jac == null)
            {
                te.Jac = new JacInterpreter();
            }
            te.Jac.Reset();

            var jac = $@"
                TheStage = new Stage
                    Name = '{te.Template.Name}'
                    AccentColor = {te.AccentColor.ToString()}
                    Remarks = '{te.Remarks}'
            ";
            te.Jac.Exec(jac);
            te.Jac["Gui"] = Hot.TheBroker;
        }


        /// <summary>
        /// Pane for Jit Model Parts
        /// </summary>
        public IDrawArea PaneJitParts => View;


        public LayoutX DistancePositionerX(CodeX<Distance> x, CodeY<Distance> y)
        {
            return new LayoutX
            {
                Lx = x.Cx.m,
            };
        }

        public LayoutY DistancePositionerY(CodeX<Distance> x, CodeY<Distance> y)
        {
            return new LayoutY
            {
                Ly = y.Cy.m,
            };
        }

        public CodeX<Distance> DistanceCoderX(LayoutX x, LayoutY y)
        {
            return new CodeX<Distance>
            {
                Cx = Distance.FromMeter(x.Lx),
            };
        }
        public CodeY<Distance> DistanceCoderY(LayoutX x, LayoutY y)
        {
            return new CodeY<Distance>
            {
                Cy = Distance.FromMeter(y.Ly)
            };
        }

        public CodePos<Distance, Distance> GetCoderPos(IDrawArea pane, PointerState po)
        {
            return GetCoderPos(pane, po.Position);
        }
        public CodePos<Distance, Distance> GetCoderPos(IDrawArea pane, ScreenPos po)
        {
            var lpos = LayoutPos.From(pane, po);
            return CodePos<Distance, Distance>.From(DistanceCoderX(lpos.X, lpos.Y), DistanceCoderY(lpos.X, lpos.Y));
        }

    }
}
