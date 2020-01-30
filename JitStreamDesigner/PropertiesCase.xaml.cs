// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using Windows.UI.Xaml.Controls;

namespace JitStreamDesigner
{
    public sealed partial class PropertyCase : UserControl
    {
        public PropertyCase()
        {
            InitializeComponent();
            CleanDesignDummy();
        }

        private void CleanDesignDummy()
        {
            Screen.Children.Clear();
        }
    }
}
