// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Tono.Gui.Uwp;
using Tono.Jit;

namespace JitStreamDesigner
{
    /// <summary>
    /// Creating Jit-instance
    /// </summary>
    public class EventTokenTriggerPropertyOpen : EventTokenTrigger
    {
        public override string TokenID { get => FeatureProperties.TOKEN.PROPERTYOPEN; set => throw new NotSupportedException(); }
        public object Target { get; set; }
    }

    /// <summary>
    /// Variable message
    /// </summary>
    public class EventTokenJitVariableTrigger : EventTokenTrigger
    {
        /// <summary>
        /// Value changed target
        /// </summary>
        public IJitObjectID From { get; set; }
    }

    /// <summary>
    /// Cio message
    /// </summary>
    public class EventTokenJitCioTrigger : EventTokenTrigger
    {
        /// <summary>
        /// add / remove / update
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The process that have Cio
        /// </summary>
        public string TargetProcessID { get; set; }

        /// <summary>
        /// Value changed target
        /// </summary>
        public string FromCioID { get; set; }
    }

    public class EventTokenCioBasedCassetteValueChangedTrigger : EventTokenTrigger
    {
        public CioBase Cio { get; set; }
        public string CassetteID { get; set; }
    }

    public class EventTokenVariableBasedCassetteValueChangedTrigger : EventTokenTrigger
    {
        public JitVariable Variable { get; set; }   // JitProcess / JitWork
        public string CassetteID { get; set; }
    }

    public class EventTokenProcessLinkTrigger : EventTokenTrigger
    {
        public string ProcessIDFrom { get; set; }
        public string ProcessIDTo { get; set; }
    }
}
