// WarehouseManager.cs
using System;
using GTA;
using GTA.Native;
using NativeUI;
using System.Windows.Forms;
using System.Collections.Generic;
using GTA.Math;

namespace ImportExportModNamespace
{
    public class WarehouseManager
{
    public bool IsPlayerNearWarehouse
    {
        get
        {
            if (OwnedWarehouseLocation == Vector3.Zero) return false;
            return World.GetDistance(Game.Player.Character.Position, OwnedWarehouseLocation) < 50f;
        }
    }
        public int WarehouseCost { get; } = 0; // Change when version is final
        public Blip OwnedWarehouseBlip { get; private set; }
        public List<Warehouse> Warehouses { get; private set; }
        public Warehouse NearestWarehouse { get; set; }
        public Vector3 NearestWarehouseLocation => NearestWarehouse?.Location ?? Vector3.Zero;
        public Warehouse OwnedWarehouse { get; private set; }
        public Vector3 OwnedWarehouseLocation
        {
            get => OwnedWarehouse?.Location ?? Vector3.Zero;
            private set
            {
                if (OwnedWarehouse != null)
                {
                    OwnedWarehouse.Location = value;
                }
            }
        }


        public WarehouseManager()
        {
            InitializeWarehouses();
        }
        

        private void InitializeWarehouses()
        {
            Warehouses = new List<Warehouse>
            {
                new Warehouse(new Vector3(144.3558f, -3004.987f, 7.030922f)), // Elysian Island Warehouse
                new Warehouse(new Vector3(804.5468f, -2220.445f, 29.44725f)), // Cypress Flats Warehouse
                new Warehouse(new Vector3(1211.428f, -1262.586f, 35.22675f)), // Murrieta Heights Warehouse
                new Warehouse(new Vector3(1764.536f, -1647.494f, 112.6444f)), // El Buro Heights Warehouse
                new Warehouse(new Vector3(-71.89099f, -1821.3256f, 26.94197f)), // Davis Street Warehouse
                new Warehouse(new Vector3(-1152.891f, -2173.466f, 13.26305f)), // Los Santos International Airport Warehouse
                new Warehouse(new Vector3(-513.2588f, -2199.715f, 6.394024f)), // Los Santos International Airport Warehouse 2
                new Warehouse(new Vector3(-636.0688f, -1774.854f, 24.0514f)), // La Puerta Warehouse
                new Warehouse(new Vector3(998.7968f, -1855.474f, 31.03981f)) // La Mesa Warehouse
            };

            CreateBlips();

            // Call CreateOwnedWarehouseBlip if the warehouse is already owned when the script is loaded
            if (OwnedWarehouse != null)
            {
                CreateOwnedWarehouseBlip(OwnedWarehouse);
            }
        }

        public void CreateBlips()
        {
            foreach (Warehouse warehouse in Warehouses)
            {
                // Skip creating a blip if the warehouse is owned
                if (warehouse == OwnedWarehouse)
                {
                    continue;
                }

                Blip blip = World.CreateBlip(warehouse.Location);
                blip.Sprite = BlipSprite.Warehouse;
                blip.Name = warehouse.Name ?? "Warehouse"; // Set a default name if the warehouse name is not set
                blip.IsShortRange = true;
                warehouse.Blip = blip;
            }
        }


        public void UpdateOwnedWarehouseBlip(Warehouse previousOwnedWarehouse = null)
{
    if (OwnedWarehouse != null)
    {
        if (previousOwnedWarehouse != null)
        {
            // Remove the white blip from the previous owned warehouse
            previousOwnedWarehouse.Blip.Delete();

            // Re-create the white blip for the previous owned warehouse
            Blip previousBlip = World.CreateBlip(previousOwnedWarehouse.Location);
            previousBlip.Sprite = BlipSprite.Warehouse;
            previousBlip.Name = "Warehouse";
            previousBlip.IsShortRange = true;
            previousOwnedWarehouse.Blip = previousBlip;
        }

        if (OwnedWarehouseBlip != null)
        {
            OwnedWarehouseBlip.Delete();
        }

        OwnedWarehouseBlip = World.CreateBlip(OwnedWarehouse.Location);
        OwnedWarehouseBlip.Sprite = BlipSprite.Warehouse;
        OwnedWarehouseBlip.Color = BlipColor.Yellow; // Change the color to Yellow
        OwnedWarehouseBlip.Name = "Owned Warehouse";
        OwnedWarehouseBlip.IsShortRange = true;
    }
}




        public void SetOwnedWarehouseLocation(Vector3 location)
        {
            Warehouse previousOwnedWarehouse = OwnedWarehouse;

            foreach (var warehouse in Warehouses)
            {
                if (warehouse.Location == location)
                {
                    OwnedWarehouse = warehouse;
                    break;
                }
            }

            if (previousOwnedWarehouse != null)
            {
                // Update the previous owned warehouse blip
                previousOwnedWarehouse.Blip.Color = BlipColor.White;
                previousOwnedWarehouse.Blip.Name = "Warehouse";
            }

            UpdateOwnedWarehouseBlip();
        }



        public void CheckPlayerProximity()
        {
            NearestWarehouse = GetNearestWarehouse();
        }

        private Warehouse GetNearestWarehouse()
        {
            Warehouse nearest = null;
            float minDistance = float.MaxValue;

            foreach (var warehouse in Warehouses)
            {
                float distance = Game.Player.Character.Position.DistanceTo(warehouse.Location);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = warehouse;
                }
            }

            return minDistance <= 100.0f ? nearest : null;
        }

        public void UpdateNearestWarehouse()
        {
            NearestWarehouse = GetNearestWarehouse();
        }



        public void SetOwnedWarehouseBlip(Blip blip)
        {
            if (OwnedWarehouseBlip != null)
            {
                OwnedWarehouseBlip.Delete();
            }

            OwnedWarehouseBlip = blip;
            OwnedWarehouseBlip.Color = BlipColor.Green; // Change the color to the desired one
            OwnedWarehouseBlip.IsShortRange = true;
        }




        public void RemoveOwnedWarehouse()
        {
            if (OwnedWarehouse != null)
            {
                // Remove the white blip from the owned warehouse
                OwnedWarehouse.Blip.Delete();

                // Re-create the white blip for the owned warehouse
                Blip blip = World.CreateBlip(OwnedWarehouse.Location);
                blip.Sprite = BlipSprite.Warehouse;
                blip.Name = "Warehouse";
                blip.IsShortRange = true;
                OwnedWarehouse.Blip = blip;

                // Remove the owned warehouse blip
                if (OwnedWarehouseBlip != null)
                {
                    OwnedWarehouseBlip.Delete();
                    OwnedWarehouseBlip = null;
                }

                // Remove the reference to the owned warehouse
                OwnedWarehouse = null;
            }
        }



        public void RemoveBlips()
        {
            foreach (Warehouse warehouse in Warehouses)
            {
                warehouse.Blip?.Delete();
            }
        }


    private void CreateOwnedWarehouseBlip(Warehouse warehouse)
        {
            if (OwnedWarehouseBlip != null)
            {
                OwnedWarehouseBlip.Delete();
            }

            OwnedWarehouseBlip = World.CreateBlip(warehouse.Location);
            OwnedWarehouseBlip.Sprite = BlipSprite.Warehouse; // Keep the warehouse icon
            OwnedWarehouseBlip.Color = BlipColor.Yellow; // Change the color to yellow
            OwnedWarehouseBlip.Name = "Owned Warehouse"; // Set the name to "Owned Warehouse"
            OwnedWarehouseBlip.IsShortRange = true;
        }


        public Vector3 GetOwnedWarehouseExteriorLocation()
        {
            // Return the location of the owned warehouse, or Vector3.Zero if there's no owned warehouse
            return OwnedWarehouse != null ? OwnedWarehouse.Location : Vector3.Zero;
        }
    }
}




