// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tono.Jit;
using Windows.UI.Xaml.Controls;

namespace JitStreamDesigner
{
    public sealed partial class PropertyCiPickTo : UserControl, INotifyPropertyChanged, ISetPropertyTarget, IEventPropertySpecificUndoRedo, IUpdateCassette
    {
        public event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsFireEvents = true;

        public PropertyCiPickTo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Try to set target from object type
        /// </summary>
        /// <param name="target"></param>
        public void SetPropertyTarget(object target)
        {
            if (target is CiPickTo co)
            {
                Target = co;
            }
        }

        public void UpdateCassette()
        {
            IsFireEvents = false;
            TargetWorkClass = target.TargetWorkClass;
            Delay = JacInterpreter.MakeTimeSpanString(target.Delay);
            DestProcessKey = target.DestProcessKey;
            IsFireEvents = true;
        }

        private CiPickTo target;
        /// <summary>
        /// Target Jit Object
        /// </summary>
        public CiPickTo Target
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

        private string targetWorkClass = "_NONAME_";
        public string TargetWorkClass
        {
            get => targetWorkClass;
            set
            {
                if (value != targetWorkClass)
                {
                    PreviousValue["TargetWorkClass"] = targetWorkClass;
                    targetWorkClass = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    TargetWorkClass = '{targetWorkClass}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    TargetWorkClass = '{PreviousValue["TargetWorkClass"]}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetWorkClass"));
                }
            }
        }

        private string delay = "987.65498D";
        public string Delay
        {
            get => delay;
            set
            {
                if (JacInterpreter.ParseTimeSpan(value) != JacInterpreter.ParseTimeSpan(delay))
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

        private string destProcessKey = "_NONAME_";
        public string DestProcessKey
        {
            get => destProcessKey;
            set
            {
                if (value != destProcessKey)
                {
                    PreviousValue["DestProcessKey"] = destProcessKey;
                    destProcessKey = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    DestProcessKey = '{destProcessKey}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    DestProcessKey = '{PreviousValue["DestProcessKey"]}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DestProcessKey"));
                }
            }
        }
    }
}
