// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Tono.Gui;
using Tono.Gui.Uwp;

namespace JitStreamDesigner
{
    /// <summary>
    /// Toolbox
    /// </summary>
    public class FeatureToolbox : FeatureSimulatorBase, IPointerListener
    {
        public const string TokenIdCreating = "TokenIdCreating";
        public const string TokenIdPositioning = "TokenIdPositioning";
        public const string TokenIdFinished = "TokenIdFinished";
        public const string TokenIdCancelling = "TokenIdCancelling";

        /// <summary>
        /// Tool Button Collection
        /// </summary>
        public List<PartsToolButton> Buttons { get; set; } = new List<PartsToolButton>();

        /// <summary>
        /// Initialize feature
        /// </summary>
        public override void OnInitialInstance()
        {
            base.OnInitialInstance();
            Pane.Target = Pane["ToolBox"];

            // Tool box background
            Parts.Add(Pane.Target, new PartsToolBox { }, LAYER.ToolButtonBox);

            var x = ScreenX.From(0);
            var y = ScreenY.From(48);
            var btnSize = ScreenSize.From(24, 24);
            var marginY = ScreenY.From(8);

            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];
                btn.Name = string.IsNullOrEmpty(btn.Name) ? btn.GetType().Name : btn.Name;
                btn.Size = btnSize;
                btn.Location = CodePos<ScreenX, ScreenY>.From(x, y);
                var dmy = btn.Load(View);   // dmy = thread control
                Parts.Add(Pane.Target, btn, LAYER.ToolButtons);
                y += btnSize.Height + marginY;
            }
        }

        private PartsToolButton Dragging = null;

        public void OnPointerPressed(PointerState po)
        {
            if (Dragging == null)
            {
                var tar = checkSelect(po);
                if (tar != null)
                {
                    Dragging = tar;
                    DraggingMessage(po, TokenIdCreating);
                }
            }
        }

        public void OnPointerMoved(PointerState po)
        {
            if (Dragging != null)
            {
                DraggingMessage(po, po.IsInContact ? TokenIdPositioning : TokenIdFinished);
            }
            else if (po.DeviceType == PointerState.DeviceTypes.Mouse)
            {
                checkSelect(po);
            }
        }

        public void OnPointerReleased(PointerState po)
        {
            if (po.DeviceType == PointerState.DeviceTypes.Touch || po.DeviceType == PointerState.DeviceTypes.Pen)
            {
                checkSelect(po);
            }
            DraggingMessage(po, Pane.Target.Rect.IsIn(po.Position) ? TokenIdCancelling : TokenIdFinished);
        }

        public void OnPointerHold(PointerState po)
        {
        }

        private PartsToolButton checkSelect(PointerState po)
        {
            var selected = new List<PartsToolButton>();
            foreach (var btn in Parts.GetParts<PartsToolButton>(LAYER.ToolButtons))
            {
                if (btn.Rect.IsIn(po.Position))
                {
                    btn.IsSelected = true;
                    selected.Add(btn);
                }
                else
                {
                    btn.IsSelected = false;
                }
            }
            Redraw();

            return selected.OrderBy(a => a.SelectingScore(Pane.Target, po.Position)).FirstOrDefault();
        }

        private void DraggingMessage(PointerState po, string tokenid)
        {
            if (Dragging == null)
            {
                return;
            }

            Token.AddNew(new EventTokenTriggerToolDragging
            {
                TokenID = tokenid,
                Name = Dragging.Name,
                ToolButtonType = Dragging.GetType(),
                Pointer = po,
                Sender = this,
            });
            if (tokenid == TokenIdFinished || tokenid == TokenIdCancelling)
            {
                Dragging = null;
                Status["IsEnableSelectingBox"].ValueB = true;
            }
            else
            {
                Status["IsEnableSelectingBox"].ValueB = false;
            }
        }
    }

    /// <summary>
    /// Creating Jit-instance
    /// </summary>
    public class EventTokenTriggerToolDragging : EventTokenTrigger
    {
        public string Name { get; set; }
        public Type ToolButtonType { get; set; }
        public PointerState Pointer { get; set; }
    }
}
