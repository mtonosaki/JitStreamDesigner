using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// コンテンツ ダイアログの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace JitStreamDesigner
{
    public sealed partial class CiPalette : ContentDialog
    {
        public Type SelectedCommand { get; private set; }

        public CiPalette()
        {
            this.InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                SelectedCommand = fe.Tag as Type;   // Expecting to have set Type of CiBase to Button.Tag
            }
            Hide();
        }
    }
}
