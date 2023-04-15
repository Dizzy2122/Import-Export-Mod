// VehicleEventsArgs.cs
using System;
using GTA;

namespace ImportExportModNamespace
{   
        public class VehicleEventArgs : EventArgs
    {
        public Vehicle Vehicle { get; }

        public VehicleEventArgs(Vehicle vehicle)
        {
            Vehicle = vehicle;
        }
    }
}