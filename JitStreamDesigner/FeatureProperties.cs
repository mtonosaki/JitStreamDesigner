// Copyright(c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;
using System.Text;
using Tono;
using Tono.Gui.Uwp;
using Tono.Jit;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JitStreamDesigner
{
    public class FeatureProperties : FeatureSimulatorBase
    {
        public static class TOKEN
        {
            public const string PROPERTYOPEN = "FeaturePropertiesOpen";
        };

        private StackPanel Casettes = null;

        public override void OnInitialInstance()
        {
            base.OnInitialInstance();

            Casettes = ControlUtil.FindControl(View, "PropertyCasettes") as StackPanel;
            if (Casettes == null)
            {
                Kill(new NotImplementedException("FeatureProperties need a StackPanel named 'PropertyCasettes' to show property controls."));
            }

            redosaver = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200),
            };
            redosaver.Tick += Redosaver_Tick;
        }

        [EventCatch(TokenID = TOKEN.PROPERTYOPEN)]
        public void PropertyOpen(EventTokenTriggerPropertyOpen token)
        {
            if (token.Target is JitProcess proc)
            {
                var obj = Casettes.FindName(proc.ID);
                if (obj is UIElement ue)
                {
                    Casettes.Children.Remove(ue);
                }
                PropertyProcess pp;
                Casettes.Children.Insert(0, pp = new PropertyProcess
                {
                    Target = proc,
                });
                pp.PropertyChanged += OnPropertyChanged;
            }
        }

        StringBuilder jacUndo = new StringBuilder();
        StringBuilder jacRedo = new StringBuilder();

        DispatcherTimer redosaver = new DispatcherTimer();

        /// <summary>
        /// Make chunk of REDO/UNDO (for Round function that change two properties W and H)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Redosaver_Tick(object sender, object e)
        {
            redosaver.Stop();
            if (jacRedo.Length < 1)
            {
                return;
            }
            SetNewAction(null, jacRedo.ToString(), jacUndo.ToString());
            jacRedo.Clear();
            jacUndo.Clear();
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is PropertyProcess model)
            {
                switch (e.PropertyName)
                {
                    case "InstanceName":
                        jacRedo.AppendLine($@"{model.Target.ID}.Name = '{model.InstanceName}'");
                        jacRedo.AppendLine($@"Gui.UpdateName = {model.Target.ID}");
                        jacUndo.AppendLine($@"{model.Target.ID}.Name = '{model.PreviousValue[e.PropertyName]}'");
                        jacUndo.AppendLine($@"Gui.UpdateName = {model.Target.ID}");
                        break;
                    case "X":
                        jacRedo.AppendLine($@"{model.Target.ID}.LocationX = {model.X}");
                        jacRedo.AppendLine($@"Gui.UpdateLocation = {model.Target.ID}");
                        jacUndo.AppendLine($@"{model.Target.ID}.LocationX = {model.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateLocation = {model.Target.ID}");
                        break;
                    case "Y":
                        jacRedo.AppendLine($@"{model.Target.ID}.LocationY = {model.Y}");
                        jacRedo.AppendLine($@"Gui.UpdateLocation = {model.Target.ID}");
                        jacUndo.AppendLine($@"{model.Target.ID}.LocationY = {model.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateLocation = {model.Target.ID}");
                        break;
                    case "W":
                        jacRedo.AppendLine($@"{model.Target.ID}.Width = {model.W}");
                        jacRedo.AppendLine($@"Gui.UpdateSize = {model.Target.ID}");
                        jacUndo.AppendLine($@"{model.Target.ID}.Width = {model.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateSize = {model.Target.ID}");
                        break;
                    case "H":
                        jacRedo.AppendLine($@"{model.Target.ID}.Height = {model.H}");
                        jacRedo.AppendLine($@"Gui.UpdateSize = {model.Target.ID}");
                        jacUndo.AppendLine($@"{model.Target.ID}.Height = {model.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateSize = {model.Target.ID}");
                        break;
                }
                redosaver.Stop();
                redosaver.Start();
            }
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.LocationChanged)]
        public void LocationChanged(EventTokenJitVariableTrigger token)
        {
            if (token.Target is IJitObjectID tar && token.Target is JitVariable va)
            {
                if (Casettes.FindName(tar.ID) is IPropertyXy chip)
                {
                    var pp = chip as INotifyPropertyChanged;
                    pp.PropertyChanged -= OnPropertyChanged;

                    chip.X = $"{((Distance)va.ChildVriables["LocationX"].Value).m}m";
                    chip.Y = $"{((Distance)va.ChildVriables["LocationY"].Value).m}m";

                    pp.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.SizeChanged)]
        public void SizeChanged(EventTokenJitVariableTrigger token)
        {
            if (token.Target is IJitObjectID tar && token.Target is JitVariable va)
            {
                if (Casettes.FindName(tar.ID) is IPropertyWh chip)
                {
                    var pp = chip as INotifyPropertyChanged;
                    pp.PropertyChanged -= OnPropertyChanged;

                    chip.W = $"{((Distance)va.ChildVriables["Width"].Value).m}m";
                    chip.H = $"{((Distance)va.ChildVriables["Height"].Value).m}m";

                    pp.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.NameChanged)]
        public void NameChanged(EventTokenJitVariableTrigger token)
        {
            if (token.Target is IJitObjectID tar && token.Target is JitVariable va)
            {
                if (Casettes.FindName(tar.ID) is IPropertyInstanceName chip && chip.InstanceName != va.Name)
                {
                    var pp = chip as INotifyPropertyChanged;
                    pp.PropertyChanged -= OnPropertyChanged;
                    chip.InstanceName = va.Name;
                    pp.PropertyChanged += OnPropertyChanged;
                }
            }
        }

        [EventCatch(TokenID = FeatureJitProcess.TOKEN.REMOVE)]
        public void ProcessRemoved(EventTokenProcessPartsTrigger token)
        {
            if (Casettes.FindName(token.Process.ID) is UserControl chip)
            {
                Casettes.Children.Remove(chip);
            }
        }
    }

    /// <summary>
    /// Creating Jit-instance
    /// </summary>
    public class EventTokenTriggerPropertyOpen : EventTokenTrigger
    {
        public override string TokenID { get => FeatureProperties.TOKEN.PROPERTYOPEN; set => throw new NotSupportedException(); }
        public object Target { get; set; }
    }
}
