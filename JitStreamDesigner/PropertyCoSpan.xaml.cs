// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Tono.Jit;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace JitStreamDesigner
{
    public sealed partial class PropertyCoSpan : UserControl, INotifyPropertyChanged, ISetPropertyTarget
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public PropertyCoSpan()
        {
            this.InitializeComponent();
        }

        public void SetPropertyTarget(object target)
        {
            if (target is CoSpan co)
            {
                Target = co;
            }
        }

        private CoSpan target;
        /// <summary>
        /// Target Jit Object
        /// </summary>
        public CoSpan Target
        {
            get => target;
            set
            {
                target = value;
                Span = JacInterpreter.MakeTimeSpanString(target.Span);
                PorlingSpan = JacInterpreter.MakeTimeSpanString(target.PorlingSpan);
            }
        }

        public string ID
        {
            get => Target?.ID ?? "(n/a)";
            set => new NotSupportedException();
        }

        public Dictionary<string, object> PreviousValue { get; } = new Dictionary<string, object>();

        private string span = "0S";
        public string Span
        {
            get => span;
            set
            {
                //if (Distance.Parse(value) != Distance.Parse(x))
                {
                    PreviousValue["Span"] = span;
                    span = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Span"));
                }
            }
        }

        private string porlingspan = "0S";
        public string PorlingSpan
        {
            get => porlingspan;
            set
            {
                //if (Distance.Parse(value) != Distance.Parse(x))
                {
                    PreviousValue["PorlingSpan"] = span;
                    porlingspan = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PorlingSpan"));
                }
            }
        }
    }
}
