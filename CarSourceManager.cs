// CarSourceManager.cs
using System;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using NativeUI;
using System.Windows.Forms;
using System.Linq;

namespace ImportExportModNamespace
{
    public class CarSourceManager
    {
        public ICarSourcingMission ActiveMission { get; private set; }
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
            new Vector3(1010.3323f, -1870.058f, 30.4136f), // parkingspot outside the test warehouse
            new Vector3(932.9781f, -1812.830f, 30.2651f), // down the street from test warehouse
            new Vector3(769.8359f, -2963.262f, 5.3186f), // dock parking lot
            new Vector3(1115.0125f, -3138.796f, 5.2314f), // dock parking lot 2
            new Vector3(-228.8594f,-2600.245f,5.5256f), // Elysian Island parking
            new Vector3(-297.2009f, -2661.719f, 5.5356f), // Elysian Island parking 2
            new Vector3(871.7629f, -1349.943f, 25.8333f), // La Mesa Police Lockup
            new Vector3(-940.5126f, -2672.061f, 31.5748f), // LSIA Parking Garage
            new Vector3(-50.5228f, -2001.060f, 17.5415f), // Arena Parking
            new Vector3(-205.7497f, -2041.375f, 27.1431f), // Arena Parking 2
            new Vector3(63.2171f, -1542.497f, 28.9839f), // Strawberry
            new Vector3(467.2097f, -1344.746f, 43.0758f), // Strawberry Parking Garage
            new Vector3(454.4957f, -1366.513f, 38.0772f), // Strawberry Parking Garage 2
            new Vector3(57.0322f, -1415.404f, 28.8814f), // Car Wash
            new Vector3(485.2096f, -1105.826f, 28.7242f), // Misson Row
            new Vector3(306.7961f, -1094.168f, 28.8909f), // Mission Row 2
            new Vector3(145.0191f, -1144.953f, 28.8155f), // Pillbox
            new Vector3(27.7057f, -1070.947f, 37.5766f), // Ammunation Parking Garage
        };
        private List<float> carSpawnHeadings = new List<float>
        {
            0f,  // parkingspot outside the test warehouse
            90f, // down the street from test warehouse
            114.6677f, // dock parking lot
            180.9713f, //dock parkinglot 2
            153.3027f, // Elysian Island parking
            224.6166f, // Elysian Island parking 2
            90f, // LA Mesa Police Lockup
            60f, //LSIA Parking Garage
            108f, // Arena Parking
            240f, // Arena Parking 2
            231f, // Strawberry
            139f, // Strawberry Parking Garage
            230.5f, // Strawberry Parking Garage 2
            224f, // Car Wash
            89f, // Mission Row
            121.7f, // Mission Row 2
            4f, // Pillbox
            252.2358f, // Ammunation Parking Garage
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

            // If the car model is invalid, generate a new car sourcing mission and return
            if (missionCar == null)
            {
                GenerateCarSourceMission();
                return;
            }

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

            // Wait for the model to load or exit early if the model is not valid
            int attempts = 0;
            while (!carModel.IsLoaded && attempts < 50)
            {
                Script.Wait(100);
                attempts++;
            }

            // If the model is not loaded after 50 attempts, return null
            if (!carModel.IsLoaded)
            {
                GTA.UI.Notification.Show("Failed to load car model. Please check the model name.");
                return null;
            }

            // Spawn the car at the random location with the heading
            Vehicle spawnedCar = World.CreateVehicle(carModel, spawnLocation);
            spawnedCar.Heading = spawnHeading;

            // Mark the vehicle as owned by the player
            spawnedCar.IsPersistent = true;

            // Apply random modifications to the spawned car
            RandomVehicleModifications randomMods = new RandomVehicleModifications(spawnedCar);
            randomMods.ApplyAll();

            // Clean up the car model
            carModel.MarkAsNoLongerNeeded();

            return spawnedCar;
        }

        public void EndMission()
        {
            if (missionCar != null)
            {
                missionCar.MarkAsNoLongerNeeded();
                missionCar = null;
            }

            if (missionCarBlip != null)
            {
                missionCarBlip.Delete();
                missionCarBlip = null;
            }

            IsMissionActive = false;
            PlayerEnteredMissionCar = false;
            WarehouseWaypointSet = false;
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