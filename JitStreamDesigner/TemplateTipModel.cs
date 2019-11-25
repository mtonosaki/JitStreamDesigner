using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tono.Jit;

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
        public string TemplateID { get; set; }

        /// <summary>
        /// Jit model stage
        /// </summary>
        public JitStage Stage { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name} TemplateID = {TemplateID}";
        }

        public override bool Equals(object obj)
        {
            if( obj is TemplateTipModel tar)
            {
                return TemplateID.Equals(tar.TemplateID);
            } else
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
    public class ClassTipCollection : List<TemplateTipModel>
    {
    }
}
