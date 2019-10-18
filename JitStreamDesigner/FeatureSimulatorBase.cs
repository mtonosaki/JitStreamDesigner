using System;
using Tono.Gui.Uwp;

namespace JitStreamDesigner
{
    /// <summary>
    /// Simulator utility
    /// </summary>
    public abstract class FeatureSimulatorBase : FeatureBase
    {
        /// <summary>
        /// common cold data context
        /// </summary>
        public DataCold Cold => base.DataCold as DataCold;

        /// <summary>
        /// common hot data context
        /// </summary>
        public DataHot Hot => base.DataHot as DataHot;

        /// <summary>
        /// Simulator clock current time
        /// </summary>
        public DateTime Now { get; set; }
    }
}
