// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace JitStreamDesigner
{
    public class XamlTimeSpanToStringConverter : IValueConverter
    {
        private bool IsDoubleEqual(double v1, double v2)
        {
            return v1 - v2 < 0.001;
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan ts)
            {
                if (ts.TotalMilliseconds < 1000)
                {
                    return $"{ts.TotalMilliseconds}MS";
                }
                if (ts.TotalMinutes < 60)
                {
                    var sts = new[] { 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 0.6, 0.7, 0.75, 0.8, 0.9 };
                    var sttar = ts.TotalMinutes % 1;
                    foreach (var st in sts)
                    {
                        if( IsDoubleEqual(sttar, st))
                        {
                            return $"{ts.TotalMinutes}M";
                        }
                    }
                    return $"{ts.TotalSeconds}S";
                }
                if (ts.TotalHours < 10.5)
                {
                    var sts = new[] { 0.1, 0.2, 0.25, 0.3, 0.4, 0.5, 0.6, 0.7, 0.75, 0.8, 0.9 };
                    var sttar = ts.TotalHours % 1;
                    foreach (var st in sts)
                    {
                        if (IsDoubleEqual(sttar, st))
                        {
                            return $"{ts.TotalHours}H";
                        }
                    }
                    return $"{ts.TotalMinutes}M";
                }
                if (ts.TotalDays < 2)
                {
                    return $"{ts.TotalHours}H";
                }
                return $"{ts.TotalDays}D";
            }
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
