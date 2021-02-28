using Godot;
using System;
public class VehicleEngine
{
    private float rpmReadonly;
    public float RPM { get { return rpmReadonly; } }

    public float ClampingReverse(float value, float m_MaxReverseInput)
    {
        return Mathf.Clamp(value, m_MaxReverseInput, 1);
    }
    public void FixedRun(ref Ackermann ackermann, float maxTorque, float value, float idleRpm)
    {
        ApplyMotorTorque(ref ackermann, maxTorque, value);
        CalculateEngineRPM(ref ackermann, idleRpm);
    }



    void ApplyMotorTorque(ref Ackermann ackermann, float maxTorque, float value)
    {
        float fs, fp, rs, rp;

        // If we have Ackerman steering, we apply torque based on the steering radius of each wheel
        var radii = AckermannUtils.GetRadii(ackermann.angle, ackermann.AxleSeparation, ackermann.AxleWidth);
        var total = radii[0] + radii[1] + radii[2] + radii[3];
        fp = radii[0] / total;
        fs = radii[1] / total;
        rp = radii[2] / total;
        rs = radii[3] / total;

        if (ackermann.angle > 0)
        {
            ackermann.m_FrontRight.motorTorque = value * maxTorque * fp;
            ackermann.m_FrontLeft.motorTorque = value * maxTorque * fs;
            ackermann.m_RearRight.motorTorque = value * maxTorque * rp;
            ackermann.m_RearLeft.motorTorque = value * maxTorque * rs;
        }
        else
        {
            ackermann.m_FrontLeft.motorTorque = value * maxTorque * fp;
            ackermann.m_FrontRight.motorTorque = value * maxTorque * fs;
            ackermann.m_RearLeft.motorTorque = value * maxTorque * rp;
            ackermann.m_RearRight.motorTorque = value * maxTorque * rs;
        }
    }

    void CalculateEngineRPM(ref Ackermann ackermann, float idleRpm)
    {
        var sum = ackermann.m_FrontLeft.rpm;
        sum += ackermann.m_FrontRight.rpm;
        sum += ackermann.m_RearLeft.rpm;
        sum += ackermann.m_RearRight.rpm;

        rpmReadonly = idleRpm + sum;
    }
}