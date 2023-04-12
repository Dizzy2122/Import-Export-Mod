// WarehouseInteriorManager.cs
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;


namespace ImportExportModNamespace
{
    public class WarehouseInteriorManager
    {
        public Vector3 ExitDoorPosition
        {
            get { return exitDoorPosition; }
        }

        private Vector3 exitDoorPosition;
        public int WarehouseInteriorID { get; private set; }
        // Declare properties and variables here, such as warehouse interior IPLs, prop lists, etc.
        private readonly List<string> warehouseIPLs = new List<string>
        {
            "imp_impexp_interior_placement_interior_1_impexp_intwaremed_milo_",
            //"imp_dt1_02_cargarage_a",
            //"imp_dt1_02_cargarage_c",
            //"imp_dt1_02_cargarage_b",
            //"prop_garage_decor_01",
            //"prop_garage_decor_02",
            //"prop_garage_decor_03",
            //"prop_garage_decor_04",
            //"prop_lighting_option01",
            //"prop_floor_vinyl_19",
            //"prop_urban_style_set",
            //"prop_car_floor_hatch",
            //"prop_door_blocker"
        };

        public WarehouseInteriorManager()
        {
            // Initialize properties and variables, load IPLs, etc.
            // LoadIPLs();
        }

        public void EnterWarehouse(Warehouse warehouse)
        {
            // Load the appropriate IPL and props based on the given warehouse
            LoadIPL("imp_impexp_interior_placement_interior_1_impexp_intwaremed_milo_");

            // Teleport the player into the warehouse interior
            Vector3 spawnPosition = new Vector3(971.0764f, -2988.314f, -39.6470f);
            float spawnHeading = 180.0f;
            Game.Player.Character.Position = spawnPosition;
            Game.Player.Character.Heading = spawnHeading;

            // Force the game to load all objects at the spawn position
            Function.Call(Hash.LOAD_ALL_OBJECTS_NOW, spawnPosition.X, spawnPosition.Y, spawnPosition.Z);

            // Set warehouse interior ID
            int interiorID = Function.Call<int>(Hash.GET_INTERIOR_AT_COORDS, spawnPosition.X, spawnPosition.Y, spawnPosition.Z);
            WarehouseInteriorID = interiorID;
            
            // Set exit door position
            exitDoorPosition = spawnPosition;
        }




        // Other methods ...

        private void LoadIPLs()
        {
            foreach (string ipl in warehouseIPLs)
            {
                if (ipl.StartsWith("prop_"))
                {
                    LoadProp(ipl);
                }
                else
                {
                    LoadIPL(ipl);
                }
            // Load the given IPL using Function.Call(Hash.REQUEST_IPL, iplName);
            Function.Call(Hash.REQUEST_IPL, ipl);
            GTA.UI.Notification.Show($"Requested IPL: {ipl}");
            }
        }

        private void LoadProp(string propName)
        {
            // Load the given prop and place it in the warehouse interior
            // You'll need to replace this with the actual implementation for loading props in your mod
        }


        private void LoadIPL(string iplName)
        {
            // Load the given IPL using Function.Call(Hash.REQUEST_IPL, iplName);
            Function.Call(Hash.REQUEST_IPL, iplName);
        }

        public bool IsPlayerInsideWarehouse()
        {
            if (WarehouseInteriorID == 0)
            {
                return false;
            }

            int currentInteriorID = Function.Call<int>(Hash.GET_INTERIOR_FROM_ENTITY, Game.Player.Character);
            return currentInteriorID == WarehouseInteriorID;
        }
    }
}
