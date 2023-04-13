//ParkedCarMission.cs
using GTA;
using GTA.Math;
using ImportExportModNamespace;


namespace ImportExportModNamespace
{
    public class ParkedCarMission : ICarSourcingMission
    {
        private Vehicle missionCar;
        // private Blip destinationBlip;
        private Vector3 warehouseLocation;

        public ParkedCarMission(Vehicle missionCar, Vector3 warehouseLocation)
        {
            this.missionCar = missionCar;
            this.warehouseLocation = warehouseLocation;
        }

        public void StartMission()
        {
            // Set up the destination blip
//            destinationBlip = World.CreateBlip(warehouseLocation);
//            destinationBlip.Sprite = BlipSprite.Warehouse;
//            destinationBlip.Color = BlipColor.Yellow;
//            destinationBlip.Scale = 1.0f;
//            destinationBlip.Name = "Warehouse";
        }

        public void UpdateMission()
        {
            // TODO: Add any mission-specific update logic here
        }

        public void EndMission()
        {
//            if (destinationBlip != null)
           {
//                destinationBlip.Delete();
//                destinationBlip = null;
            }
        }
    }
}