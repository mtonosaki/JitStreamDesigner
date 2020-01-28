// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Tono.Jit;

namespace JitStreamDesigner
{
    public class CioClickedEventArgs : EventArgs
    {
        public JitProcess TargetProcess { get; set; }
        public CioBase Cio { get; set; }
    }
}
