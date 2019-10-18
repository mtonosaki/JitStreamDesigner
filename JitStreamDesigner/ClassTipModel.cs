using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JitStreamDesigner
{
    /// <summary>
    /// Class Selection Tip
    /// </summary>
    public class ClassTipModel
    {
        public string ClassID { get; set; }

        public override string ToString()
        {
            return $"{GetType().Name} ClassID = {ClassID}";
        }

        public override bool Equals(object obj)
        {
            if( obj is ClassTipModel tar)
            {
                return ClassID.Equals(tar.ClassID);
            } else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return ClassID.GetHashCode();
        }

        public static bool operator ==(ClassTipModel left, ClassTipModel right) => left.Equals(right);
        public static bool operator !=(ClassTipModel left, ClassTipModel right) => !left.Equals(right);
    }

    /// <summary>
    /// Class Selection Tip Collection
    /// </summary>
    public class ClassTipCollection : List<ClassTipModel>
    {
    }
}
