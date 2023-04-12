// Warehouse.cs

using GTA.Math;
using GTA;

namespace ImportExportModNamespace
{
    public class Warehouse
    {
        public Vector3 Location { get; set; }
        public bool IsOwned { get; set; }
        public string Name { get; set; }
        public Blip Blip { get; set; }

        public Warehouse(Vector3 location)
        {
            Location = location;
            IsOwned = false;
        }
    }
}
