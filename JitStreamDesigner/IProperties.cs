// Copyright(c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using Tono.Jit;

namespace JitStreamDesigner
{
    public interface IPropertyInstanceName : IJitObjectID
    {
        string InstanceName { get; set; }
    };
    public interface IPropertyXy : IJitObjectID
    {
        string X { get; set; }
        string Y { get; set; }
    }
    public interface IPropertyWh : IJitObjectID
    {
        string W { get; set; }
        string H { get; set; }
    }

}
