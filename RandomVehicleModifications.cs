// RandomVehicleModifications.cs
using GTA;
using GTA.Native;
using System;

namespace ImportExportModNamespace
{
    public class RandomVehicleModifications
    {
        private Vehicle _missionCar;

        public RandomVehicleModifications(Vehicle missionCar)
        {
            _missionCar = missionCar;
        }

        public void ApplyAll()
        {
            ApplyRandomBaseModifications();

            // Wait for the vehicle mods to load before applying the front bumper modification
            GTA.Script.Wait(1000);
            ApplyFrontBumperModification();
        }

        private void ApplyRandomBaseModifications()
        {
            Random random = new Random();
            if (random.NextDouble() <= 1.0) // change back to 0.4 (40% chance) after testing
            {
                for (int modIndex = 0; modIndex < 50; modIndex++) // Reduced the modIndex upper limit to 50
                {
                    int maxModVariation = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, _missionCar, modIndex);
                    if (maxModVariation > 0)
                    {
                        int randomMod = random.Next(maxModVariation);
                        Function.Call(Hash.SET_VEHICLE_MOD, _missionCar, modIndex, randomMod, false);
                    }
                }

                // Turbo
                Function.Call(Hash.TOGGLE_VEHICLE_MOD, _missionCar, 18, true);

                // Xenon Headlights
                Function.Call(Hash.TOGGLE_VEHICLE_MOD, _missionCar, 22, true);
            }
        }

        private void ApplyFrontBumperModification()
        {
            Random random = new Random();
            int frontBumperModIndex = 1;
            int maxFrontBumperVariation = Function.Call<int>(Hash.GET_NUM_VEHICLE_MODS, _missionCar, frontBumperModIndex);
            
            GTA.UI.Notification.Show($"Front Bumper Variations: {maxFrontBumperVariation}"); // Debug notification

            if (maxFrontBumperVariation > 0)
            {
                int randomFrontBumper = random.Next(maxFrontBumperVariation);
                
                GTA.UI.Notification.Show($"Applying Front Bumper: {randomFrontBumper}"); // Debug notification
                
                Function.Call(Hash.SET_VEHICLE_MOD, _missionCar, frontBumperModIndex, randomFrontBumper, false);
            }
        }

    }
}
