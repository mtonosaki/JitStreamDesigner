// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace JitStreamDesigner
{
    public interface ICioSelectedClass
    {
        Type Selected { get; }
    }

    public sealed partial class CiPalette : ContentDialog, ICioSelectedClass
    {
        public Type Selected { get; private set; }

        public CiPalette()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                Selected = fe.Tag as Type;   // Expecting to have set Type of CiBase to Button.Tag
            }
            Hide();
        }
    }
}
