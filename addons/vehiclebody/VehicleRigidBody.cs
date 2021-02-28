using System.Data.SqlTypes;
using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class VehicleRigidBody : RigidBody
{
    [Export]
    public float maxTorque = 10000;

    [Export]
    public float maxTorqueMultiplier = 0;

    [Export]
    public float m_MaxReverseInput = -.5f;


    [Export]
    public float steeringRange = 35;

    [Export]
    public Vector3 CenterOfMass = new Vector3(0f, -0.2f, 0.09f);

    [Export]
    public float steeringValue;

    [Export]
    public float steeringRate = 45;

    [Export]
    public float idleRpm = 600;

    [Export]
    public NodePath FrontRightWheelPath;

    [Export]
    public NodePath FrontLeftWheelPath;

    [Export]
    public NodePath RearRightWheelPath;

    [Export]
    public NodePath RearLeftWheelPath;

    public Ackermann ackermann = new Ackermann();
    public VehicleEngine engine = new VehicleEngine();
    public VehicleSteering steering = new VehicleSteering();

    public override void _Ready()
    {
        if (Engine.EditorHint)
            return;

        ackermann.m_FrontRight = GetNode<VehicleRigidWheel>(FrontRightWheelPath);
        ackermann.m_FrontLeft = GetNode<VehicleRigidWheel>(FrontLeftWheelPath);
        ackermann.m_RearRight = GetNode<VehicleRigidWheel>(RearRightWheelPath);
        ackermann.m_RearLeft = GetNode<VehicleRigidWheel>(RearLeftWheelPath);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState state)
    {
        if (Engine.EditorHint)
            return;

        state.AddCentralForce(state.TotalGravity * Mass);

        while (forces.Count > 0)
        {
            var force = forces.Dequeue();

            state.AddForce(force.force, force.position);
            GetViewport().GetNode<Node>("DebugDraw").Call("draw_box", force.position, new Vector3(0.2f, 0.2f, 0.2f), Colors.Red);

        }
    }

    public void reset()
    {
        LinearVelocity = new Vector3();
        AngularVelocity = new Vector3();
    }

    public Queue<VehicleForce> forces = new Queue<VehicleForce>();

    public void AddForceAtPosition(Vector3 force, Vector3 pos)
    {
        forces.Enqueue(new VehicleForce { force = force, position = pos });
    }

    public override void _Process(float delta)
    {

        if (Engine.EditorHint)
            return;

        //set engine force
        maxTorqueMultiplier = Input.GetActionStrength("move_forward") - Input.GetActionStrength("move_backward");

        //set steering force
        steeringValue = Input.GetActionStrength("move_left") - Input.GetActionStrength("move_right");

        ackermann.Run(delta);
        maxTorqueMultiplier = engine.ClampingReverse(maxTorqueMultiplier, m_MaxReverseInput);
        steering.Run(ref ackermann, steeringValue, steeringRange, steeringRate, delta);


        GetViewport().GetNode<Node>("DebugDraw").Call("set_text", "maxTorque", maxTorque);
        GetViewport().GetNode<Node>("DebugDraw").Call("set_text", "maxTorqueMultiplier", maxTorqueMultiplier);

    }


    public override void _PhysicsProcess(float delta)
    {

        if (Engine.EditorHint)
            return;

        ForceUpdateTransform();

        engine.FixedRun(ref ackermann, maxTorque, maxTorqueMultiplier, idleRpm);
    }

    public Vector3 GetPointVelocity(Vector3 point)
    {
        var p = point - GlobalTransform.origin;
        var v = AngularVelocity.Cross(p);
        v = ToGlobal(v);
        v += LinearVelocity;

        return ToLocal(v);
    }

    public float Angle
    {
        get { return steeringRange * steeringValue; }
        set
        {
            this.steeringValue = value / steeringRange;
        }
    }
}


