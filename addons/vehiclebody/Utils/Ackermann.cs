using Godot;
using System;
public class Ackermann
{

    public VehicleRigidWheel m_FrontRight;

    public VehicleRigidWheel m_FrontLeft;

    public VehicleRigidWheel m_RearRight;

    public VehicleRigidWheel m_RearLeft;

    public float angle;


    public float AxleSeparation
    {
        get { return (m_FrontLeft.GlobalTransform.origin - m_RearLeft.GlobalTransform.origin).Length(); }
    }

    public float AxleWidth
    {
        get { return (m_FrontLeft.GlobalTransform.origin - m_FrontRight.GlobalTransform.origin).Length(); }
    }

    public float FrontRightRadius
    {
        get { return AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontRight.steerAngle)); }
    }

    public float FrontLeftRadius
    {
        get { return AxleSeparation / Mathf.Sin(Mathf.Abs(m_FrontLeft.steerAngle)); }
    }

    public void Run(float delta)
    {
        var farAngle = AckermannUtils.GetSecondaryAngle(angle, AxleSeparation, AxleWidth);

        // The rear wheels are always at 0 steer in Ackermann
        m_RearLeft.steerAngle = m_RearRight.steerAngle = 0;

        if (Mathf.IsZeroApprox(angle))
            m_FrontRight.steerAngle = m_FrontLeft.steerAngle = 0;

        m_FrontLeft.steerAngle = angle;
        m_FrontRight.steerAngle = farAngle;
    }

    public Vector3 GetPivot()
    {
        if (angle > 0)
            return m_RearRight.GlobalTransform.origin + CurrentRadii[0] * m_RearRight.GlobalTransform.basis.x;
        else
            return m_RearLeft.GlobalTransform.origin - CurrentRadii[0] * m_RearLeft.GlobalTransform.basis.x;
    }

    public float[] CurrentRadii
    {
        get { return AckermannUtils.GetRadii(angle, AxleSeparation, AxleWidth); }
    }
}


