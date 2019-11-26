using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace JitStreamDesigner
{
    /// <summary>
    /// Simulator Clock Controller
    /// </summary>
    [FeatureDescription(En = "Simulator Clock", Jp = "シミュレータークロック")]
    public partial class FeatureClock : FeatureSimulatorBase, IKeyListener
    {
        public const string ClockSwitch = "ClockSwitch";
        public const string Clock_Running = "Clock_Running";
        public const string Clock_Stopping = "Clock_Stopping";

        #region KEY
        private static KeyListenSetting _keyStartStop;
        public IEnumerable<KeyListenSetting> KeyListenSettings => _keys;
        private static readonly KeyListenSetting[] _keys = new KeyListenSetting[]
        {
            _keyStartStop = new KeyListenSetting // [0]
			{
                Name = "Clock Start/Stop",
                KeyStates = new[]
                {
                    (VirtualKey.F, KeyListenSetting.States.Down),
                },
            },

        };
        #endregion

        private TButton buttonStart;
        private FrameworkElement buttonStartText;
        private ProgressBar runningBar;
        private PartsClock ClockParts = null;
        private DispatcherTimer _timer = null;

        public override void OnInitialInstance()
        {
            Now = DateTime.Now;

            // to prepare clock control status
            Status[ClockSwitch].AddValues(new[]
            {
                Clock_Stopping,
                Clock_Running,
            });
            Status[ClockSwitch].Value = Clock_Stopping; // initial status



            // for Running Bar ON/OFF
            runningBar = (ProgressBar)ControlUtil.FindControl(View, "ClockRunning");

            // for Start Button Control
            buttonStart = (TButton)ControlUtil.FindControl(View, "ClockStart");
            buttonStartText = ControlUtil.FindControl(View, "ClockStartCaption");

            // Make digital clock parts
            Pane.Target = Pane["LogPanel"];
            ClockParts = new PartsClock
            {
                Parent = this,
                Seg7 = new NumberDisplay { View = View, },
            };
            Parts.Add(Pane.Target, ClockParts, LAYER.Clock);
            ClockParts.Seg7.Loaded += (s, e) => Redraw();   // redraw when loaded images completely.
            var _ = ClockParts.Seg7.Load7Seg();

            BlinkProc();
        }

        private void BlinkProc()
        {
            var blinkTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500),
            };
            blinkTimer.Tick += (s, e) =>
            {
                if (_timer == null)
                {
                    buttonStartText.Visibility = Visibility.Visible;
                }
                else
                {
                    if (buttonStartText.Visibility == Visibility.Visible)
                    {
                        buttonStartText.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        buttonStartText.Visibility = Visibility.Visible;
                    }
                }
            };
            blinkTimer.Start();
        }

        [EventCatch(TokenID = TOKEN.ReadCompleted)]
        public void ReadCompleted(EventTokenTrigger _)
        {
            DelayUtil.Start(TimeSpan.FromMilliseconds(MathUtil.Rand(990, 1000)), () =>
            {
                Parts.ClearDoubleBuffer(LAYER.DoubleBufferMap, true);  // clock redraw after open study
                Redraw();
            });
        }

        [EventCatch(TokenID = TOKEN.ClockStart, Status = (ClockSwitch + "=" + Clock_Stopping))]
        public void ClockStart(EventTokenTrigger _)
        {
            Status[ClockSwitch].Value = Clock_Running;

            runningBar.Visibility = Visibility.Visible;
            buttonStart.Background = new LinearGradientBrush(new GradientStopCollection
            {
                new GradientStop{ Color = Color.FromArgb(255, 0,  0,  64), Offset = 0.0, },
                new GradientStop{ Color = Color.FromArgb(255, 0, 48, 160), Offset = 0.2, },
                new GradientStop{ Color = Color.FromArgb(255, 0,  0, 128), Offset = 0.8, },
                new GradientStop{ Color = Color.FromArgb(255, 0, 48, 160), Offset = 1.0, },
            }, 90);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10),
            };
            _timer.Tick += OnTick;
            _timer.Start();
            LOG.WriteLine(LLV.INF, $"○ Clock Start : {DateTime.Now.ToString(TimeUtil.FormatYMDHMS)}");
        }

        [EventCatch(TokenID = TOKEN.ClockStop, Status = (ClockSwitch + "=" + Clock_Running))]
        public void ClockStop(EventTokenTrigger _)
        {
            _timer.Stop();
            _timer = null;
            Status[ClockSwitch].Value = Clock_Stopping;
            runningBar.Visibility = Visibility.Collapsed;
            buttonStart.Background = new SolidColorBrush(Colors.Black);
            LOG.WriteLine(LLV.INF, $"● Clock Stop : {Now.ToString(TimeUtil.FormatYMDHMS)}");
        }

        /// <summary>
        /// clock start button token trigger
        /// </summary>
        /// <param name="tb"></param>
        [EventCatch(Name = "ClockStart")]
        public void ClockStartButton(EventTokenButton tb)
        {
            switch (Status[ClockSwitch].Value)
            {
                case Clock_Stopping:
                    Token.Link(tb, new EventTokenTrigger
                    {
                        TokenID = TOKEN.ClockStart,
                        Sender = this,
                        Remarks = tb.Remarks,
                    });
                    break;
                case Clock_Running:
                    Token.Link(tb, new EventTokenTrigger
                    {
                        TokenID = TOKEN.ClockStop,
                        Sender = this,
                        Remarks = tb.Remarks,
                    });
                    break;
            }
        }

        /// <summary>
        /// Keyboard event catch
        /// </summary>
        /// <param name="kt"></param>
        public void OnKey(KeyEventToken kt)
        {
            if( Hot.KeybordShortcutDisabledFlags.Values.Where(a => a).Count() > 0)  // Check disabled flag
            {
                return;
            }
            kt.Select(_keyStartStop, Status[ClockSwitch].IsOn(Clock_Running), ks =>
            {
                Token.Link(kt, new EventTokenTrigger
                {
                    TokenID = TOKEN.ClockStop,
                    Sender = this,
                    Remarks = "stop clock by key"
                });
            });
            kt.Select(_keyStartStop, Status[ClockSwitch].IsOn(Clock_Stopping), ks =>
            {
                Token.Link(kt, new EventTokenTrigger
                {
                    TokenID = TOKEN.ClockStart,
                    Sender = this,
                    Remarks = "start clock by key"
                });
            });
        }

        /// <summary>
        /// Timer porling
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnTick(object sender, object args)
        {
            if (_timer == null)    // Stop後にイベントが来た場合のポカヨケ
            {
                return;
            }
            _timer.Stop();

            UpdateSimulationTime(EventTokenDispatchTimerWrapper.From((DispatcherTimer)sender, this, "onTick"), this, Now + Hot.ClockTick);
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Start();
        }

        /// <summary>
        /// Update clock forcely
        /// </summary>
        /// <param name="from"></param>
        /// <param name="newtime"></param>
        /// <param name="remarks"></param>
        public static void UpdateSimulationTime(EventToken token, FeatureSimulatorBase from, DateTime newtime, string remarks = null)
        {
            var pre = from.Now;
            from.Now = newtime;
            from.Token.Link(token, new EventClockUpdatedTokenTrigger
            {
                TokenID = TOKEN.ClockUpdated,
                Pre = pre,
                Now = newtime,
                Sender = from,
                Remarks = remarks ?? "UpdateSimulationTime",
            });
            from.Redraw();
        }
    }
}
