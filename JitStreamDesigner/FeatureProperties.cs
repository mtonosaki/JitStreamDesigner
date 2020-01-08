// Copyright(c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Tono;
using Tono.Gui.Uwp;
using Tono.Jit;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace JitStreamDesigner
{
    public class FeatureProperties : FeatureSimulatorBase
    {
        public static class TOKEN
        {
            public const string PROPERTYOPEN = "FeaturePropertiesOpen";
        };

        private ScrollViewer ScrollView = null;
        private StackPanel Screen = null;

        public override void OnInitialInstance()
        {
            base.OnInitialInstance();

            ScrollView = ControlUtil.FindControl(View, "View") as ScrollViewer;
            Screen = ControlUtil.FindControl(View, "Screen") as StackPanel;
            if (Screen == null || ScrollView == null)
            {
                Kill(new NotImplementedException("FeatureProperties need both a StackPanel named 'Screen' and a ScrollViewer named 'View'."));
            }

            redosaver = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200),
            };
            redosaver.Tick += Redosaver_Tick;
        }

        private (StackPanel Parent, UIElement Cassette) FindCassette(string xname)
        {
            var cassettes =
                from lane0 in Screen.Children
                let lane = lane0 as StackPanel
                where lane != null
                from cas0 in lane.Children
                let cassette = cas0 as FrameworkElement
                where cassette != null
                where cassette.Name == xname
                select (lane, cassette);
            return cassettes.FirstOrDefault();
        }

        private StackPanel SetLevel(int level)
        {
            // delete level lane
            var dels =
                from lane0 in Screen.Children
                let lane = lane0 as FrameworkElement
                where lane?.Name.StartsWith("Level_") ?? false
                let lno = int.Parse(lane.Name.Substring(6))
                where lno >= level
                select lane;

            foreach (var lane in dels.ToArray())
            {
                Screen.Children.Remove(lane);
            }
            Screen.Width = 300 * level;

            StackPanel tarlane;
            Screen.Children.Add(tarlane = new StackPanel
            {
                Name = $"Level_{level}",
                Width = 290,
                Background = new SolidColorBrush(Color.FromArgb(40, 0, 255, 0)),
            });
            return tarlane;
        }

        private UIElement AddCassette(Func<UIElement> instanciator)
        {
            var npc = instanciator.Invoke();
            ((INotifyPropertyChanged)npc).PropertyChanged += OnPropertyChanged;

            var lane = SetLevel(1);
            lane.Children.Add(npc);

            return npc;
        }

        private void RemoveCassette(string xname)
        {
            var res = FindCassette(xname);
            if (res != default)
            {
                res.Parent.Children.Remove(res.Cassette);
            }
        }

        private UIElement FocusIfExist(string xname)
        {
            var pp = FindCassette(xname);
            if (pp != default)
            {
                // TODO: 先頭に持ってくる
                // TODO: 次レベルのカセットが xnameでなければ、それらを消す
            }
            return pp.Cassette;
        }

        [EventCatch(TokenID = TOKEN.PROPERTYOPEN)]
        public void PropertyOpen(EventTokenTriggerPropertyOpen token)
        {
            if (token.Target is IJitObjectID oid)
            {
                if (FocusIfExist(oid.ID) != null)
                {
                    return;
                }
            }
            if (token.Target is JitProcess proc)
            {
                AddCassette(() => new PropertyProcess
                {
                    Target = proc,
                });
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

        private void UpdateCassette<TCassette>(EventTokenJitVariableTrigger token, Action<TCassette, JitVariable> updateAction)
        {
            if (token.From is IJitObjectID tar && token.From is JitVariable va)
            {
                if (FindCassette(tar.ID).Cassette is TCassette cassette)
                {
                    ((INotifyPropertyChanged)cassette).PropertyChanged -= OnPropertyChanged;
                    updateAction.Invoke(cassette, va);
                    ((INotifyPropertyChanged)cassette).PropertyChanged += OnPropertyChanged;
                }
            }
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.LocationChanged)]
        public void LocationChanged(EventTokenJitVariableTrigger token)
        {
            UpdateCassette<IPropertyXy>(token, (cassette, tokenFrom) =>
            {
                cassette.X = $"{((Distance)tokenFrom.ChildVriables["LocationX"].Value).m}m";
                cassette.Y = $"{((Distance)tokenFrom.ChildVriables["LocationY"].Value).m}m";
            });
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.SizeChanged)]
        public void SizeChanged(EventTokenJitVariableTrigger token)
        {
            UpdateCassette<IPropertyWh>(token, (cassette, tokenFrom) =>
            {
                cassette.W = $"{((Distance)tokenFrom.ChildVriables["Width"].Value).m}m";
                cassette.H = $"{((Distance)tokenFrom.ChildVriables["Height"].Value).m}m";
            });
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.NameChanged)]
        public void NameChanged(EventTokenJitVariableTrigger token)
        {
            UpdateCassette<IPropertyInstanceName>(token, (cassette, tokenFrom) =>
            {
                cassette.InstanceName = tokenFrom.Name;
            });
        }

        [EventCatch(TokenID = FeatureJitProcess.TOKEN.REMOVE)]
        public void ProcessRemoved(EventTokenProcessPartsTrigger token)
        {
            RemoveCassette(token.Process.ID);
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
