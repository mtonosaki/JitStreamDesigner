// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// コンテンツ ダイアログの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace JitStreamDesigner
{
    public sealed partial class TemplateDialog : ContentDialog
    {
        public TemplateDialog()
        {
            InitializeComponent();
            ErrorName.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = false;
        }

        public string TemplateName
        {
            get => InputTemplateName.Text;
            set => InputTemplateName.Text = value;
        }

        public Color AccentColor
        {
            get => InputTemplateColor.Color;
            set => InputTemplateColor.Color = value;
        }

        public string TemplateRemarks
        {
            get => InputRemarks.Text;
            set => InputRemarks.Text = value;
        }

        private static readonly Regex NameFormat = new Regex("^([A-Z,a-z])+([A-Z,a-z,0-9])*$");

        private void TemplateName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var check = NameFormat.IsMatch(InputTemplateName.Text);
            ErrorName.Visibility = check ? Visibility.Collapsed : Visibility.Visible;
            IsPrimaryButtonEnabled = check;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
