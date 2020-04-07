using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tono;
using Tono.Gui.Uwp;
using Tono.Jit;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace JitStreamDesigner
{
    public sealed partial class PropertyWork : UserControl, INotifyPropertyChanged, IPropertyXy, IEventPropertySpecificUndoRedo, ISetPropertyTarget, IUpdateCassette
    {
        /// <summary>
        /// Property Changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Request to save new Undo/Redo Jac
        /// </summary>
        public event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;

        private bool IsFireEvents = true;


        public PropertyWork()
        {
            this.InitializeComponent();
        }

        public Visibility HideWhenRun => DesignMode.DesignModeEnabled ? Visibility.Visible : Visibility.Collapsed;

        private JitWork _target;

        /// <summary>
        /// Target Jit Object
        /// </summary>
        public JitWork Target
        {
            get => _target;
            set
            {
                _target = value;
                UpdateCassette();
            }
        }

        public void UpdateCassette()
        {
            IsFireEvents = false;
            Name = Target.ID;           // Control.Name to find chip
            X = $"{((Distance)Target.ChildVriables["LocationX"].Value).m}m";
            Y = $"{((Distance)Target.ChildVriables["LocationY"].Value).m}m";
            if (Target.FindStage() is JitStage st)
            {
                var node = st.Events.Find(Target);
                TimeToPush = node.Value.DT.ToString(TimeUtil.FormatYMDHMSms);
            }
            IsFireEvents = true;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NextLocation"));
        }

        public string ID
        {
            get => Target?.ID ?? "(n/a)";
            set => new NotSupportedException();
        }

        public Dictionary<string, object> PreviousValue { get; } = new Dictionary<string, object>();

        /// <summary>
        /// ViewModel : Instance Name
        /// </summary>
        public string InstanceName { get => Target?.Name ?? "(n/a)";  }    // Not Editable

        public string NextLocation
        {
            get => Target?.Next?.FullPath;
        }

        //TimeToPush
        private string timeToPush = "--";
        public string TimeToPush
        {
            get => timeToPush;
            set
            {
                var dt = JacInterpreter.ParseDateTime(value, DateTime.MinValue);
                if ( dt != JacInterpreter.ParseDateTime(timeToPush, DateTime.MinValue))
                {
                    PreviousValue["TimeToPush"] = timeToPush;
                    timeToPush = value;
                    if (IsFireEvents)
                    {
                        NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                        {
                            NewRedo = $"TheStage\r\n" +
                                      $"    Works\r\n" +
                                      $"        remove {Target.ID}\r\n" +
                                      $"        add datetime('{dt.ToString(TimeUtil.FormatYMDHMSms)}'):{Target.ID}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                            NewUndo = $"TheStage\r\n" +
                                      $"    Works\r\n" +
                                      $"        remove {Target.ID}\r\n" +
                                      $"        add datetime('{PreviousValue["TimeToPush"]}'):{Target.ID}\r\n" +
                                      $"Gui.UpdateCassetteValue = {Target.ID}\r\n",
                        });
                    }
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TimeToPush"));
                }
            }
        }

        private string x = "0m";
        public string X
        {
            get => x;
            set
            {
                if (Distance.Parse(value) != Distance.Parse(x))
                {
                    PreviousValue["X"] = x;
                    x = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("X"));
                }
            }
        }
        private string y = "0m";
        public string Y
        {
            get => y;
            set
            {
                if (Distance.Parse(value) != Distance.Parse(y))
                {
                    PreviousValue["Y"] = y;
                    y = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Y"));
                }
            }
        }

        private void Button_Round_Click(object sender, RoutedEventArgs e)
        {
            X = $"{Math.Round(Distance.Parse(X).m)}m";
            Y = $"{Math.Round(Distance.Parse(Y).m)}m";
        }

        public void SetPropertyTarget(object target)
        {
            if (target is JitWork work)
            {
                Target = work;
            }
        }
    }
}
