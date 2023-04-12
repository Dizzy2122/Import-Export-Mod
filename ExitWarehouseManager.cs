// ExitWarehouseManager.cs
using GTA;
using GTA.Math;
using NativeUI;

namespace ImportExportModNamespace
{
    public class ExitWarehouseManager
    {
        private WarehouseManager warehouseManager;
        private MenuPool menuPool;
        private UIMenu exitMenu;

        public ExitWarehouseManager(WarehouseManager warehouseManager)
        {
            this.warehouseManager = warehouseManager;
            menuPool = new MenuPool();
            SetupExitMenu();
        }

        public void OnTick(WarehouseInteriorManager warehouseInteriorManager)
        {
            menuPool.ProcessMenus();

            if (warehouseInteriorManager.IsPlayerInsideWarehouse())
            {
                float distanceToExitDoor = Game.Player.Character.Position.DistanceTo(warehouseInteriorManager.ExitDoorPosition);

                if (distanceToExitDoor < 2f)
                {
                    if (!exitMenu.Visible)
                    {
                        exitMenu.Visible = true;
                    }
                }
                else
                {
                    if (exitMenu.Visible)
                    {
                        exitMenu.Visible = false;
                    }
                }
            }
        }





        private void SetupExitMenu()
        {
            exitMenu = new UIMenu("Exit Warehouse", "OPTIONS");
            menuPool.Add(exitMenu);

            UIMenuItem exitItem = new UIMenuItem("Leave Warehouse");
            exitMenu.AddItem(exitItem);

            exitMenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == exitItem)
                {
                    exitMenu.Visible = false;

                    Vector3 exitPosition = warehouseManager.OwnedWarehouseLocation + new Vector3(0, 0, 1); // Use the owned warehouse location as the exit position
                    float exitHeading = 0; // You can set a specific heading value if needed

                    Game.Player.Character.Position = exitPosition;
                    Game.Player.Character.Heading = exitHeading;
                }
            };

            exitMenu.RefreshIndex();
        }
    }
}

