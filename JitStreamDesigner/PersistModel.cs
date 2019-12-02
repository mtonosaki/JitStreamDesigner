// Copyright(c) Manabu Tonosaki All rights reserved.
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
        /// REDO Jac Queue
        /// </summary>
        [DataMember]
        public List<string> RedoStream { get; set; } = new List<string>();

        /// <summary>
        /// UNDO Jac Queue (First [0] data is a dummy)
        /// </summary>
        [DataMember]
        public List<string> UndoStream { get; set; } = new List<string>();

        /// <summary>
        /// UNDO/REDO Current Pointer
        /// </summary>
        [DataMember]
        public int UndoRedoCurrenttPointer { get; set; } = 0;

        /// <summary>
        /// UNDO/REDO Requested Pointer position
        /// </summary>
        [DataMember]
        public int UndoRedoRequestedPointer { get; set; } = 0;
    }

}
