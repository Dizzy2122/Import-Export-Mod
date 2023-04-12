// Vector3Extensions.cs
using GTA.Math;

public static class Vector3Extensions
{
    public static bool IsInRangeOfBox(this Vector3 point, Vector3 min, Vector3 max)
    {
        return point.X >= min.X && point.X <= max.X
            && point.Y >= min.Y && point.Y <= max.Y
            && point.Z >= min.Z && point.Z <= max.Z;
    }
}
