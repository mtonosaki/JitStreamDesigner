// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Tono.Gui.Uwp;

namespace JitStreamDesigner
{
    /// <summary>
    /// Hot data model context
    /// </summary>
    public class DataHot : DataHotBase
    {
        /// <summary>
        /// Persist data (for save / load target) Poco Model
        /// </summary>
        public PersistModel PersistTarget { get; set; } = new PersistModel();

        /// <summary>
        /// Simulation Clock
        /// </summary>
        public DateTime SimStartTime { get => PersistTarget.SimStartTime; set => PersistTarget.SimStartTime = value; }

        /// <summary>
        /// Simulation clock step unit
        /// </summary>
        public TimeSpan ClockTick { get => PersistTarget.ClockTick; set => PersistTarget.ClockTick = value; }

        /// <summary>
        /// Simulator Clock
        /// </summary>
        public DateTime Now { get; set; }

        /// <summary>
        /// JitStreamDesigner template list
        /// </summary>
        public TemplateTipCollection TemplateList => PersistTarget.TemplateList;

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
