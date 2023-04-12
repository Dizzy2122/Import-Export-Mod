using GTA;
using GTA.Math;
using System.Collections.Generic;
using System;

public class SettingsManager
{
    private ScriptSettings settings;

    public int CarListOption { get; private set; }
    public string[] AddOnCars { get; private set; }
    private List<string> baseGameCars = new List<string> { "adder", "zentorno", "jester" };

    public SettingsManager(string iniFilePath)
    {
        settings = ScriptSettings.Load(iniFilePath);

        CarListOption = settings.GetValue("CarSpawning", "CarListOption", 1);
        AddOnCars = settings.GetValue("CarSpawning", "AddOnCars", "").Split(',');

        settings.SetValue("CarSpawning", "CarListOption", CarListOption);
        settings.SetValue("CarSpawning", "AddOnCars", string.Join(",", AddOnCars));

        settings.Save();
    }

    public string GetRandomCarModelName()
    {
        List<string> usingCarList = new List<string>();

        if (CarListOption == 1 || CarListOption == 3)
        {
            usingCarList.AddRange(baseGameCars);
        }

        if (CarListOption == 2 || CarListOption == 3)
        {
            usingCarList.AddRange(AddOnCars);
        }

        if (usingCarList.Count > 0)
        {
            int randomIndex = new Random().Next(usingCarList.Count);
            return usingCarList[randomIndex];
        }

        return null;
    }
}
