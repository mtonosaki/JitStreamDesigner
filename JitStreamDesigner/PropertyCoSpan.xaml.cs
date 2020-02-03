// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tono.Jit;
using Windows.UI.Xaml.Controls;

namespace JitStreamDesigner
{
    public sealed partial class PropertyCoSpan : UserControl, INotifyPropertyChanged, ISetPropertyTarget, IEventPropertySpecificUndoRedo, IUpdateCassette
    {
        public event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsFireEvents = true;

        public PropertyCoSpan()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Try to set target from object type
        /// </summary>
        /// <param name="target"></param>
        public void SetPropertyTarget(object target)
        {
            if (target is CoSpan co)
            {
                Target = co;
            }
        }

        public void UpdateCassette()
        {
            IsFireEvents = false;
            Span = JacInterpreter.MakeTimeSpanString(target.Span);
            PorlingSpan = JacInterpreter.MakeTimeSpanString(target.PorlingSpan);
            IsFireEvents = true;
        }

        private CoSpan target;
        /// <summary>
        /// Target Jit Object
        /// </summary>
        public CoSpan Target
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

        private string span = "0S";
        public string Span
        {
            get => span;
            set
            {
                if (JacInterpreter.ParseTimeSpan(value) != JacInterpreter.ParseTimeSpan(span))
                {
                    PreviousValue["Span"] = span;
                    span = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    Span = {span}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    Span = {PreviousValue["Span"]}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Span"));
                }
            }
        }

        private string porlingspan = "0S";
        public string PorlingSpan
        {
            get => porlingspan;
            set
            {
                if (JacInterpreter.ParseTimeSpan(value) != JacInterpreter.ParseTimeSpan(porlingspan))
                {
                    PreviousValue["PorlingSpan"] = porlingspan;
                    porlingspan = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    PorlingSpan = {porlingspan}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    PorlingSpan = {PreviousValue["PorlingSpan"]}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PorlingSpan"));
                }
            }
        }
    }
}
