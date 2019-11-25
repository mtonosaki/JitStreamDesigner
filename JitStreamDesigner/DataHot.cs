using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        public List<TemplateTipModel> TemplateList { get; private set; } = new List<TemplateTipModel>();

        /// <summary>
        /// JitStreamDesigner Template target (GUI)
        /// </summary>
        public TemplateTipModel ActiveTemplate { get; set; }
    }
}
