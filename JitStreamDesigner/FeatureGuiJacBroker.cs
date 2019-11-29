using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tono.Gui.Uwp;
using Tono.Jit;

namespace JitStreamDesigner
{
    [FeatureDescription(En = "Broker between Gui and Jac")]
    [JacTarget(Name = "GuiBroker")]
    public class FeatureGuiJacBroker : FeatureSimulatorBase
    {
        private Queue<Action> Actions = new Queue<Action>();

        public override void OnInitialInstance()
        {
            Hot.TheBroker = this;
            DelayUtil.Start(TimeSpan.FromMilliseconds(50), ActionQueueProc);
        }

        private void ActionQueueProc()
        {
            if (Actions.Count == 0)
            {
                WaitNext(isSlow: true);
            }
            else
            {
                Actions.Dequeue()?.Invoke();
            }
        }

        private void WaitNext(bool isSlow = false)
        {
            DelayUtil.Start(TimeSpan.FromMilliseconds(isSlow ? 200 : 50), ActionQueueProc);
        }

        [JacGetDotValue]
        public object GetChildValue(string varname)
        {
            throw new JacException(JacException.Codes.SyntaxError, $"Cannot get a GuiBroker's property");
        }

        [JacSetDotValue]
        public void SetChildValue(string varname, object value)
        {
            var me = typeof(FeatureGuiJacBroker).GetMethod(varname);
            if (me != null)
            {
                Actions.Enqueue(() => me.Invoke(this, new object[] { value }));
            }
            else
            {
                throw new JacException(JacException.Codes.NotImplementedMethod, $"Method '{varname}' is not implement in FeatureGuiJacBroker yet.");
            }
        }

        /// <summary>
        /// Select active template
        /// </summary>
        /// <param name="value"></param>
        public void Template(object value)
        {
            var temp = Hot.TemplateList.Where(a => a.Template.ID == value?.ToString()).FirstOrDefault();
            if (temp != null)
            {
                Token.AddNew(new EventTokenTemplateChangedTrigger
                {
                    TargetTemplate = temp,
                    TokenID = FeatureJitTemplateListPanel.TOKEN.TemplateSelectionChanged,
                    Sender = this,
                    Remarks = DateTime.Now.ToString(),
                }); ;
                Token.Finalize(() => WaitNext());
            }
            else
            {
                throw new JacException(JacException.Codes.ArgumentError, $"Template ID '{(value ?? "(null)")}' is not found in your study.");
            }
        }

        /// <summary>
        /// Create Gui parts : Process
        /// </summary>
        /// <param name="value"></param>
        public void CreateProcess(object value)
        {

            WaitNext();
        }

        public void RemoveProcess(object value)
        {
            WaitNext();
        }
    }
}
