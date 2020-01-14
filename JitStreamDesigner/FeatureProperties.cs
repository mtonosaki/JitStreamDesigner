// (c) 2020 Manabu Tonosaki
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
                Orientation = Orientation.Vertical,
            });
            if (level > 1)
            {
                Button backButton;
                tarlane.Children.Add(backButton = new Button
                {
                    Content = "← Back",
                    Background = new SolidColorBrush(Colors.Transparent),
                    Tag = level - 1,
                });
                backButton.Click += LaneBackButton_Click;
            }
            return tarlane;
        }

        private void LaneBackButton_Click(object sender, RoutedEventArgs e)
        {
            var alane = Screen.Children.Where(a => ((FrameworkElement)a).Name.StartsWith("Level_")).FirstOrDefault() as FrameworkElement;
            var btn = sender as Button;
            var toLevel = (int)btn.Tag;

            // auto scroll to show back cassette
            ScrollView.HorizontalScrollMode = ScrollMode.Enabled;
            ScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            ScrollView.ChangeView(alane.Width * (toLevel - 1), null, null, false);
        }

        private (StackPanel Parent, UIElement Cassette) AddCassette(Func<UIElement> instanciator)
        {
            var npc = instanciator.Invoke();
            if (npc is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += OnPropertyChanged;
            }
            if (npc is IEventPropertySpecificUndoRedo sur)
            {
                sur.NewUndoRedo += OnNewUndoRedo;
            }
            if (npc is IEventPropertyCioOpen pco)
            {
                pco.CioClicked += OnCioClicked;
            }

            var lane = SetLevel(1); // remove the all lanes and add a first lane
            lane.Children.Add(npc); // add a propery cassette

            return (lane, npc);
        }

        /// <summary>
        /// Open Ci/Co property casette
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCioClicked(object sender, CioClickedEventArgs e)
        {
            var res = FindCassette(e.TargetProcess.ID);
            var level = int.Parse(StrUtil.MidSkip(res.Parent.Name, "Level_"));
            var lane = SetLevel(level + 1);
            var cassetteType = GetType().Assembly.GetTypes().Where(a => a.Name == $"Property{e.Cio.GetType().Name}").FirstOrDefault();
            var cioCassette = Activator.CreateInstance(cassetteType) as UIElement;
            lane.Children.Add(cioCassette);
            if (cioCassette is ISetPropertyTarget sp)
            {
                sp.SetPropertyTarget(e.Cio);
            }
            DelayUtil.Start(TimeSpan.FromMilliseconds(20), () =>
            {
                // auto scroll to show new cassette
                ScrollView.HorizontalScrollMode = ScrollMode.Enabled;
                ScrollView.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                ScrollView.ChangeView(lane.Width * level, null, null, false);
            });
        }

        private void RemoveCassette(string xname)
        {
            var res = FindCassette(xname);
            if (res != default)
            {
                res.Parent.Children.Remove(res.Cassette);
            }
        }

        private UIElement AddOrFocus(string xname, Func<UIElement> instanciator)
        {
            var pp = FindCassette(xname);
            if (pp != default)
            {
                // TODO: 先頭に持ってくる
                // TODO: 次レベルのカセットが xnameでなければ、それらを消す
            }
            else
            {
                pp = AddCassette(instanciator);
            }
            return pp.Cassette;
        }

        [EventCatch(TokenID = TOKEN.PROPERTYOPEN)]
        public void PropertyOpen(EventTokenTriggerPropertyOpen token)
        {
            if (token.Target is JitProcess proc)
            {
                AddOrFocus(proc.ID, () => new PropertyProcess
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
            redosaver.Stop();
            redosaver.Start();

            // GENERAL PROPERTY
            if (sender is IPropertyInstanceName name && e.PropertyName == "InstanceName")
            {
                jacRedo.AppendLine($@"{name.ID}.Name = '{name.InstanceName}'");
                jacRedo.AppendLine($@"Gui.UpdateName = {name.ID}");
                jacUndo.AppendLine($@"{name.ID}.Name = '{name.PreviousValue[e.PropertyName]}'");
                jacUndo.AppendLine($@"Gui.UpdateName = {name.ID}");
                return;
            }
            if (sender is IPropertyXy xy)
            {
                switch (e.PropertyName)
                {
                    case "X":
                        jacRedo.AppendLine($@"{xy.ID}.LocationX = {xy.X}");
                        jacRedo.AppendLine($@"Gui.UpdateLocation = {xy.ID}");
                        jacUndo.AppendLine($@"{xy.ID}.LocationX = {xy.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateLocation = {xy.ID}");
                        return;
                    case "Y":
                        jacRedo.AppendLine($@"{xy.ID}.LocationY = {xy.Y}");
                        jacRedo.AppendLine($@"Gui.UpdateLocation = {xy.ID}");
                        jacUndo.AppendLine($@"{xy.ID}.LocationY = {xy.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateLocation = {xy.ID}");
                        return;
                }
            }
            if (sender is IPropertyWh wh)
            {
                switch (e.PropertyName)
                {
                    case "W":
                        jacRedo.AppendLine($@"{wh.ID}.Width = {wh.W}");
                        jacRedo.AppendLine($@"Gui.UpdateSize = {wh.ID}");
                        jacUndo.AppendLine($@"{wh.ID}.Width = {wh.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateSize = {wh.ID}");
                        return;
                    case "H":
                        jacRedo.AppendLine($@"{wh.ID}.Height = {wh.H}");
                        jacRedo.AppendLine($@"Gui.UpdateSize = {wh.ID}");
                        jacUndo.AppendLine($@"{wh.ID}.Height = {wh.PreviousValue[e.PropertyName]}");
                        jacUndo.AppendLine($@"Gui.UpdateSize = {wh.ID}");
                        return;
                }
            }
        }

        /// <summary>
        /// treat original property change to support UNDO/REDO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNewUndoRedo(object sender, NewUndoRedoEventArgs e)
        {
            jacRedo.AppendLine(e.NewRedo);
            jacUndo.AppendLine(e.NewUndo);

            redosaver.Stop();
            redosaver.Start();
        }

        [EventCatch(TokenID = FeatureGuiJacBroker.TOKEN.CioChanged)]
        public void CioChanged(EventTokenJitCioTrigger token)
        {
            var pp = AddOrFocus(token.TargetProcessID, () => new PropertyProcess
            {
                Target = Hot.ActiveTemplate.Jac.GetProcess(token.TargetProcessID),
            }) as PropertyProcess;

            pp.UpdateCioButton(token.Action, token.FromCioID);  // Add/Remove Ci/Co button in Process Cassette
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
}
