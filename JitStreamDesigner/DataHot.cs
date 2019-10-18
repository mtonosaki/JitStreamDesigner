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
        /// シミュレーション開始時刻
        /// </summary>
        public DateTime SimStartTime { get; set; }

        /// <summary>
        /// シミュレーション粒度
        /// </summary>
        public TimeSpan ClockTick { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Virtual TPS Player Modeling Language Root JitStage
        /// </summary>
        public JitStage JitStage { get; set; }

        /// <summary>
        /// 相対時間からシミュレーション時刻を計算する
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public DateTime SimTime(TimeSpan span)
        {
            return SimStartTime + span;
        }

        /// <summary>
        /// Excelからロードした かんばんを 一時保管しておくところ
        /// </summary>
        public BlockingCollection<JitKanban> LoadJitKanbans { get; private set; } = new BlockingCollection<JitKanban>();
        public BlockingCollection<JitWork> LoadJitWorks { get; private set; } = new BlockingCollection<JitWork>();

        public IEnumerable<JitVariable> AllJitVariables => LoadJitKanbans.Select(a => (JitVariable)a).Concat(LoadJitWorks);
    }
}
