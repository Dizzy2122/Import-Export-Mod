// LaptopMenu.cs
using GTA;
using System;
using NativeUI;



namespace ImportExportModNamespace
{
    public class LaptopMenu
    {
        private UIMenu laptopMenu;

        // Declare menu items as instance variables
        private UIMenuItem sourceVehicleItem;
        private UIMenuItem sellWarehouseVehicleItem;
        private UIMenuItem customizeWarehouseItem;
        private CarSourceManager carSourceManager;
        public Action TeleportToWarehouseExterior { get; set; }
        public UIMenu Menu
    {
        get { return laptopMenu; }
    }


        // Declare menuPool as an instance variable
        private MenuPool menuPool;

        public LaptopMenu(CarSourceManager carSourceManager)
        {
            this.carSourceManager = carSourceManager;
            menuPool = new MenuPool();
            laptopMenu = new UIMenu("Laptop Menu", "");
            menuPool.Add(laptopMenu);

            // Initialize menu items
            sourceVehicleItem = new UIMenuItem("Source a Vehicle");
            sellWarehouseVehicleItem = new UIMenuItem("Sell Warehouse Vehicle");
            customizeWarehouseItem = new UIMenuItem("Customize Warehouse");

            laptopMenu.AddItem(sourceVehicleItem);
            laptopMenu.AddItem(sellWarehouseVehicleItem);
            laptopMenu.AddItem(customizeWarehouseItem);

            // Bind events
            laptopMenu.OnItemSelect += LaptopMenu_OnItemSelect;

            laptopMenu.RefreshIndex();
            
        }


        private void OnTick(object sender, EventArgs e)
    {
        // Update the 'Source a Vehicle' button state
        sourceVehicleItem.Enabled = !carSourceManager.IsMissionActive;
    }




        // Add the Process method
        public void Process()
        {
            menuPool.ProcessMenus();
        }

        private void LaptopMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            // Handle item selection
            if (selectedItem == sourceVehicleItem)
            {
                carSourceManager.StartCarSourcingMission();
                TeleportToWarehouseExterior?.Invoke();
                sender.Visible = false;
            }
            else if (selectedItem == sellWarehouseVehicleItem)
            {
                // Code for selling a warehouse vehicle
            }
            else if (selectedItem == customizeWarehouseItem)
            {
                // Code for customizing the warehouse
            }
        }
    }
}