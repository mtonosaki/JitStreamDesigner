// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

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
            public const string QUEUE_CONSUMPTION = "FeatureUndoRedo.QUEUE_CONSUMPTION";
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
            token.TemplateChip.RedoStream.RemoveRange(token.TemplateChip.UndoRedoCurrenttPointer, token.TemplateChip.RedoStream.Count - token.TemplateChip.UndoRedoCurrenttPointer);
            token.TemplateChip.UndoStream.RemoveRange(token.TemplateChip.UndoRedoCurrenttPointer + 1, token.TemplateChip.UndoStream.Count - token.TemplateChip.UndoRedoCurrenttPointer - 1);

            // add queue
            token.TemplateChip.RedoStream.Add(token.JacRedo);
            token.TemplateChip.UndoStream.Add(token.JacUndo);

            token.TemplateChip.UndoRedoRequestedPointer++;
            QueueConsumptionTask(null);
        }

        [EventCatch(Name = "UndoButton")]
        public void Undo(EventTokenButton token)
        {
            if (Hot.ActiveTemplate.UndoRedoCurrenttPointer < 1)
            {
                LOG.AddMes(LLV.WAR, "FeatureUndoRedo-NoUndo").Solo();
                return;
            }

            Hot.ActiveTemplate.UndoRedoRequestedPointer--;
            QueueConsumptionTask(null);
        }

        [EventCatch(Name = "RedoButton")]
        public void Redo(EventTokenButton token)
        {
            if (Hot.ActiveTemplate.UndoRedoCurrenttPointer > Hot.ActiveTemplate.RedoStream.Count - 1)
            {
                LOG.AddMes(LLV.WAR, "FeatureUndoRedo-NoRedo").Solo();
                return;
            }

            Hot.ActiveTemplate.UndoRedoRequestedPointer++;
            QueueConsumptionTask(null);
        }

        [EventCatch(TokenID = TOKEN.QUEUE_CONSUMPTION)]
        public void QueueConsumptionTask(EventUndoRedoQueueConsumptionTokenTrigger token)
        {
            Token.Finalize(MoveCurrentPointer);
        }

        /// <summary>
        /// Undo / Redo pointer move
        /// </summary>
        private void MoveCurrentPointer()
        {
            var dir = MathUtil.Sgn(Hot.ActiveTemplate.UndoRedoRequestedPointer - Hot.ActiveTemplate.UndoRedoCurrenttPointer);
            while (Hot.ActiveTemplate.UndoRedoCurrenttPointer != Hot.ActiveTemplate.UndoRedoRequestedPointer && dir != 0)
            {
                var jac = dir > 0 ? Hot.ActiveTemplate.RedoStream[Hot.ActiveTemplate.UndoRedoCurrenttPointer] : Hot.ActiveTemplate.UndoStream[Hot.ActiveTemplate.UndoRedoCurrenttPointer];

                //=== INSTANCIATE WITH JAC ===
                Hot.ActiveTemplate.Jac.Exec(jac);

                Hot.ActiveTemplate.UndoRedoCurrenttPointer += dir;
            }

            ButtonEnable(UndoButton, Hot.ActiveTemplate.UndoRedoCurrenttPointer > 0);
            ButtonEnable(RedoButton, Hot.ActiveTemplate.UndoRedoCurrenttPointer < Hot.ActiveTemplate.RedoStream.Count);
        }
    }

    /// <summary>
    /// Set Undo/Redo and activate queue consumption task
    /// </summary>
    public class EventSetUndoRedoTokenTrigger : EventTokenTrigger
    {
        public TemplateTipModel TemplateChip { get; set; }
        public string JacUndo { get; set; }
        public string JacRedo { get; set; }
    }

    /// <summary>
    /// To activate queue consumption task
    /// </summary>
    public class EventUndoRedoQueueConsumptionTokenTrigger : EventTokenTrigger
    {
        public EventUndoRedoQueueConsumptionTokenTrigger()
        {
            TokenID = FeatureUndoRedo.TOKEN.QUEUE_CONSUMPTION;
        }
    }
}
