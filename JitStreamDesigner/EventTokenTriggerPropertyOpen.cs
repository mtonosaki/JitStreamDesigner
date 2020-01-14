// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Tono.Gui.Uwp;

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
}
