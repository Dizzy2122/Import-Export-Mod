// ImportExportMod.cs
using GTA;
using GTA.Native;
using NativeUI;
using System.Windows.Forms;
using System;
using GTA.Math;

namespace ImportExportModNamespace
{
    
    public class ImportExportMod : Script
    {

        // Declare managers here
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



        // Declare 'menuPool' and 'warehouseMenu' as class-level variables
        private MenuPool menuPool;
        private UIMenu warehouseMenu;
        private bool wasInsideWarehouse;

        public ImportExportMod()
        {
            settingsManager = new SettingsManager("ImportExportMod.ini");

            // Initialize managers here
            settingsManager = new SettingsManager(IniFilePath);
            warehouseManager = new WarehouseManager();
            exitWarehouseManager = new ExitWarehouseManager(warehouseManager);
            warehouseOwnershipManager = new WarehouseOwnershipManager();
            warehouseInteriorManager = new WarehouseInteriorManager();
            carSourceManager = new CarSourceManager(settingsManager);
            customizationManager = new CustomizationManager();

              if (warehouseOwnershipManager.GetOwnedWarehouse() != null)
            {
                warehouseManager.SetOwnedWarehouseLocation(warehouseOwnershipManager.GetOwnedWarehouse().ToVector3());
            }

            warehouseManager.CreateBlips();

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
                float distanceToNearestWarehouse = Game.Player.Character.Position.DistanceTo(warehouseManager.NearestWarehouseLocation);

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

                    // ... (Additional logic for purchasing a warehouse, e.g., deducting money, showing a notification, etc.)

                    warehouseManager.SetOwnedWarehouseLocation(warehouseManager.NearestWarehouseLocation);
                    warehouseManager.SetOwnedWarehouseBlip(World.CreateBlip(warehouseManager.NearestWarehouseLocation));
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
            }
        }