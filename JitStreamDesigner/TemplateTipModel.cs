// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Tono;
using Tono.Gui.Uwp;
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

        /// <summary>
        /// REDO Jac Queue
        /// </summary>
        [DataMember]
        public List<string> RedoStream { get; set; } = new List<string>();

        /// <summary>
        /// UNDO Jac Queue
        /// </summary>
        [DataMember]
        public List<string> UndoStream { get; set; } = new List<string>();

        /// <summary>
        /// UNDO/REDO Current Pointer
        /// </summary>
        [DataMember]
        public int UndoRedoCurrenttPointer { get; set; }

        /// <summary>
        /// UNDO/REDO Requested Pointer position
        /// </summary>
        [DataMember]
        public int UndoRedoRequestedPointer { get; set; }

        /// <summary>
        /// Work scroll position
        /// </summary>
        [DataMember]
        public (double X, double Y) Scroll { get; set; }

        /// <summary>
        /// Work zoom position
        /// </summary>
        [DataMember]
        public (double X, double Y) Zoom { get; set; }


        [IgnoreDataMember]
        private static readonly Regex hexpattern = new Regex("^[0-9,a-f,A-F]+$");

        /// <summary>
        /// Accent color for template sign background
        /// </summary>
        public Color AccentColor
        {
            get => ColorUtil.From(Template?.ChildVriables["AccentColor"].Value?.ToString() ?? "#ffff0000" ?? "#ffff0000");
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
