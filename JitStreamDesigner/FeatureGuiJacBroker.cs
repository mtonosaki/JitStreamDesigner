// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tono;
using Tono.Gui;
using Tono.Gui.Uwp;
using Tono.Jit;

namespace JitStreamDesigner
{
    [FeatureDescription(En = "Broker between Gui and Jac")]
    [JacTarget(Name = "GuiBroker")]
    public class FeatureGuiJacBroker : FeatureSimulatorBase
    {
        public static class TOKEN
        {
            public const string NameChanged = "FeatureGuiJacBrokerNameChanged";
            public const string SizeChanged = "FeatureGuiJacBrokerSizeChanged";
            public const string LocationChanged = "FeatureGuiJacBrokerLocationChanged";
            public const string CioNewRemoveChanged = "FeatureGuiJacBrokerCioNewRemoveChanged";
            public const string CassetteValueChanged = "FeatureGuiJacBrokerCassetteValueChanged";
            public const string AddProcessLink = "FeatureGuiJacBrokerAddProcessLink";
            public const string RemoveProcessLink = "FeatureGuiJacBrokerRemoveProcessLink";
        }
        private readonly LinkedList<(string Remarks, Action Act)> Actions = new LinkedList<(string Remarks, Action Act)>();

        public override void OnInitialInstance()
        {
            Hot.TheBroker = this;
            WaitNext(true);
        }

        private bool IsOptimized = false;

        private void Optimize()
        {
            if (IsOptimized)
            {
                return;
            }

            bool isClearAll = false;
            var updatedIds = new Dictionary<string/*Parts.ID*/, bool>();
            var dels = new List<LinkedListNode<(string Remarks, Action Act)>>();
            for (var node = Actions.Last; node != null; node = node.Previous)
            {
                var ops = node.Value.Remarks.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (ops.Length >= 2)
                {
                    switch (ops[0])
                    {
                        case "ClearAllSelection":
                            if (isClearAll == false)
                            {
                                isClearAll = true;
                            }
                            else
                            {
                                dels.Add(node);
                            }
                            break;
                        case "UpdateLocation":
                            if (updatedIds.ContainsKey(ops[1]))
                            {
                                dels.Add(node);
                            }
                            else
                            {
                                updatedIds[ops[1]] = true;
                            }
                            break;
                    }
                }
            }
            foreach (var del in dels)
            {
                Actions.Remove(del);
            }
            IsOptimized = true;
        }

        private void ActionQueueProc()
        {
            if (Actions.Count == 0)
            {
                WaitNext(isSlow: true);
            }
            else
            {
                Optimize();

                var item = Actions.First;
                Actions.RemoveFirst();

                item.Value.Act.Invoke();
            }
        }

        private void WaitNext(bool isSlow = false)
        {
            DelayUtil.Start(TimeSpan.FromMilliseconds(isSlow ? 100 : 0), ActionQueueProc);
        }

        [JacGetDotValue]
        public object GetChildValue(string varname)
        {
            throw new JacException(JacException.Codes.SyntaxError, $"Cannot get a GuiBroker's property");
        }

        private string MakeRemarks(string varname, object value)
        {
            if (value is JitProcess process)
            {
                return $"{varname} {process.ID} // Process";
            }
            return $"{varname} = {value} // not a Jit Object";
        }

        [JacSetDotValue]
        public void SetChildValue(string varname, object value)
        {
            var me = typeof(FeatureGuiJacBroker).GetMethod(varname);
            if (me != null)
            {
                Actions.AddLast((MakeRemarks(varname, value), () => me.Invoke(this, new object[] { value })));
                IsOptimized = false;
            }
            else
            {
                throw new JacException(JacException.Codes.NotImplementedMethod, $"Method '{varname}' is not implement in FeatureGuiJacBroker yet.");
            }
        }

        private bool isBatchMode = false;

        public void BatchMode(object value)
        {
            if (value is bool sw)
            {
                isBatchMode = sw;
            }
            WaitNext();
        }

        /// <summary>
        /// Gui.Template JAC interface : Change active Template
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
                });
                Token.Finalize(() => WaitNext());   // Wait all tokens in queue finished for TOKEN.TemplateSelectionChanged
            }
            else
            {
                throw new JacException(JacException.Codes.ArgumentError, $"Template ID '{(value ?? "(null)")}' is not found in your study.");
            }
        }

        /// <summary>
        /// Gui.CreateProcess JAC interface
        /// </summary>
        /// <param name="value">JitProcess instance</param>
        public void CreateProcess(object value)
        {
            if (value is JitProcess proc)
            {
                Token.AddNew(new EventTokenProcessPartsTrigger
                {
                    TokenID = FeatureJitProcess.TOKEN.CREATE,
                    Process = proc,
                    Sender = this,
                });
            }
            Token.Finalize(() => WaitNext());   // Wait all tokens in queue finished for TOKEN.TemplateSelectionChanged
        }

        /// <summary>
        /// Gui.RemoveProcess JAC interface
        /// </summary>
        /// <param name="value">JitProcess instance</param>
        public void RemoveProcess(object value)
        {
            if (value is JitProcess proc)
            {
                Token.AddNew(new EventTokenProcessPartsTrigger
                {
                    TokenID = FeatureJitProcess.TOKEN.REMOVE,
                    Process = proc,
                    Sender = this,
                });
            }
            Token.Finalize(() => WaitNext());   // Wait all tokens in queue finished for TOKEN.TemplateSelectionChanged
        }

        /// <summary>
        /// Gui.UpdateLocation = Process.ID
        /// </summary>
        /// <param name="value">JitProcess</param>
        public void UpdateLocation(object value)
        {
            if (value is JitProcess process)
            {
                if (Parts.GetParts<PartsJitBase>(LAYER.JitProcess, a => a.ID == process.ID).FirstOrDefault() is PartsJitBase pt)
                {
                    pt.Location = CodePos<Distance, Distance>.From((Distance)process.ChildVriables["LocationX"].Value, (Distance)process.ChildVriables["LocationY"].Value);
                    pt.IsSelected = true;
                    Redraw();

                    Token.AddNew(new EventTokenJitVariableTrigger
                    {
                        TokenID = TOKEN.LocationChanged,
                        From = process,
                        Sender = this,
                    });
                }
            }
            WaitNext();
        }
        /// <summary>
        /// Gui.UpdateSize = Process.ID
        /// </summary>
        /// <param name="value">JitProcess</param>
        public void UpdateSize(object value)
        {
            if (value is JitProcess process)
            {
                if (Parts.GetParts<PartsJitBase>(LAYER.JitProcess, a => a.ID == process.ID).FirstOrDefault() is PartsJitBase pt)
                {
                    pt.Width = (Distance)process.ChildVriables["Width"].Value;
                    pt.Height = (Distance)process.ChildVriables["Height"].Value;
                    pt.IsSelected = true;
                    Redraw();

                    Token.AddNew(new EventTokenJitVariableTrigger
                    {
                        TokenID = TOKEN.SizeChanged,
                        From = process,
                        Sender = this,
                    });
                }
            }
            WaitNext();
        }

        /// <summary>
        /// Gui.UpdateName = Process.ID
        /// </summary>
        /// <param name="value">JitProcess</param>
        public void UpdateName(object value)
        {
            if (value is IJitObjectID va)
            {
                var tar = Hot.ActiveTemplate.Jac[va.ID];

                Token.AddNew(new EventTokenJitVariableTrigger
                {
                    TokenID = TOKEN.NameChanged,
                    From = va,
                    Sender = this,
                    Remarks = "Jac:Name Changed",
                });
            }
            WaitNext();
        }

        /// <summary>
        /// Gui.UpdateProcessCio = Process.ID
        /// </summary>
        /// <param name="value"></param>
        public void UpdateProcessCio(object value)
        {
            var co = value.ToString().Split(',');
            Debug.Assert(co.Length == 3);

            Token.AddNew(new EventTokenJitCioTrigger
            {
                TokenID = TOKEN.CioNewRemoveChanged,
                Action = co[0],
                TargetProcessID = co[1],
                FromCioID = co[2],
                Sender = this,
                Remarks = "Jac:Gui:Cio Add/Remove",
            });
            WaitNext();
        }

        /// <summary>
        /// Common Cassette Special Value Changed
        /// </summary>
        /// <param name="value"></param>
        public void UpdateCassetteValue(object value)
        {
            if (value is CioBase cio)
            {
                Token.AddNew(new EventTokenCioCassetteValueChangedTrigger
                {
                    TokenID = TOKEN.CassetteValueChanged,
                    Cio = cio,
                    CassetteID = cio.ID,
                    Sender = this,
                    Remarks = "Jac:Gui:Cassette Value Changed",
                });
            }
            WaitNext();
        }

        /// <summary>
        /// Gui.ClearAllSelection = dummy
        /// </summary>
        /// <param name="value">dummy</param>
        public void ClearAllSelection(object value)
        {
            var pts =
                from layer in LAYER.JitObjects
                from pt in Parts.GetParts<ISelectableParts>(layer)
                select pt;

            var nChanged = 0;
            foreach (var pt in pts)
            {
                if (pt.IsSelected)
                {
                    pt.IsSelected = false;
                    nChanged++;
                }
            }
            if (nChanged > 0)
            {
                Redraw();
            }
            WaitNext();
        }

        /// <summary>
        /// ProcessLink
        /// </summary>
        /// <param name="value"></param>
        public void AddProcLink(object value)
        {
            if (value is string link)
            {
                var vals = link.Split(',');
                if( vals.Length == 2)
                {
                    Token.AddNew(new EventTokenProcessLinkTrigger
                    {
                        TokenID = TOKEN.AddProcessLink,
                        ProcessIDFrom = vals[0].Trim(),
                        ProcessIDTo = vals[1].Trim(),
                        Sender = this,
                        Remarks = "Jac:Gui:ProcLink Add",
                    });
                }
            }
            WaitNext();
        }
        /// <summary>
        /// ProcessLink
        /// </summary>
        /// <param name="value"></param>
        public void RemoveProcLink(object value)
        {
            if (value is string link)
            {
                var vals = link.Split(',');
                if (vals.Length == 2)
                {
                    Token.AddNew(new EventTokenProcessLinkTrigger
                    {
                        ProcessIDFrom = vals[0].Trim(),
                        ProcessIDTo = vals[1].Trim(),
                        TokenID = TOKEN.RemoveProcessLink,
                        Sender = this,
                        Remarks = "Jac:Gui:ProcLink Remove",
                    });
                }
            }
            WaitNext();
        }

        public void CreateWork(object value)
        {
            if (value is JitWork work)
            {
                Token.AddNew(new EventTokenWorkPartsTrigger
                {
                    TokenID = FeatureJitWork.TOKEN.CREATE,
                    Work = work,
                    Sender = this,
                });
            }
            Token.Finalize(() => WaitNext());   // Wait all tokens in queue finished for TOKEN.TemplateSelectionChanged
        }

        public void RemoveWork(object value)
        {
            if (value is JitWork work)
            {
                Token.AddNew(new EventTokenWorkPartsTrigger
                {
                    TokenID = FeatureJitWork.TOKEN.REMOVE,
                    Work = work,
                    Sender = this,
                });
            }
            Token.Finalize(() => WaitNext());   // Wait all tokens in queue finished for TOKEN.TemplateSelectionChanged
        }
    }
}
