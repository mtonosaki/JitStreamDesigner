// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;

namespace JitStreamDesigner
{
    public class NewUndoRedoEventArgs : EventArgs
    {
        public string NewRedo { get; set; }
        public string NewUndo { get; set; }
    }
}
