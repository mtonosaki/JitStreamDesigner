// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Tono;
using Tono.Jit;
using Windows.UI;

namespace JitStreamDesigner
{
    /// <summary>
    /// Class Selection Tip
    /// </summary>
    public class TemplateTipModel
    {
        /// <summary>
        /// Name (Template ID)
        /// </summary>
        [DataMember]
        public string ID
        {
            get => Template?.ID ?? "(n/a)";
            set => Template.ID = value;
        }

        [DataMember]
        public string Name 
        {
            get => Template?.Name ?? "(no template)" ?? "(no name)";
            set => Template.Name = value;
        }

        /// <summary>
        /// Just-in-time model as a code
        /// </summary>
        [IgnoreDataMember]
        public JacInterpreter Jac { get; set; }

        /// <summary>
        /// Jit model stage
        /// </summary>
        [IgnoreDataMember]
        public JitTemplate Template { get; set; } = new JitTemplate();



        private static readonly Regex hexpattern = new Regex("^[0-9,a-f,A-F]+$");
        /// <summary>
        /// TODO: Waiting NuGet Tono.Gui.Uwp
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Color ColorUtilFrom(string str)
        {
            if (str.StartsWith("#"))
            {
                str = StrUtil.Mid(str, 1);
            }

            if (str.StartsWith("0x"))
            {
                str = StrUtil.Mid(str, 2);
            }

            if (hexpattern.IsMatch(str))
            {
                var val = uint.Parse(str, System.Globalization.NumberStyles.HexNumber);
                return Color.FromArgb((byte)((val & 0xff000000) / 0x1000000), (byte)((val & 0x00ff0000) / 0x10000), (byte)((val & 0x0000ff00) / 0x100), (byte)(val & 0x000000ff));
            }
            else
            {
                var pi = typeof(Colors).GetProperty(str);
                var ret = pi.GetValue(null);
                if (ret is Color col)
                {
                    return col;
                }
                else
                {
                    throw new ArgumentException("ColorUtil.From support #ff112233 or 0xff112233 or Blue style only");
                }
            }
        }

        /// <summary>
        /// Accent color for template sign background
        /// </summary>
        public Color AccentColor
        {
            get => ColorUtilFrom(Template?.ChildVriables["AccentColor"].Value?.ToString() ?? "#ffff0000" ?? "#ffff0000");
            set => Template.ChildVriables["AccentColor"] = JitVariable.From(value.ToString(), classNames: ":RGB32");
        }

        /// <summary>
        /// template remarks
        /// </summary>
        public string Remarks
        {
            get => Template?.ChildVriables["Remarks"].Value?.ToString();
            set => Template.ChildVriables["Remarks"] = JitVariable.From(value);
        }

        public override string ToString()
        {
            return $"{GetType().Name} TemplateID = {ID} Name = {Name}";
        }

        public override bool Equals(object obj)
        {
            if (obj is TemplateTipModel tar)
            {
                return Name.Equals(tar.Name);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    /// <summary>
    /// Class Selection Tip Collection
    /// </summary>
    public class TemplateTipCollection : ObservableCollection<TemplateTipModel>
    {
    }
}
