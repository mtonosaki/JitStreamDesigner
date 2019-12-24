using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    Casettes.Children.Insert(0, ue);
                }
                else
                {
                    Casettes.Children.Insert(0, new PropertyProcess
                    {
                        Name = proc.ID,
                        ProcessID = proc.ID,
                        ProcessName = proc.Name,
                        X = (Distance)proc.ChildVriables["LocationX"].Value,
                        Y = (Distance)proc.ChildVriables["LocationY"].Value,
                        W = (Distance)proc.ChildVriables["Width"].Value,
                        H = (Distance)proc.ChildVriables["Height"].Value,

                        HorizontalAlignment = HorizontalAlignment.Stretch,
                    });
                }
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
