// CarSourceManager.cs
using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using System.Windows.Forms;

namespace ImportExportModNamespace
{
    public class CarSourceManager
    {
        public Blip WarehouseBlip { get; set; }
        public bool PlayerEnteredMissionCar { get; set; } = false;
        public bool WarehouseWaypointSet { get; set; } = false;
        public ICarSourcingMission CurrentMission { get; private set; }
        public Vehicle missionCar { get; private set; }
        public Blip missionCarBlip;
        private WarehouseManager warehouseManager;
        private SettingsManager settingsManager;
        private const int ParkedCarMissionType = 0;
        System.Random random = new System.Random();
        public bool IsMissionActive { get; private set; }

        private List<string> baseGameCars = new List<string> { "adder", "zentorno", "jester" };
        private List<Vector3> carSpawnLocations = new List<Vector3>
        {
            new Vector3(1010.3323f, -1870.058f, 30.4136f), //parkingspot outside the test warehouse
            new Vector3(932.9781f, -1812.830f, 30.2651f), // down the street from test warehouse
        };
        private List<float> carSpawnHeadings = new List<float>
        {
            0f,  // Heading for the first spawn location
            90f, // Heading for the second spawn location
        };



        public CarSourceManager(SettingsManager settingsManager, WarehouseManager warehouseManager)
        {
            this.settingsManager = settingsManager;
            this.warehouseManager = warehouseManager;
        }

        public void GenerateCarSourceMission()
        {
            string selectedCarModel = settingsManager.GetRandomCarModelName();
        }

        public void StartCarSourcingMission()
        {
            if (IsMissionActive) return;

            // 1. Select a mission type
            int missionType = ParkedCarMissionType;

            // 2. Select a car from the list
            string carModelName = settingsManager.GetRandomCarModelName();

            // 3. Spawn the car at the mission location
            missionCar = SpawnCarAtMissionLocation(carModelName, missionType);

            // After spawning the vehicle
            int blipHandle = Function.Call<int>(Hash.ADD_BLIP_FOR_ENTITY, missionCar);
            missionCarBlip = new Blip(blipHandle);
            missionCarBlip.Sprite = BlipSprite.PersonalVehicleCar;
            missionCarBlip.Color = BlipColor.Yellow;
            missionCarBlip.Scale = 0.8f;
            missionCarBlip.Name = "Target Vehicle";

            GTA.UI.Notification.Show("Created mission car blip."); // Debug notification
            GTA.UI.Notification.Show($"Car position: {missionCar.Position}");
            GTA.UI.Notification.Show($"Warehouse position: {warehouseManager.OwnedWarehouseLocation}"); // Debug notification

            // 4. Set up mission elements
            SetupMissionElements(missionCar, missionType);

            IsMissionActive = true;
        }

        public void EndCarSourcingMission()
        {
            IsMissionActive = false;
            WarehouseWaypointSet = false;
            PlayerEnteredMissionCar = false;
            if (missionCarBlip != null)
            {
                //missionCarBlip.Delete();
                missionCarBlip = null;
            }
            Function.Call(Hash.CLEAR_GPS_PLAYER_WAYPOINT);

            if (WarehouseBlip != null)
            {
                WarehouseBlip.Delete();
                WarehouseBlip = null;
            }
        }


        private int SelectMissionType()
        {
            // TODO: Implement mission type selection logic
            return 0;
        }

        private Vehicle SpawnCarAtMissionLocation(string carModelName, int missionType)
        {
            // Get a random spawn location
            Random random = new Random();
            int spawnIndex = random.Next(carSpawnLocations.Count);
            Vector3 spawnLocation = carSpawnLocations[spawnIndex];
            float spawnHeading = carSpawnHeadings[spawnIndex];

            // Load the car model
            Model carModel = new Model(carModelName);
            carModel.Request();
            while (!carModel.IsLoaded)
            {
                Script.Wait(0);
            }

            // Spawn the car at the random location with the heading
            Vehicle spawnedCar = World.CreateVehicle(carModel, spawnLocation);
            spawnedCar.Heading = spawnHeading;

            // Clean up the car model
            carModel.MarkAsNoLongerNeeded();

            return spawnedCar;
        }



        private void SetupMissionElements(Vehicle missionCar, int missionType)
        {
            // For now, only handle the parked car mission
            if (missionType == ParkedCarMissionType)
            {
                Vector3 warehouseLocation = new Vector3(1000.0f, -1900.0f, 30.0f); // Replace with your warehouse location
                CurrentMission = new ParkedCarMission(missionCar, warehouseLocation);
                CurrentMission.StartMission();
            }
        }
    }
}