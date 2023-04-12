// WarehouseOwnershipManager.cs
using GTA.Math;
using System.IO;
using Newtonsoft.Json;

namespace ImportExportModNamespace
{
    public class WarehouseOwnershipManager
{
        private const string WarehouseOwnershipFilePath = "warehouse_ownership.json";
        private OwnedWarehouseData ownedWarehouse;

        public WarehouseOwnershipManager()
        {
            LoadOwnedWarehouse();
        }

        public void SaveOwnedWarehouse()
        {
            string json = JsonConvert.SerializeObject(ownedWarehouse, Formatting.Indented);
            File.WriteAllText(WarehouseOwnershipFilePath, json);
        }

        private void LoadOwnedWarehouse()
        {
            if (File.Exists(WarehouseOwnershipFilePath))
            {
                string json = File.ReadAllText(WarehouseOwnershipFilePath);
                ownedWarehouse = JsonConvert.DeserializeObject<OwnedWarehouseData>(json);
            }
            else
            {
                ownedWarehouse = null;
            }
        }

        public OwnedWarehouseData GetOwnedWarehouse()
        {
            return ownedWarehouse;
        }

        public void SetOwnedWarehouse(OwnedWarehouseData newOwnedWarehouse)
        {
            ownedWarehouse = newOwnedWarehouse;
        }
    }


        public class OwnedWarehouseData
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }

            public OwnedWarehouseData()
            {
            }

            public OwnedWarehouseData(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Vector3 ToVector3()
            {
                return new Vector3(X, Y, Z);
            }
        }
    }