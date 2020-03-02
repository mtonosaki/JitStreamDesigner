// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tono.Jit;
using Windows.UI.Xaml.Controls;
using static Tono.Jit.Utils;

namespace JitStreamDesigner
{
    public sealed partial class PropertyCiKanbanReturn : UserControl, INotifyPropertyChanged, ISetPropertyTarget, IEventPropertySpecificUndoRedo, IUpdateCassette
    {
        public event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsFireEvents = true;

        public PropertyCiKanbanReturn()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Try to set target from object type
        /// </summary>
        /// <param name="target"></param>
        public void SetPropertyTarget(object target)
        {
            if (target is CiKanbanReturn co)
            {
                Target = co;
            }
        }

        public void UpdateCassette()
        {
            IsFireEvents = false;
            TargetKanbanClass = target.TargetKanbanClass;
            Delay = MakeTimeSpanString(target.Delay);
            IsFireEvents = true;
        }

        private CiKanbanReturn target;
        /// <summary>
        /// Target Jit Object
        /// </summary>
        public CiKanbanReturn Target
        {
            get => target;
            set
            {
                target = value;
                UpdateCassette();
            }
        }

        public string ID
        {
            get => Target?.ID ?? "(n/a)";
            set => new NotSupportedException();
        }

        public Dictionary<string, object> PreviousValue { get; } = new Dictionary<string, object>();

        private string targetKanbanClass = "_NONAME_";
        public string TargetKanbanClass
        {
            get => targetKanbanClass;
            set
            {
                if (value != targetKanbanClass)
                {
                    PreviousValue["TargetKanbanClass"] = targetKanbanClass;
                    targetKanbanClass = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    TargetKanbanClass = '{targetKanbanClass}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    TargetKanbanClass = '{PreviousValue["TargetKanbanClass"]}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetKanbanClass"));
                }
            }
        }
        private string delay = "987.65498D";
        public string Delay
        {
            get => delay;
            set
            {
                if (ParseTimeSpan(value) != ParseTimeSpan(delay))
                {
                    PreviousValue["Delay"] = delay;
                    delay = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    Delay = {delay}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    Delay = {PreviousValue["Delay"]}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Delay"));
                }
            }
        }
    }
}
