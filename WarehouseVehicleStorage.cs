// WarehouseVehicleStorage.cs
using GTA;
using GTA.Native;
using GTA.Math;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System;

namespace ImportExportModNamespace
{
    public class WarehouseVehicleStorage
    {
        public event Action<VehicleEventArgs> VehicleTeleportedInside;
        private ImportExportMod importExportMod;
        private List<float> storageSlotHeadings;
        private const int MaxSlots = 20;
        private List<Vehicle> storedVehicles = new List<Vehicle>(new Vehicle[MaxSlots]);
        private List<Vector3> storageSlotPositions;

        public WarehouseVehicleStorage(Action<VehicleEventArgs> onVehicleTeleportedInside, ImportExportMod importExportMod)
        {
            this.importExportMod = importExportMod;
            VehicleTeleportedInside += importExportMod.OnVehicleTeleportedInside;

            VehicleTeleportedInside = onVehicleTeleportedInside;

            storageSlotPositions = new List<Vector3>(MaxSlots);
            storageSlotHeadings = new List<float>(MaxSlots);

            // Add hardcoded storage slot positions and headings here
            storageSlotPositions.Add(new Vector3(978.1122f, -2993.954f, -40.1204f));
            storageSlotHeadings.Add(180.0f);

            storageSlotPositions.Add(new Vector3(987.3356f, -2991.736f, -40.1228f));
            storageSlotHeadings.Add(0.0f);

            storageSlotPositions.Add(new Vector3(996.0087f, -2991.213f, -40.2202f));
            storageSlotHeadings.Add(0.0f);

            storageSlotPositions.Add(new Vector3(1001.2040f, -2990.638f, -40.3836f));
            storageSlotHeadings.Add(90.0f);

            storageSlotPositions.Add(new Vector3(1001.2040f, -2994.403f, -40.3836f));
            storageSlotHeadings.Add(90.0f);

            // ... and so on for all storage slots
        }



        public bool IsVehicleCloseToWarehouseDoor(Vehicle vehicle, Vector3 warehouseDoorPosition, float proximityDistance = 10.0f)
        {
            return vehicle.Position.DistanceTo(warehouseDoorPosition) <= proximityDistance;
        }

        public List<StoredVehicle> StoredVehicles { get; set; } = new List<StoredVehicle>();

        public void StoreVehicle(Vehicle vehicle)
        {
            StoredVehicle storedVehicle = new StoredVehicle();
            storedVehicle.ModelName = vehicle.Model.ToString();
            
            for (int modIndex = 0; modIndex < 50; modIndex++)
            {
                int modVariation = Function.Call<int>(Hash.GET_VEHICLE_MOD, vehicle, modIndex);
                if (modVariation != -1)
                {
                    VehicleModification modification = new VehicleModification
                    {
                        ModIndex = modIndex,
                        ModVariation = modVariation
                    };
                    storedVehicle.Modifications.Add(modification);
                }
            }

            StoredVehicles.Add(storedVehicle);

            // Serialize the stored vehicles to JSON
            string storedVehiclesJson = JsonConvert.SerializeObject(StoredVehicles);

            // Save the JSON string to a file (e.g., "WarehouseVehicles.json")
            File.WriteAllText("WarehouseVehicles.json", storedVehiclesJson);
        }

        public class StoredVehicle
        {
            public string ModelName { get; set; }
            public List<VehicleModification> Modifications { get; set; } = new List<VehicleModification>();
        }

        public class VehicleModification
        {
            public int ModIndex { get; set; }
            public int ModVariation { get; set; }
        }

        public void HandleVehicleStorage(Vehicle vehicle, Vector3 warehouseDoorPosition, Vector3 insideWarehousePosition, int slotIndex)
        {
            // End the active mission       
            Function.Call(Hash.SET_MISSION_FLAG, false); 

            // Teleport the vehicle inside the warehouse
            vehicle.Position = insideWarehousePosition;

            GTA.UI.Notification.Show("Teleporting vehicle."); // Add this debug notification

            // Call the passed action instead of raising the event
            VehicleTeleportedInside?.Invoke(new VehicleEventArgs(vehicle));

            // Set the vehicle's heading to the heading of the storage slot
            vehicle.Heading = storageSlotHeadings[slotIndex];

            GTA.UI.Notification.Show("Vehicle teleported."); // Add this debug notification

            // Make the player exit the vehicle after the teleportation
            Game.Player.Character.Task.LeaveVehicle(vehicle, true);

            // Wait for 1000 ms (1 second) before freezing the vehicle and turning off its engine
            Script.Wait(1000);

            // Freeze the vehicle so it doesn't roll or move inside the warehouse
            vehicle.IsPositionFrozen = true;

            // Save the vehicle's data to the JSON file
            SaveVehicleData(vehicle, slotIndex);
            GTA.UI.Notification.Show("Vehicle data is saved."); // Add this debug notification

            // Store the vehicle in the warehouse
            StoreVehicle(vehicle, slotIndex);
        }




        private void ApplyModificationsToVehicle(Vehicle vehicle, StoredVehicle storedVehicle)
        {
            foreach (VehicleModification modification in storedVehicle.Modifications)
            {
                Function.Call(Hash.SET_VEHICLE_MOD, vehicle, modification.ModIndex, modification.ModVariation, false);
            }
        }

        public int FindAvailableSlot()
        {
            for (int i = 0; i < MaxSlots; i++)
            {
                if (storedVehicles[i] == null)
                {
                    return i;
                }
            }
            return -1; // No available slot found
        }


        public Vector3 GetStorageSlotPosition(int index)
        {
            if (index >= 0 && index < storageSlotPositions.Count)
            {
                return storageSlotPositions[index];
            }
            else
            {
                // You can return a default value (e.g., Vector3.Zero) or throw an exception if the index is out of range
                return Vector3.Zero;
            }
        }



        public void StoreVehicle(Vehicle vehicle, int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < MaxSlots && storedVehicles[slotIndex] == null)
            {
                storedVehicles[slotIndex] = vehicle; // Update the storedVehicles list

                // Save the vehicle data to a JSON file
                SaveVehicleData(vehicle, slotIndex);

                // Position the vehicle and player
                vehicle.Position = storageSlotPositions[slotIndex];
                vehicle.IsPersistent = true;
            }
        }

        private void SaveVehicleData(Vehicle vehicle, int slotIndex)
        {
            Directory.CreateDirectory("scripts\\StoredVehicleData");

            CustomVehicleData customVehicleData = new CustomVehicleData();
            customVehicleData.ModelName = vehicle.Model.ToString();

            for (int modIndex = 0; modIndex < 50; modIndex++)
            {
                int modVariation = Function.Call<int>(Hash.GET_VEHICLE_MOD, vehicle, modIndex);
                if (modVariation != -1)
                {
                    VehicleModification modification = new VehicleModification
                    {
                        ModIndex = modIndex,
                        ModVariation = modVariation
                    };
                    customVehicleData.Modifications.Add(modification);
                }
            }

            string vehicleDataJson = JsonConvert.SerializeObject(customVehicleData);
            string filePath = $"scripts\\StoredVehicleData\\Vehicle_{slotIndex}.json";
            File.WriteAllText(filePath, vehicleDataJson);
        }

        public class CustomVehicleData
        {
            public string ModelName { get; set; }
            public List<VehicleModification> Modifications { get; set; } = new List<VehicleModification>();
        }
    }
}

