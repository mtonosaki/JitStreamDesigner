using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tono.Jit;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace JitStreamDesigner
{
    public sealed partial class PropertyCoJoinFrom : UserControl, INotifyPropertyChanged, ISetPropertyTarget, IEventPropertySpecificUndoRedo, IUpdateCassette
    {
        public event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;
        public event PropertyChangedEventHandler PropertyChanged;

        private bool IsFireEvents = true;

        public PropertyCoJoinFrom()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Try to set target from object type
        /// </summary>
        /// <param name="target"></param>
        public void SetPropertyTarget(object target)
        {
            if (target is CoJoinFrom co)
            {
                Target = co;
            }
        }

        public void UpdateCassette()
        {
            IsFireEvents = false;
            PullFromProcessKey = target.PullFromProcessKey;
            ChildWorkKey = target.ChildWorkKey;
            PorlingSpan = JacInterpreter.MakeTimeSpanString(target.PorlingSpan);
            IsFireEvents = true;
        }

        private CoJoinFrom target;
        /// <summary>
        /// Target Jit Object
        /// </summary>
        public CoJoinFrom Target
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

        private string pullFromProcessKey = "_NONAME_";
        public string PullFromProcessKey
        {
            get => pullFromProcessKey;
            set
            {
                if (value != pullFromProcessKey)
                {
                    PreviousValue["PullFromProcessKey"] = pullFromProcessKey;
                    pullFromProcessKey = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    PullFromProcessKey = '{pullFromProcessKey}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    PullFromProcessKey = '{PreviousValue["PullFromProcessKey"]}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PullFromProcessKey"));
                }
            }
        }

        private string childWorkKey = "_NONAME_";
        public string ChildWorkKey
        {
            get => childWorkKey;
            set
            {
                if (value != childWorkKey)
                {
                    PreviousValue["ChildWorkKey"] = childWorkKey;
                    childWorkKey = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"{Target.ID}\r\n" +
                                      $"    ChildWorkKey = '{childWorkKey}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"{Target.ID}\r\n" +
                                      $"    ChildWorkKey = '{PreviousValue["ChildWorkKey"]}'\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ChildWorkKey"));
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
