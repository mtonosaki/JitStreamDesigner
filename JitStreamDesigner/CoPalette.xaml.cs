using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
