// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JitStreamDesigner
{
    public sealed partial class CoPalette : ContentDialog, ICioSelectedClass
    {
        public Type Selected { get; private set; }

        public CoPalette()
        {
            this.InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                Selected = fe.Tag as Type;   // Expecting to have set Type of CoBase to Button.Tag
            }
            Hide();
        }
    }
}
