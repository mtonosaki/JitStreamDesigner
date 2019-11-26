using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class TemplateDialog : ContentDialog
    {
        public TemplateDialog()
        {
            this.InitializeComponent();
            ErrorName.Visibility = Visibility.Collapsed;
            IsPrimaryButtonEnabled = false;
        }

        public string TemplateName
        {
            get
            {
                return InputTemplateName.Text;
            }
            set
            {
                InputTemplateName.Text = value;
            }
        }

        public Color AccentColor
        {
            get
            {
                return InputTemplateColor.Color;
            }
            set
            {
                InputTemplateColor.Color = value;
            }
        }

        public string TemplateRemarks
        {
            get
            {
                return InputRemarks.Text;
            }
            set
            {
                InputRemarks.Text = value;
            }
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
