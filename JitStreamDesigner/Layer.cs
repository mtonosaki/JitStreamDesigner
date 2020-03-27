// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Tono;

namespace JitStreamDesigner
{
    public static class LAYER
    {
        public static readonly NamedId ActiveTemplate = NamedId.From("ActiveTemplate", 9011);
        public static readonly NamedId Clock = NamedId.From("Clock", 9010);    // Need to set upper with Tono.Gui.Uwp.uLayer.LogPanel
        public static readonly string DoubleBufferMap = "DoubleBufferMap";
        public static readonly NamedId ToolButtons = NamedId.From("ToolButtons", 5001);
        public static readonly NamedId ToolButtonBox = NamedId.From("ToolButtonBox", 5000);
        public static readonly NamedId SelectionMask = NamedId.From("SelectionMask", 990);
        public static readonly NamedId EventQueue = NamedId.From("EventQueue", 900);

        public static readonly NamedId JitWork = NamedId.From("JitWork", 400);
        public static readonly NamedId JitProcessConnectorGrip = NamedId.From("JitProcessConnectorGrip", 304);
        public static readonly NamedId JitProcessConnector = NamedId.From("JitProcessConnector", 301);
        public static readonly NamedId JitProcess = NamedId.From("JitProcess", 300);

        public static readonly NamedId[] JitPartsLayers = new[] { JitProcess, JitWork };
        public static readonly NamedId[] MaskIgnoreLayers = new[] { JitProcessConnectorGrip };
    }
}
