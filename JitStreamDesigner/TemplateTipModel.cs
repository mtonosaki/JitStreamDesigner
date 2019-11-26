using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        /// Template ID (Name)
        /// </summary>
        public string TemplateID 
        {
            get => Template?.Name ?? "(n/a)";
            set => Template.Name = value;
        }

        /// <summary>
        /// TODO: Waiting NuGet Tono.Gui.Uwp
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Color ColorUtilFrom(string str)
        {
            if (str.StartsWith("#")) str = StrUtil.Mid(str, 1);
            if (str.StartsWith("0x")) str = StrUtil.Mid(str, 2);

            var val = UInt32.Parse(str, System.Globalization.NumberStyles.HexNumber);
            return Color.FromArgb((byte)((val & 0xff000000) / 0x1000000), (byte)((val & 0x00ff0000) / 0x10000), (byte)((val & 0x0000ff00) / 0x100), (byte)(val & 0x000000ff));
        }

        public Color AccentColor
        {
            get => ColorUtilFrom(Template?.ChildVriables["AccentColor"].Value?.ToString() ?? "#ffff0000" ?? "#ffff0000");
            set => Template.ChildVriables["AccentColor"] = JitVariable.From(value.ToString(), classNames: ":RGB32");
        }

        public string Remarks
        {
            get => Template?.ChildVriables["Remarks"].Value?.ToString();
            set => Template.ChildVriables["Remarks"] = JitVariable.From(value);
        }

        /// <summary>
        /// Jit model stage
        /// </summary>
        public JitTemplate Template { get; set; } = new JitTemplate();

        public override string ToString()
        {
            return $"{GetType().Name} TemplateID = {TemplateID}";
        }

        public override bool Equals(object obj)
        {
            if (obj is TemplateTipModel tar)
            {
                return TemplateID.Equals(tar.TemplateID);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return TemplateID.GetHashCode();
        }

        public static bool operator ==(TemplateTipModel left, TemplateTipModel right) => left.Equals(right);
        public static bool operator !=(TemplateTipModel left, TemplateTipModel right) => !left.Equals(right);
    }

    /// <summary>
    /// Class Selection Tip Collection
    /// </summary>
    public class TemplateTipCollection : ObservableCollection<TemplateTipModel>
    {
    }
}
