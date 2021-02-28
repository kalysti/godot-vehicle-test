
using System;
using Godot;

public static class AckermannUtils
{
    /// <summary>
    /// Returns current turning radius of secondary wheel
    /// </summary>
    /// <param name="primaryAngle">Angle the inner front wheel is making</param>
    /// <param name="separation">Distance between front and rear wheels</param>
    /// <param name="width">Distance between left and right wheels</param>
    /// <returns></returns>
    public static float GetSecondaryAngle(float primaryAngle, float separation, float width)
    {
        // To avoid NaN assume primary angle isn't within [-1,1]
        if (Mathf.Abs(primaryAngle) < 1)
            primaryAngle = Mathf.Abs(primaryAngle);
        float close = separation / Mathf.Tan(Mathf.Deg2Rad(Mathf.Abs(primaryAngle)));
        float far = close + width;
        return Mathf.Rad2Deg( Mathf.Sign(primaryAngle) * Mathf.Atan(separation / far));
    }

    /// <summary>
    /// Returns the current turning redius of each wheel
    /// </summary>
    /// <param name="primaryAngle">Primary front angle. IE. if turning right, angle of front right wheel</param>
    /// <param name="separation">Distance between front and rear wheels</param>
    /// <param name="width">Distance between left and right wheels</param>
    /// <returns></returns>
    public static float[] GetRadii(float primaryAngle, float separation, float width)
    {
        // To avoid NaN we assume primaryAngle to be at least 1
        primaryAngle = Mathf.Clamp(primaryAngle, 1, Mathf.Inf);

        var frontPrimary = separation / Mathf.Sin(Mathf.Deg2Rad(primaryAngle));
        var rearPrimary = separation / Mathf.Tan(Mathf.Deg2Rad(primaryAngle));
        var rearSecondary = width + rearPrimary;
        var frontSecondary = Mathf.Sqrt(separation * separation + rearSecondary * rearSecondary);

        return new[] {
                frontPrimary,
                frontSecondary,
                rearPrimary,
                rearSecondary
            };
    }

    /// <summary>
    /// Returns current average turning radius of the wheels
    /// </summary>
    /// <param name="primaryAngle">Primary front angle. IE. if turning right, angle of front right wheel</param>
    /// <param name="separation">Distance between front and rear wheels</param>
    /// <param name="width">Distance between left and right wheels</param>
    /// <returns></returns>
    public static float GetRadius(float primaryAngle, float separation, float width)
    {
        // To avoid NaN we assume primaryAngle to be at least 1
        primaryAngle = Mathf.Clamp(primaryAngle, 1, Mathf.Inf);
        var radii = GetRadii(primaryAngle, separation, width);
        float sum = 0;

        for (int i = 0; i < radii.Length; i++)
            sum += radii[i];

        return sum / 4;
    }
}