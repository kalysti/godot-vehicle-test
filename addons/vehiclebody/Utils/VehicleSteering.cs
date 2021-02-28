using Godot;
using System;
public class VehicleSteering
{
    float m_CurrAngle;

    public void Run(ref Ackermann ackermann, float steeringValue, float steeringRange, float steeringRate, float delta)
    {
        var destination = steeringValue * steeringRange;
        m_CurrAngle = Mathf.MoveToward(m_CurrAngle, destination, delta * steeringRate);
        m_CurrAngle = Mathf.Clamp(m_CurrAngle, -steeringRange, steeringRange);
        ackermann.angle = m_CurrAngle;
    }

}


