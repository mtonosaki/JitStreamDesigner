// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Tono;
using Tono.Gui.Uwp;
using Tono.Jit;

namespace JitStreamDesigner
{
    /// <summary>
    /// Feature UNDO / REDO processor
    /// the All activities will be processed with REDO function
    /// </summary>
    public class FeatureUndoRedo : FeatureSimulatorBase
    {
        public static class TOKEN
        {
            public const string REDO = "FeatureUndoRedo.REDO";
            public const string UNDO = "FeatureUndoRedo.UNDO";
            public const string SET = "FeatureUndoRedo.SET";
        }
        private readonly List<string> RedoStream = new List<string>();
        private readonly List<string> UndoStream = new List<string>();
        private int CurrenttPointer = 0;
        private int RequestedPointer = 0;

        public override void OnInitialInstance()
        {
            JacInterpreter.RegisterJacTarget(typeof(FeatureGuiJacBroker).Assembly);
            UndoStream.Add("// no action here");
        }

        [EventCatch(TokenID = TOKEN.SET)]
        public void Set(EventSetUndoRedoTokenTrigger token)
        {
            RedoStream.Add(token.JacRedo);
            UndoStream.Add(token.JacUndo);
            RequestedPointer++;
            Token.Finalize(MoveCurrentPointer);
        }

        /// <summary>
        /// Undo / Redo pointer move
        /// </summary>
        private void MoveCurrentPointer()
        {
            var dir = MathUtil.Sgn(RequestedPointer - CurrenttPointer);
            while (CurrenttPointer != RequestedPointer && dir != 0)
            {
                string jac = dir > 0 ? RedoStream[CurrenttPointer] : UndoStream[CurrenttPointer];

                // TODO: TONO NOT WORK    Gui  Object yet
                Hot.ActiveTemplate.Jac.Exec(jac);
                CurrenttPointer += dir;
            }
        }
    }

    public class EventSetUndoRedoTokenTrigger : EventTokenTrigger
    {
        public string JacUndo { get; set; }
        public string JacRedo { get; set; }
    }
}
