// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using Tono.Jit;

namespace JitStreamDesigner
{
    public interface ISetPropertyTarget
    {
        void SetPropertyTarget(object target);
    }
    public interface IEventPropertyCioOpen
    {
        event EventHandler<CioClickedEventArgs> CioClicked;
    }
    public interface IEventPropertySpecificUndoRedo
    {
        event EventHandler<NewUndoRedoEventArgs> NewUndoRedo;
    }

    public interface IUpdateCassette
    {
        void UpdateCassette();
    }

    public interface IPropertyInstanceName : IJitObjectID
    {
        string InstanceName { get; set; }
        Dictionary<string, object> PreviousValue { get; }
    };
    public interface IPropertyXy : IJitObjectID
    {
        string X { get; set; }
        string Y { get; set; }
        Dictionary<string, object> PreviousValue { get; }
    }
    public interface IPropertyWh : IJitObjectID
    {
        string W { get; set; }
        string H { get; set; }
        Dictionary<string, object> PreviousValue { get; }
    }

}
