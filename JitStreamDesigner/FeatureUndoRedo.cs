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
            Hot.UndoStream.Add("// no action here");
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
            // Cut pro-queue data
            Hot.RedoStream.RemoveRange(Hot.UndoRedoCurrenttPointer, Hot.RedoStream.Count - Hot.UndoRedoCurrenttPointer);
            Hot.UndoStream.RemoveRange(Hot.UndoRedoCurrenttPointer + 1, Hot.UndoStream.Count - Hot.UndoRedoCurrenttPointer - 1);

            // add queue
            Hot.RedoStream.Add(token.JacRedo);
            Hot.UndoStream.Add(token.JacUndo);

            Hot.UndoRedoRequestedPointer++;
            Token.Finalize(MoveCurrentPointer);
        }

        /// <summary>
        /// Undo / Redo pointer move
        /// </summary>
        private void MoveCurrentPointer()
        {
            var dir = MathUtil.Sgn(Hot.UndoRedoRequestedPointer - Hot.UndoRedoCurrenttPointer);
            while (Hot.UndoRedoCurrenttPointer != Hot.UndoRedoRequestedPointer && dir != 0)
            {
                string jac = dir > 0 ? Hot.RedoStream[Hot.UndoRedoCurrenttPointer] : Hot.UndoStream[Hot.UndoRedoCurrenttPointer];

                Hot.ActiveTemplate.Jac.Exec(jac);
                Hot.UndoRedoCurrenttPointer += dir;
            }

            ButtonEnable(UndoButton, Hot.UndoRedoCurrenttPointer > 0);
            ButtonEnable(RedoButton, Hot.UndoRedoCurrenttPointer < Hot.RedoStream.Count);
        }

        [EventCatch(Name = "UndoButton")]
        public void Undo(EventTokenButton token)
        {
            if (Hot.UndoRedoCurrenttPointer < 1) return;

            Hot.UndoRedoRequestedPointer--;
            Token.Finalize(MoveCurrentPointer);
        }

        [EventCatch(Name = "RedoButton")]
        public void Redo(EventTokenButton token)
        {
            if (Hot.UndoRedoCurrenttPointer > Hot.RedoStream.Count - 1) return;

            Hot.UndoRedoRequestedPointer++;
            Token.Finalize(MoveCurrentPointer);
        }
    }

    public class EventSetUndoRedoTokenTrigger : EventTokenTrigger
    {
        public string JacUndo { get; set; }
        public string JacRedo { get; set; }
    }
}
