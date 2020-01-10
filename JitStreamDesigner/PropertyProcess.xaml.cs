// Copyright(c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Tono;
using Tono.Jit;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace JitStreamDesigner
{
    public class NewUndoRedoEventArgs : EventArgs
    {
        public string NewRedo { get; set; }
        public string NewUndo { get; set; }
    }

    public sealed partial class PropertyProcess : UserControl, INotifyPropertyChanged, IPropertyInstanceName, IPropertyXy, IPropertyWh, IPropertySpecificUndoRedo
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;

        private JitProcess target;
        private const string CIOBUTTON_MARKER = "CB_";

        public PropertyProcess()
        {
            this.InitializeComponent();
            CleanDesignDummy();
        }

        private void CleanDesignDummy()
        {
            ClearCioButtons();
        }

        public Visibility HideWhenRun
        {
            get
            {
                return DesignMode.DesignModeEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ClearCioButtons()
        {
            foreach (var lane in new[] { CiLane, CoLane })
            {
                var dels = lane.Children.Where(a => a is FrameworkElement).Select(a => (FrameworkElement)a).Where(a => a.Name.StartsWith(CIOBUTTON_MARKER)).ToArray();
                foreach (var del in dels)
                {
                    lane.Children.Remove(del);
                }
            }
        }

        /// <summary>
        /// Target Jit Object
        /// </summary>
        public JitProcess Target
        {
            get => target;
            set
            {
                target = value;
                Name = target.ID;           // Control.Name to find chip
                InstanceName = target.Name;
                X = $"{((Distance)target.ChildVriables["LocationX"].Value).m}m";
                Y = $"{((Distance)target.ChildVriables["LocationY"].Value).m}m";
                W = $"{((Distance)target.ChildVriables["Width"].Value).m}m";
                H = $"{((Distance)target.ChildVriables["Height"].Value).m}m";

                ClearCioButtons();
                foreach (var cio in Target.Cios)
                {
                    AddCioButton(cio);
                }
            }
        }

        public string ID
        {
            get => Target?.ID ?? "(n/a)";
            set => new NotSupportedException();
        }

        public Dictionary<string, object> PreviousValue { get; } = new Dictionary<string, object>();

        private string instanceName;
        /// <summary>
        /// ViewModel : Instance Name
        /// </summary>
        public string InstanceName
        {
            get => instanceName;
            set
            {
                if (instanceName != value)
                {
                    PreviousValue["InstanceName"] = instanceName;
                    instanceName = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InstanceName"));
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
        private string w = "0m";
        public string W
        {
            get => w;
            set
            {
                if (Distance.Parse(value) != Distance.Parse(w))
                {
                    PreviousValue["W"] = w;
                    w = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("W"));
                }
            }
        }
        private string h = "0m";
        public string H
        {
            get => h;
            set
            {
                if (Distance.Parse(value) != Distance.Parse(h))
                {
                    PreviousValue["H"] = h;
                    h = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("H"));
                }
            }
        }

        public string GetCiMajorValue(string id)
        {
            var cio = Target.Cios.Where(a => a.ID == id).FirstOrDefault();
            return cio?.MakeShortValue() ?? "";
        }

        private void Button_Round_Click(object sender, RoutedEventArgs e)
        {
            X = $"{Math.Round(Distance.Parse(X).m)}m";
            Y = $"{Math.Round(Distance.Parse(Y).m)}m";
            W = $"{Math.Round(Distance.Parse(W).m)}m";
            H = $"{Math.Round(Distance.Parse(H).m)}m";
        }

        private async void CioAdd_Click(object sender, RoutedEventArgs e)
        {
            ICioSelectedClass pal = null;
            if (sender is FrameworkElement btn)
            {
                switch (btn.Name)
                {
                    case "CiAddButton":
                        pal = new CiPalette();
                        break;
                    case "CoAddButton":
                        pal = new CoPalette();
                        break;
                }
            }
            var ret = await ((ContentDialog)pal).ShowAsync();
            if (pal.Selected != null)
            {
                var id = JacInterpreter.MakeID("CIO");
                NewUndoRedo?.Invoke(this, new NewUndoRedoEventArgs
                {
                    NewRedo = $"{Target.ID}\r\n" +                              // Process.ID
                              $"    Cio\r\n" +                                  // Cio Collection
                              $"        add new {pal.Selected.Name}\r\n" +      // Add the selected Ci/Co
                              $"            ID = '{id}'\r\n" +                  // Specified ID because it will be used by Undo with same ID
                              $"Gui.UpdateProcessCio = 'add,{Target.ID},{id}'\r\n",

                    NewUndo = $"Gui.UpdateProcessCio = 'remove,{Target.ID},{id}'\r\n" +
                              $"{Target.ID}\r\n" +
                              $"    Cio\r\n" +
                              $"        remove {id}\r\n",
                });
                // wait REDO mechanism to add CioButton
            }
        }

        /// <summary>
        /// Come from Gui.Broker
        /// </summary>
        /// <param name="action">add/remove that set at Jac(Undo/Redo) above</param>
        /// <param name="cio"></param>
        public void UpdateCioButton(string action, string cioid)
        {
            if (action.Equals("add"))
            {
                var cio = Target.Cios.Where(a => a.ID == cioid).FirstOrDefault();
                AddCioButton(cio);
            }
            if (action.Equals("remove"))
            {
                foreach (var lane in new[] { CiLane, CoLane })
                {
                    var dels = lane.Children.Select(a => (Button)a).Where(a => a.Name == $"{CIOBUTTON_MARKER}{cioid}").ToArray();
                    foreach (var del in dels)
                    {
                        lane.Children.Remove(del);
                    }
                }
            }
        }

        private void AddCioButton(CioBase cio)
        {
            var lane = cio is CiBase ? CiLane : CoLane;
            if( lane.Children.Where(a => ((FrameworkElement)a).Name.StartsWith($"{CIOBUTTON_MARKER}{cio.ID}")).FirstOrDefault() != null)
            {
                return; // Already added button
            }

            //< Button x: Name = "CB_Dummy1" Background = "Transparent" Margin = "0,-6" >
            //  < Button.Content >
            //      < StackPanel Orientation = "Horizontal" >
            //          < Image Width = "18" Height = "18" Source = "./Assets/CiDelay.png" />
            //          < TextBlock VerticalAlignment = "Center" Foreground = "White" FontSize = "9" >< Run >= 25.2S </ Run ></ TextBlock >
            //      </ StackPanel >
            //  </ Button.Content >
            //</ Button >

            StackPanel btnContent;
            Button btn;
            lane.Children.Insert(lane.Children.Count - 1, btn = new Button
            {
                Name = $"{CIOBUTTON_MARKER}{cio.ID}",
                Background = new SolidColorBrush(Colors.Transparent),
                Margin = new Thickness { Left = 0, Top = -6, Right = 0, Bottom = -6 },
                Content = btnContent = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                },
            });
            btnContent.Children.Add(new Image
            {
                Width = 18,
                Height = 18,
                Source = new BitmapImage(new Uri($"ms-appx:///Assets/{cio.GetType().Name}.png")),
            });
            var shortCaption = GetCiMajorValue(cio.ID);
            if (string.IsNullOrEmpty(shortCaption) == false)
            {
                TextBlock btnCaption;
                btnContent.Children.Add(btnCaption = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 10,
                });
                btnCaption.Text = $" {shortCaption}";
            }

            ToolTipService.SetToolTip(btn, $"{cio.GetType().Name} {cio.MakeShortValue()}");
        }
    }
}
