// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JitStreamDesigner
{
    /// <summary>
    /// Persist data (for save / load target) Poco Model
    /// </summary>
    public class PersistModel
    {
        /// <summary>
        /// Simulation Clock
        /// </summary>
        [DataMember]
        public DateTime SimStartTime { get; set; }

        /// <summary>
        /// Simulation clock step unit
        /// </summary>
        [DataMember]
        public TimeSpan ClockTick { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// JitStreamDesigner template list
        /// </summary>
        [DataMember]
        public TemplateTipCollection TemplateList { get; private set; } = new TemplateTipCollection();

    }
}
