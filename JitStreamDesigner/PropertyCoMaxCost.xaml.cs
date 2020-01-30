// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Tono.Jit;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace JitStreamDesigner
{
    public sealed partial class PropertyCoMaxCost : UserControl, INotifyPropertyChanged, ISetPropertyTarget, IEventPropertySpecificUndoRedo, IUpdateCassette
    {
        public event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsFireEvents = true;

        public PropertyCoMaxCost()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Try to set target from object type
        /// </summary>
        /// <param name="target"></param>
        public void SetPropertyTarget(object target)
        {
            if (target is CoMaxCost co)
            {
                Target = co;
            }
        }

        public void UpdateCassette()
        {
            IsFireEvents = false;
            ReferenceVarName = target.ReferenceVarName.Value?.ToString() ?? "";
            Value = target.Value.ToString();
            IsFireEvents = true;
        }

        private CoMaxCost target;
        /// <summary>
        /// Target Jit Object
        /// </summary>
        public CoMaxCost Target
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

        private string referenceVarName = "_NONAME_";
        public string ReferenceVarName
        {
            get => referenceVarName;
            set
            {
                if (value != referenceVarName)
                {
                    PreviousValue["ReferenceVarName"] = referenceVarName;
                    referenceVarName = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    ReferenceVarName = {referenceVarName}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    ReferenceVarName = {PreviousValue["ReferenceVarName"]}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReferenceVarName"));
                }
            }
        }

        private string _value = "0";
        public string Value
        {
            get => _value;
            set
            {
                if (double.Parse(value) != double.Parse(_value))
                {
                    PreviousValue["Value"] = _value;
                    _value = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    Value = {_value}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    Value = {PreviousValue["Value"]}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }
    }
}
