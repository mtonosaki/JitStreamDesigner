// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Tono.Gui.Uwp;
using Tono.Jit;

namespace JitStreamDesigner
{
    /// <summary>
    /// Hot data model context
    /// </summary>
    public class DataHot : DataHotBase
    {
        /// <summary>
        /// Simulation Clock
        /// </summary>
        public DateTime SimStartTime { get; set; }

        /// <summary>
        /// Simulation clock step unit
        /// </summary>
        public TimeSpan ClockTick { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Virtual TPS Player Modeling Language Root JitStage
        /// </summary>
        public JitStage JitStage { get; set; }

        /// <summary>
        /// Calclate Simulation time
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public DateTime SimTime(TimeSpan span)
        {
            return SimStartTime + span;
        }

        /// <summary>
        /// JitStreamDesigner template list
        /// </summary>
        public TemplateTipCollection TemplateList { get; private set; } = new TemplateTipCollection();

        /// <summary>
        /// JitStreamDesigner Template target (GUI)
        /// </summary>
        public TemplateTipModel ActiveTemplate { get; set; }

        /// <summary>
        /// Keyboard shortcut disable flag (When you input text in DialogBox, TGuiView receives key event)
        /// </summary>
        public Dictionary<string/*name*/, bool> KeybordShortcutDisabledFlags { get; private set; } = new Dictionary<string, bool>();

        /// <summary>
        /// The broker instance set by FeatureGuiJacBroker
        /// </summary>
        public FeatureGuiJacBroker TheBroker { get; set; }
    }
}
