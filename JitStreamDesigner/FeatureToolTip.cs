// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Microsoft.Graphics.Canvas.Text;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using static Tono.Gui.Uwp.CastUtil;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// provide tool tip text for hovering parts
    /// </summary>
    /// <example>
    /// XAML
    /// ＜tg:FeatureToolTip
    ///     TargetPane = "{x:Bind ToolBox}" TargetLayer="{x:Bind local:LAYER.ToolButtons}"
    ///     TooltipPane="{x:Bind ToolBox}" TooltipLayer="{x:Bind tg:FeatureToolTip.DefaultLayer}">
    ///     ＜tg:FeatureToolTip.PartsTooltip>
    ///         ＜tg:PartsTooltip />
    ///     ＜/tg:FeatureToolTip.PartsTooltip>
    /// ＜/tg:FeatureToolTip>
    /// 
    /// PARTS
    /// public class PartsToolButton : PartsBase＜ScreenX, ScreenY>, ITooltipResponse
    /// {
    /// }
    /// </example>
    public class FeatureToolTip : FeatureBase, IPointerListener
    {
        /// <summary>
        /// Event Token ID
        /// </summary>
        public const string TokenIDShow = "TokenFeatureToolTipShow";
        public const string TokenIDHide = "TokenFeatureToolTipHide";
        public static readonly NamedId DefaultLayer = NamedId.From("TooltipPanel", 7990);

        /// <summary>
        /// Target pane to find button or parts that want to show tooltip
        /// </summary>
        public IDrawArea TargetPane { get; set; }

        /// <summary>
        /// Target layer to find parts that want to show tooltip
        /// </summary>
        public NamedId TargetLayer { get; set; }

        /// <summary>
        /// Pane to put tooltip visual parts
        /// </summary>
        public IDrawArea TooltipPane { get; set; }

        /// <summary>
        /// Layer of tooltip visual parts
        /// </summary>
        public NamedId TooltipLayer { get; set; }

        /// <summary>
        /// tooltip design
        /// </summary>
        public PartsTooltip PartsTooltip { get; set; }

        public override void OnInitialInstance()
        {
            base.OnInitialInstance();

            if (TooltipPane == null)
            {
                TooltipPane = Pane.Main;
            }
            Pane.Target = TooltipPane;
            if (TooltipLayer == null)
            {
                TooltipLayer = DefaultLayer;
            }
            if (PartsTooltip == null)
            {
                PartsTooltip = new PartsTooltip();
            }

            Parts.Add(TooltipPane, PartsTooltip, TooltipLayer);
        }

        /// <summary>
        /// Show Tooltip by Token
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = TokenIDShow)]
        public virtual void DrawTooltip(EventTokenTriggerTooltip token)
        {
            PartsTooltip.Text = token.Text;
            PartsTooltip.IsUpperPositionDefault = token.IsUpperPositionDefault;
            PartsTooltip.Location = CodePos<ScreenX, ScreenY>.From(token.Position.X, token.Position.Y);
            Redraw();
        }

        /// <summary>
        /// Hide Tooltip by token
        /// </summary>
        /// <param name="token"></param>
        [EventCatch(TokenID = TokenIDHide)]
        public virtual void HideTooltip(EventTokenTriggerTooltip token)
        {
            PartsTooltip.Text = string.Empty;
            Redraw();
        }

        private string CurrentText;
        private ITooltipResponse CurrentParts;

        private void findPartsForTooltip(PointerState po)
        {
            var parts = Parts.GetParts(TargetLayer, p => p is ITooltipResponse ttr && ttr.SelectingScore(TargetPane, po.Position) <= 1.0f);
            if (parts.FirstOrDefault() is ITooltipResponse tr)
            {
                var text = Mes.Get(tr.ToolTipUid);
                if (tr.Equals(CurrentParts) == false && CurrentText != text)
                {
                    Token.AddNew(new EventTokenTriggerTooltip
                    {
                        TokenID = TokenIDShow,
                        Text = text,
                        IsUpperPositionDefault = po.DeviceType == PointerState.DeviceTypes.Touch || po.DeviceType == PointerState.DeviceTypes.Pen,
                        Position = po.Position,
                        Sender = this,
                        Remarks = "to show tooltip",
                    });
                    CurrentText = text;
                    CurrentParts = tr;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(CurrentText) == false && string.IsNullOrWhiteSpace(CurrentText) == false)
                {
                    Token.AddNew(new EventTokenTriggerTooltip
                    {
                        TokenID = TokenIDHide,
                        Sender = this,
                        Remarks = "to hide tooltip",
                    });
                    CurrentText = string.Empty;
                    CurrentParts = null;
                }
            }
        }

        public void OnPointerMoved(PointerState po)
        {
            // TODO: IsKeyNoneのバグフィックス待ち
            if ( /*po.IsKeyNone &&*/ po.IsInContact == false)
            {
                PartsTooltip.Location = CodePos<ScreenX, ScreenY>.From(po.Position.X, po.Position.Y);
                Redraw();
                findPartsForTooltip(po);
            }
        }

        public void OnPointerPressed(PointerState po)
        {
            findPartsForTooltip(po);
        }

        public void OnPointerHold(PointerState po)
        {
        }

        public void OnPointerReleased(PointerState po)
        {
        }
    }

    /// <summary>
    /// Tooltip response interface of PartsBase
    /// </summary>
    public interface ITooltipResponse
    {
        /// <summary>
        /// calculate selecting position score
        /// </summary>
        /// <param name="pane">target pane</param>
        /// <param name="pos">pointer position</param>
        /// <returns>0=exactly same position, 0.99999=Selectable limit far position,  1～Out of selection position</returns>
        float SelectingScore(IDrawArea pane, ScreenPos pos);

        /// <summary>
        /// Tooltip message key of Resources.Strings
        /// </summary>
        string ToolTipUid { get; }
    }

    public class EventTokenTriggerTooltip : EventTokenTrigger
    {
        public ScreenPos Position { get; set; }
        public string Text { get; set; }
        public bool IsUpperPositionDefault { get; set; }
    }

    public class PartsTooltip : PartsBase<ScreenX, ScreenY>
    {
        public string Text { get; set; }

        /// <summary>
        /// true = upper position for touch HMI
        /// </summary>
        public bool IsUpperPositionDefault { get; set; }

        public override void Draw(DrawProperty dp)
        {
            if (string.IsNullOrEmpty(Text) || string.IsNullOrWhiteSpace(Text)) return;

            var tf = new CanvasTextFormat
            {
                FontFamily = "Segoe UI",
                FontSize = 11.0f,
                FontStyle = FontStyle.Normal,
                FontStretch = FontStretch.Normal,
                FontWeight = FontWeights.Normal,
                WordWrapping = CanvasWordWrapping.NoWrap,
                Direction = CanvasTextDirection.LeftToRightThenTopToBottom,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                LineSpacing = 2.0f,
                OpticalAlignment = CanvasOpticalAlignment.Default,
                Options = CanvasDrawTextOptions.Default,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                VerticalGlyphOrientation = CanvasVerticalGlyphOrientation.Default,
            };
            var ssz0 = GraphicUtil.MeasureString(dp.Canvas, Text, tf);
            // TODO: Cloneサポート待ち
            //var ssz = ssz0.Clone();
            var ssz = new ScreenSize
            {
                Width = ssz0.Width,
                Height = ssz0.Height,
            };
            var sp = ScreenPos.From(Location.X.Cx, Location.Y.Cy);

            if (IsUpperPositionDefault)
            {
                sp += ScreenX.From(8);
                sp -= ScreenY.From(32);
            }
            else
            {
                sp -= ssz.Width;            // adjust tooltip position
                sp += ScreenY.From(24);
            }
            if (sp.X < 0) sp.X = ScreenX.From(0);

            var sr = ScreenRect.From(sp, ssz + ScreenSize.From(12, 12));
            sp += ScreenPos.From(6, 4); // padding
            dp.Graphics.FillRectangle(_(sr), Color.FromArgb(0xee, 0xdd, 0xdd, 0xdd));
            dp.Graphics.DrawRectangle(_(sr), Color.FromArgb(0xff, 0x88, 0x88, 0x88));
            dp.Graphics.DrawText(Text, sp + ssz0.Height, Color.FromArgb(0xff, 0x00, 0x00, 0x00), tf);
        }
    }
}
