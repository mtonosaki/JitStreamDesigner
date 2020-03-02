using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JitStreamDesigner
{
    /// <summary>
    /// One time exec utility
    /// </summary>
    public class OneTime
    {
        public bool IsExecuted { get; private set; } = false;
        public void Execute(Action act)
        {
            if (IsExecuted == false)
            {
                act?.Invoke();
                IsExecuted = true;
            }
        }

        public void Reset()
        {
            IsExecuted = false;
        }
    }
}
