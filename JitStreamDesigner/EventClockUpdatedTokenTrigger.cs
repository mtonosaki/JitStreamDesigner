using System;
using Tono.Gui.Uwp;

namespace JitStreamDesigner
{
    /// <summary>
    /// クロック更新トークンイベント
    /// </summary>
    public class EventClockUpdatedTokenTrigger : EventTokenTrigger
    {
        public DateTime Pre { get; set; }
        public DateTime Now { get; set; }
    }
}
