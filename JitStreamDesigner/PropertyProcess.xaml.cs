﻿// Copyright(c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Tono;
using Tono.Jit;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace JitStreamDesigner
{
    public sealed partial class PropertyProcess : UserControl, INotifyPropertyChanged, IPropertyInstanceName, IPropertyXy, IPropertyWh
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private JitProcess target;

        public PropertyProcess()
        {
            this.InitializeComponent();
            CleanDesignDummy();

        }

        private void CleanDesignDummy()
        {
            foreach (var lane in new[] { CiLane, CoLane })
            {
                var dels = lane.Children.Where(a => a is FrameworkElement).Select(a => (FrameworkElement)a).Where(a => a.Name.StartsWith("Cio_")).ToArray();
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

        private async void CiAdd_Click(object sender, RoutedEventArgs e)
        {
            var pal = new CiPalette();
            var ret = await pal.ShowAsync();
            if (Activator.CreateInstance(pal.SelectedCommand) is CiBase ci)
            {
                AddCioButton(ci, CiLane);
            }
        }

        private async void CoAdd_Click(object sender, RoutedEventArgs e)
        {
            var pal = new CoPalette();
            var ret = await pal.ShowAsync();
            if (Activator.CreateInstance(pal.SelectedConstraint) is CoBase co)
            {
                AddCioButton(co, CoLane);
            }
        }

        private void AddCioButton(CioBase cio, StackPanel lane)
        {
            Target.CioAdd(cio);  // Add in-command to Process

            //< Button x: Name = "Cio_Dummy1" Background = "Transparent" Margin = "0,-6" >
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
                Name = $"Cio_{cio.ID}",
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