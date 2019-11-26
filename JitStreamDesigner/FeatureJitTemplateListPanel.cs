using Microsoft.Graphics.Canvas.Text;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Tono.Jit;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using static Tono.Gui.Uwp.CastUtil;

namespace JitStreamDesigner
{
    [FeatureDescription(En = "Template Control Panel", Jp = "テンプレート一覧パネル")]
    public class FeatureJitTemplateListPanel : FeatureSimulatorBase
    {
        private PartsActiveTemplate BarParts = null;

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
                Kill(new NullReferenceException("FeatureJitTemplateListPanel must have FeatureJitTemplateListPanel={Bind:****}"));
                return;
            }

            TargetListView.ItemsSource = Hot.TemplateList;
            TargetListView.SelectionChanged += TargetListView_SelectionChanged;

            // Add default template chip
            TemplateTipModel ttm;
            const string defaultName = "@Default";
            Hot.TemplateList.Add(ttm = new TemplateTipModel
            {
                Template = new JitTemplate
                {
                    Name = defaultName,
                },
                AccentColor = Colors.Yellow,
                Remarks = "Free purpose template",
            });
            ttm.Template.ChildVriables["AccentColor"] = JitVariable.From("#ffffff00", classNames: ":RGB32");
            var jac = $@"
                TheStage = new Stage  //-ForTemplateOnly-//
            ";
            ttm.Template.AddBlock(jac);
            DelayUtil.Start(TimeSpan.FromMilliseconds(200), () =>
            {
                TargetListView.SelectedIndex = 0;
            });

            // Draw preparation
            Pane.Target = Pane["LogPanel"]; // to get priority draw layer
            BarParts = new PartsActiveTemplate
            {
            };
            Parts.Add(Pane.Target, BarParts, LAYER.ActiveTemplate);
        }

        /// <summary>
        /// List selection change UWP Control event handling
        /// To change UWP event to token trigger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TargetListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tar = e.AddedItems.LastOrDefault() as TemplateTipModel;
            if (tar != Hot.ActiveTemplate)
            {
                Token.AddNew(new EventTokenTemplateChangedTrigger
                {
                    TargetTemplate = tar,
                    TokenID = TOKEN.TemplateSelectionChanged,
                    Sender = this,
                    Remarks = "Select changed List View to change active template",
                });
            }
        }

        [EventCatch(TokenID = TOKEN.TemplateSelectionChanged)]
        public void TemplateSelectionChanged(EventTokenTemplateChangedTrigger token)
        {
            Hot.ActiveTemplate = token.TargetTemplate;
            BarParts.Text = Hot.ActiveTemplate?.TemplateID;
            BarParts.BackgroundColor = Hot.ActiveTemplate.AccentColor;
            BarParts.Remarks = Hot.ActiveTemplate?.Remarks;
            Redraw();
        }

        [EventCatch]
        public async Task AddTemplate(EventTokenButton token)
        {
            try
            {
                var td = new TemplateDialog
                {
                    AccentColor = (new ColorUtil.HSV((float)MathUtil.Rand() * 360, 1.0f, 1.0f)).ToColor(),
                };
                Hot.KeybordShortcutDisabledFlags["AddTemplate"] = true;
                var res = await td.ShowAsync();
                if (res == ContentDialogResult.Primary)
                {
                    Hot.TemplateList.Add(new TemplateTipModel
                    {
                        Template = new JitTemplate
                        {
                            Name = td.TemplateName,
                        },
                        AccentColor = td.AccentColor,
                        Remarks = td.TemplateRemarks,
                    });
                    LOG.AddMes(LLV.INF, new LogAccessor.Image { Key="Lump16" }, "FeatureJitTemplatePanel-CannotUndo");
                }
            }
            finally
            {
                Hot.KeybordShortcutDisabledFlags["AddTemplate"] = false;
            }
        }
    }

    /// <summary>
    /// Token type for template selection change
    /// </summary>
    public class EventTokenTemplateChangedTrigger : EventTokenTrigger
    {
        public TemplateTipModel TargetTemplate { get; set; }
    }

    /// <summary>
    /// Active Template name and Yellow Bar
    /// </summary>
    public class PartsActiveTemplate : PartsBase<ScreenX, ScreenY>
    {
        public string Text { get; set; }
        public Color BackgroundColor { get; set; }
        public string Remarks { get; set; }

        public override void Draw(DrawProperty dp)
        {
            if (string.IsNullOrEmpty(Text))
            {
                return;
            }
            // Yellow bar
            var r = dp.PaneRect.Clone();
            r.LT = ScreenPos.From(r.L, 0);
            r.RB = ScreenPos.From(r.R, 2);
            dp.Graphics.FillRectangle(_(r), BackgroundColor);

            // Active Template name (Back ground)
            var tf = new CanvasTextFormat
            {
                FontFamily = "Coureir New",
                FontSize = 11.0f,
                FontWeight = FontWeights.Normal,
                WordWrapping = CanvasWordWrapping.NoWrap,
            };
            r.RB = r.LT + GraphicUtil.MeasureString(dp.Canvas, Text, tf) + ScreenSize.From(10, 10);
            dp.Graphics.FillRectangle(_(r), BackgroundColor);

            // Active Template name (Text)
            dp.Graphics.TextAntialiasing = CanvasTextAntialiasing.ClearType;
            dp.Graphics.DrawText(Text, 4, 2, ColorUtil.GetNegativeColor(BackgroundColor), tf);
        }
    }
}
