using Microsoft.Graphics.Canvas.Text;
using System;
using System.Linq;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    [FeatureDescription(En = "Class Control Panel", Jp = "クラス一覧パネル")]
    public class FeatureClassListPanel : FeatureSimulatorBase
    {
        /// <summary>
        /// active Class for edit
        /// </summary>
        public ClassTipModel CurrentClass { get; set; }

        private PartsActiveClass BarParts = null;

        /// <summary>
        /// target list view UWP control
        /// </summary>
        public ListView TargetListView { get; set; }

        /// <summary>
        /// initialize feature
        /// </summary>
        public override void OnInitialInstance()
        {
            // UWP control preparation
            if (TargetListView == null)
            {
                Kill(new NullReferenceException("FeatureClassListPanel must have FeatureClassListPanel={Bind:****}"));
                return;
            }
            TargetListView.SelectionChanged += TargetListView_SelectionChanged;

            // Draw preparation
            Pane.Target = Pane.Main;
            BarParts = new PartsActiveClass
            {
            };
            Parts.Add(Pane.Target, BarParts, LAYER.ActiveClass);
        }

        /// <summary>
        /// List selection change UWP Control event handling
        /// To change UWP event to token trigger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tar = e.AddedItems.LastOrDefault() as ClassTipModel;
            if (tar != CurrentClass)
            {
                Token.AddNew(new EventTokenClassChangedTrigger
                {
                    TargetClass = tar,
                    TokenID = TOKEN.ClassSelectionChanged,
                    Sender = this,
                    Remarks = "Select changed List View to change active class",
                });
            }
        }

        [EventCatch(TokenID = TOKEN.ClassSelectionChanged)]
        public void ClassSelectionChanged(EventTokenClassChangedTrigger token)
        {
            CurrentClass = token.TargetClass;
            BarParts.Text = CurrentClass?.ClassID;
            Redraw();
        }
    }

    /// <summary>
    /// Token type for class selection change
    /// </summary>
    public class EventTokenClassChangedTrigger : EventTokenTrigger
    {
        public ClassTipModel TargetClass { get; set; }
    }

    public class DOBridge : DependencyObject
    {
        public new object GetValue(DependencyProperty dp)
        {
            return base.GetValue(dp);
        }
        public new void SetValue(DependencyProperty dp, object value)
        {
            base.SetValue(dp, value);
        }
    }

    /// <summary>
    /// Active Class name and Yellow Bar
    /// </summary>
    public class PartsActiveClass : PartsBase<ScreenX, ScreenY>
    {
        public string Text { get; set; }
        public DOBridge dob = new DOBridge();

        public static readonly DependencyProperty DummyProperty = DependencyProperty.Register("Dummy", typeof(double), typeof(PartsActiveClass), null);

        public double Dummy
        {
            get
            {
                return (double)dob.GetValue(DummyProperty);
            }
            set
            {
                dob.SetValue(DummyProperty, value);
            }
        }

        public override void Draw(DrawProperty dp)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }
            // Yellow bar
            var r = dp.PaneRect.Clone();
            r.RB = ScreenPos.From(r.R, 2) + ScreenX.From(Dummy);
            dp.Graphics.FillRectangle(_(r), Colors.Yellow);

            // Active Class name (Back ground)
            var tf = new CanvasTextFormat
            {
                FontFamily = "Coureir New",
                FontSize = 9.0f,
                FontWeight = FontWeights.Bold,
                WordWrapping = CanvasWordWrapping.NoWrap,
            };
            r.RB = r.LT + GraphicUtil.MeasureString(dp.Canvas, Text, tf) + ScreenSize.From(10, 8);
            dp.Graphics.FillRectangle(_(r), Colors.Yellow);

            // Active Class name (Text)
            dp.Graphics.TextAntialiasing = CanvasTextAntialiasing.ClearType;
            dp.Graphics.DrawText(Text, 4, 2, Color.FromArgb(0xff, 0x55, 0x77, 0xaa), tf);
        }
    }
}
