// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using Tono;
using Tono.Gui.Uwp;
using Tono.Jit;
using Windows.UI;
using Windows.UI.Xaml.Media;

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
        private TButton UndoButton = null, RedoButton = null;
        private Color ButtonForegroundColor = Colors.White;

        public override void OnInitialInstance()
        {
            UndoButton = (TButton)ControlUtil.FindControl(View, "UndoButton");
            RedoButton = (TButton)ControlUtil.FindControl(View, "RedoButton");
            if (UndoButton.Foreground is SolidColorBrush scb)
            {
                ButtonForegroundColor = scb.Color;
            }
            ButtonEnable(UndoButton, false);
            ButtonEnable(RedoButton, false);

            JacInterpreter.RegisterJacTarget(typeof(FeatureGuiJacBroker).Assembly);
            UndoStream.Add("// no action here");
        }

        /// <summary>
        /// Set button design Enable like / Disable like
        /// </summary>
        /// <param name="button"></param>
        /// <param name="sw"></param>
        private void ButtonEnable(TButton button, bool sw = true)
        {
            if (sw)
            {
                button.Foreground = new SolidColorBrush(ButtonForegroundColor);
            }
            else
            {
                button.Foreground = new SolidColorBrush(ColorUtil.ChangeAlpha(ButtonForegroundColor, 0.25f));
            }
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

                Hot.ActiveTemplate.Jac.Exec(jac);
                CurrenttPointer += dir;
            }

            ButtonEnable(UndoButton, CurrenttPointer > 0);
            ButtonEnable(RedoButton, CurrenttPointer < RedoStream.Count);
        }

        [EventCatch(Name = "UndoButton")]
        public void Undo(EventTokenButton token)
        {
            if (CurrenttPointer < 1) return;

            RequestedPointer--;
            Token.Finalize(MoveCurrentPointer);
        }

        [EventCatch(Name = "RedoButton")]
        public void Redo(EventTokenButton token)
        {
            if (CurrenttPointer > RedoStream.Count - 1) return;

            RequestedPointer++;
            Token.Finalize(MoveCurrentPointer);
        }
    }

    public class EventSetUndoRedoTokenTrigger : EventTokenTrigger
    {
        public string JacUndo { get; set; }
        public string JacRedo { get; set; }
    }
}
