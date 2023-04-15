// ImportExportMod.cs
using GTA;
using GTA.Native;
using NativeUI;
using System.Collections.Generic;
using System;
using GTA.Math;

namespace ImportExportModNamespace
{
    
    public class ImportExportMod : Script
    {

        // Declare managers here
        private WarehouseVehicleStorage warehouseVehicleStorage;
        private WarehouseManager warehouseManager;
        private WarehouseOwnershipManager warehouseOwnershipManager;
        private ExitWarehouseManager exitWarehouseManager;
        private WarehouseInteriorManager warehouseInteriorManager;
        private CarSourceManager carSourceManager;
        private CustomizationManager customizationManager;
        private SettingsManager settingsManager;
        private LaptopMenu laptopMenu;
        private Vector3 laptopPosition = new Vector3(964.4566f, -3002.892f, -39.6399f);
        private const string IniFilePath = "scripts\\ImportExportModSettings.ini";
        public bool IsMissionActive { get; set; } = false;



        // Declare 'menuPool' and 'warehouseMenu' as class-level variables
        private MenuPool menuPool;
        private UIMenu warehouseMenu;
        private bool wasInsideWarehouse;


        public ImportExportMod()
        {

            // Initialize managers here
            settingsManager = new SettingsManager(IniFilePath);
            warehouseManager = new WarehouseManager();
            exitWarehouseManager = new ExitWarehouseManager(warehouseManager);
            warehouseOwnershipManager = new WarehouseOwnershipManager();
            warehouseInteriorManager = new WarehouseInteriorManager();
            carSourceManager = new CarSourceManager(settingsManager, warehouseManager);
            customizationManager = new CustomizationManager();

              if (warehouseOwnershipManager.GetOwnedWarehouse() != null)
            {
                warehouseManager.SetOwnedWarehouseLocation(warehouseOwnershipManager.GetOwnedWarehouse().ToVector3());
            }

            warehouseManager.CreateBlips();


            // Create an Action delegate and pass it to the constructor
            Action<VehicleEventArgs> onVehicleTeleportedInsideAction = OnVehicleTeleportedInside;
            warehouseVehicleStorage = new WarehouseVehicleStorage(onVehicleTeleportedInsideAction, this);

            // Initialize MenuPool
            menuPool = new MenuPool();

            laptopMenu = new LaptopMenu(carSourceManager);
            laptopMenu.TeleportToWarehouseExterior = TeleportToOwnedWarehouseExterior;

            // Setup warehouse menu
            SetupWarehouseMenu();

            // Subscribe to the Tick event
            Tick += OnTick;

            // Other initialization code, including setting up menus, key bindings, etc.
        }


        private void OnTick(object sender, EventArgs e)
        {
            laptopMenu.Process();
            menuPool.ProcessMenus();
            warehouseManager.UpdateNearestWarehouse();
            exitWarehouseManager.OnTick(warehouseInteriorManager);

            // Define the warehouse door position
            Vector3 warehouseDoorPosition = warehouseManager.OwnedWarehouseLocation;

            Vehicle playerVehicle = Game.Player.Character.CurrentVehicle;
            float triggerDistance = 10.0f; // Set the distance threshold as needed


            if (playerVehicle != null && playerVehicle.Position.DistanceTo(warehouseDoorPosition) < triggerDistance &&
            (!carSourceManager.IsMissionActive || (carSourceManager.IsMissionActive && carSourceManager.missionCar == playerVehicle)))
            {
                // Find an available storage slot and calculate the inside warehouse position
                int availableSlot = warehouseVehicleStorage.FindAvailableSlot();
                if (availableSlot != -1)
                {
                    Vector3 insideWarehousePosition = warehouseVehicleStorage.GetStorageSlotPosition(availableSlot);

                    // Define the player inside warehouse position (e.g., 2 units forward and 2 units up from the vehicle's storage slot)
                    Vector3 playerInsideWarehousePosition = insideWarehousePosition + new Vector3(0.0f, 2.0f, 2.0f);

                    GTA.UI.Notification.Show("Preparing to store vehicle."); // Add this debug notification

                    // Handle vehicle storage
                    warehouseVehicleStorage.HandleVehicleStorage(playerVehicle, warehouseDoorPosition, insideWarehousePosition, availableSlot);
                }
            }

            bool isInsideWarehouse = warehouseInteriorManager.IsPlayerInsideWarehouse();
            if (isInsideWarehouse != wasInsideWarehouse)
            {
                wasInsideWarehouse = isInsideWarehouse;

                if (isInsideWarehouse)
                {
                    GTA.UI.Notification.Show("Inside warehouse.");
                }
                else
                {
                    GTA.UI.Notification.Show("Outside warehouse.");
                }
            }

            if (isInsideWarehouse)
            {
                float distanceToLaptop = Game.Player.Character.Position.DistanceTo(laptopPosition);

                if (distanceToLaptop < 2.0f) // Adjust the distance threshold as needed
                {
                    if (!laptopMenu.Menu.Visible)
                    {
                        laptopMenu.Menu.Visible = true;
                    }
                }
                else
                {
                    if (laptopMenu.Menu.Visible)
                    {
                        laptopMenu.Menu.Visible = false;
                    }
                }
            }

            if (!isInsideWarehouse)
            {
                float distanceToNearestWarehouse = Game.Player.Character.Position.DistanceTo(warehouseManager.NearestWarehouseLocation ?? Vector3.Zero);

                if (distanceToNearestWarehouse < 5f)
                {
                    bool isOwned = warehouseManager.OwnedWarehouseLocation == warehouseManager.NearestWarehouseLocation;

                    warehouseMenu.MenuItems[0].Enabled = !isOwned;
                    warehouseMenu.MenuItems[1].Enabled = isOwned;
                    warehouseMenu.MenuItems[2].Enabled = isOwned;

                    if (!warehouseMenu.Visible)
                    {
                        warehouseMenu.Visible = true;
                    }
                }
                else
                {
                    if (warehouseMenu.Visible)
                    {
                        warehouseMenu.Visible = false;
                    }
                }
            }

            if (carSourceManager.IsMissionActive)
            {
                Vehicle missionCar = carSourceManager.missionCar;
                Blip missionCarBlip = carSourceManager.missionCarBlip;

                if (missionCar != null && missionCarBlip != null)
                {
                    if (Game.Player.Character.IsInVehicle(missionCar))
                    {
                        GTA.UI.Notification.Show("Player is in mission car."); // Debug notification
                        missionCarBlip.Alpha = 0;

                        // Set the warehouse waypoint when the player gets in the target vehicle
                        if (!carSourceManager.WarehouseWaypointSet)
                        {
                            Function.Call(Hash.SET_NEW_WAYPOINT, warehouseManager.OwnedWarehouseLocation.X, warehouseManager.OwnedWarehouseLocation.Y);
                            carSourceManager.WarehouseWaypointSet = true;
                            GTA.UI.Notification.Show("Setting warehouse waypoint."); // Debug notification
                        }
                    }
                    else
                    {
                        missionCarBlip.Alpha = 255;
                    }
                }
            }

            if (carSourceManager.PlayerEnteredMissionCar && !carSourceManager.WarehouseWaypointSet)
            {
                Function.Call(Hash.SET_NEW_WAYPOINT, warehouseManager.OwnedWarehouseLocation.X, warehouseManager.OwnedWarehouseLocation.Y);
                carSourceManager.WarehouseWaypointSet = true;
                GTA.UI.Notification.Show("Route to the warehouse set.");
            }

            // Add this line to update the 'Source a Vehicle' button state
            laptopMenu.Menu.MenuItems[0].Enabled = !carSourceManager.IsMissionActive;
        }




        private void SetupWarehouseMenu()
        {
            warehouseMenu = new UIMenu("Warehouse", "OPTIONS");
            menuPool.Add(warehouseMenu);

            UIMenuItem purchaseItem = new UIMenuItem("Purchase Warehouse");
            UIMenuItem sellItem = new UIMenuItem("Sell Warehouse");
            UIMenuItem enterItem = new UIMenuItem("Enter Warehouse");

            warehouseMenu.AddItem(purchaseItem);
            warehouseMenu.AddItem(sellItem);
            warehouseMenu.AddItem(enterItem);

            warehouseMenu.OnItemSelect += (sender, item, index) =>
            {
                GTA.UI.Notification.Show($"Item selected: {item.Text}");

                if (item == purchaseItem)
                {
                    GTA.UI.Notification.Show("Purchase Warehouse selected");
                    PurchaseWarehouse(warehouseManager.NearestWarehouse);
                }
                else if (item == sellItem)
                {
                    GTA.UI.Notification.Show("Sell Warehouse selected");
                    SellWarehouse();
                }
                else if (item == enterItem)
                {
                    GTA.UI.Notification.Show("Enter Warehouse selected");
                    // Enter the warehouse
                    warehouseInteriorManager.EnterWarehouse(warehouseManager.OwnedWarehouse);
                }
            };

            warehouseMenu.RefreshIndex();
        }
    
                private void PurchaseWarehouse(Warehouse warehouseToPurchase)
                {
                    OwnedWarehouseData newOwnedWarehouse = new OwnedWarehouseData(warehouseToPurchase.Location.X, warehouseToPurchase.Location.Y, warehouseToPurchase.Location.Z); // Extract the location from warehouseToPurchase

                    warehouseOwnershipManager.SetOwnedWarehouse(newOwnedWarehouse);
                    warehouseOwnershipManager.SaveOwnedWarehouse();

                    // Call the new BuyWarehouse method here
                    warehouseManager.BuyWarehouse(warehouseToPurchase.Location);

                    // ... (Additional logic for purchasing a warehouse, e.g., deducting money, showing a notification, etc.)

                    warehouseManager.SetOwnedWarehouseLocation(warehouseManager.NearestWarehouseLocation ?? Vector3.Zero);
                    warehouseManager.SetOwnedWarehouseBlip(World.CreateBlip(warehouseManager.NearestWarehouseLocation ?? Vector3.Zero));
                    warehouseManager.UpdateOwnedWarehouseBlip();
                }


                private void SellWarehouse()
                {
                    warehouseOwnershipManager.SetOwnedWarehouse(null);
                    warehouseOwnershipManager.SaveOwnedWarehouse();

                    // ... (Additional logic for selling a warehouse, e.g., adding money, showing a notification, etc.)

                    warehouseManager.RemoveOwnedWarehouse();
                    warehouseManager.UpdateOwnedWarehouseBlip();
                }

                private void TeleportToOwnedWarehouseExterior()
                {
                    Vector3 exteriorLocation = warehouseManager.GetOwnedWarehouseExteriorLocation();
                    if (exteriorLocation != Vector3.Zero)
                    {
                        Game.Player.Character.Position = exteriorLocation;
                    }
                }
                public void OnVehicleTeleportedInside(VehicleEventArgs e)
                {
                    GTA.UI.Notification.Show("OnVehicleTeleportedInside called."); // Debug notification

                    // Check if the mission is active and if the vehicle teleported inside is the mission car
                    if (carSourceManager.IsMissionActive && e.Vehicle == carSourceManager.missionCar)
                    {
                        // Perform any necessary actions here, such as showing the inside menus, adding money, or updating player stats

                        // End the mission
                        carSourceManager.ActiveMission?.EndMission(); // Call the EndMission method of the active mission instance
                        carSourceManager.EndMission();
                        GTA.UI.Notification.Show("Mission complete!");

                        // Remove the waypoint
                        Function.Call(Hash.SET_WAYPOINT_OFF);
                    }
                }
            }
        }