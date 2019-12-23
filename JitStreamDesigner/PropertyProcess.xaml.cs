using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tono;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// ユーザー コントロールの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234236 を参照してください

namespace JitStreamDesigner
{
    public sealed partial class PropertyProcess : UserControl
    {
        public PropertyProcess()
        {
            this.InitializeComponent();
        }
        public string ProcessID { get => InstanceID.Text; set => InstanceID.Text = value; }
        public string ProcessName { get => InstanceName.Text; set => InstanceName.Text = value; }
        public Distance X { get => Distance.Parse(InstanceX.Text); set => InstanceX.Text = $"{value.m}m"; }
        public Distance Y { get => Distance.Parse(InstanceY.Text); set => InstanceY.Text = $"{value.m}m"; }
        public Distance W { get => Distance.Parse(InstanceW.Text); set => InstanceW.Text = $"{value.m}m"; }
        public Distance H { get => Distance.Parse(InstanceH.Text); set => InstanceH.Text = $"{value.m}m"; }

        private void Button_Round_Click(object sender, RoutedEventArgs e)
        {
            X = Distance.FromMeter(Math.Round(X.m));
            Y = Distance.FromMeter(Math.Round(Y.m));
            W = Distance.FromMeter(Math.Round(W.m));
            H = Distance.FromMeter(Math.Round(H.m));
        }
    }
}
