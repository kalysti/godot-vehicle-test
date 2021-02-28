using Godot;
using System;

[Tool]
public class VehicleRigidWheel : Spatial
{
    /*
     * NOTE (A)
     * A car with wheels of diameter ~55cm usually has axle of diameter 2.2cm. That's a ratio of 25.
       Could not get values of real cars to verify the 55 and 2.2 cm claim but hat's what I've read somewhere 
       in a physics problem. This gets me good results. For example: A Civic has 170Nm of torque, at the first 
       gear that's about 530Nm of torque after transmission with a gear ratio of 3.1. If I set the max toque 
       in the motor at 530, I get an acceleration that I'd expect when someone in a Civic floors it.
       I'd imagine heavier vehicles have thicker axles, but their wheels are also large, so may be the 
       ratio would still hold. Or maybe it won't. Please change the value as you prefer.
     */


    // See at the top of the file for NOTE (A) to read about this.
    const float engineShaftToWheelRatio = 25;

    [Export]
    /// <summary>
    /// The radius of the wheel
    /// </summary>
    public float radius = 0.25f;

    [Export]
    /// <summary>
    /// How far the spring expands when it is in air.
    /// </summary>
    public float springLength = .25f;

    [Export]
    /// <summary>
    /// The k constact in the Hooke's law for the suspension. High values make the spring hard to compress. Make this higher for heavier vehicles
    /// </summary>
    public float springStrength = 5000;

    [Export]
    /// <summary>
    /// Damping applied to the wheel. Higher values allow the car to negotiate bumps easily. Recommended: 0.25. Values outside [0, 0.5f] are unnatural
    /// </summary>
    public float springDamper = .25f;

    [Export]
    /// <summary>
    /// The rate (m/s) at which the spring relaxes to maximum length when the wheel is not on the ground.
    /// </summary>
    public float springRelaxRate = .125f;

    [Export]
    /// <summary>
    /// Determines the friction force that enables the wheel to exert sideways force while turning.
    /// </summary>
    public float lateralFrictionCoeff = 1;

    [Export]
    /// <summary>
    /// Determines the friction force that enables the wheel to exert forward force while experiencing torque.
    /// </summary>
    public float forwardFrictionCoeff = 1;

    [Export]
    /// <summary>
    /// A constant friction % applied at all times. This allows the car to slow down on its own.
    /// </summary>
    public float rollingFrictionCoeff = .1f;

    /// <summary>
    /// The velocity of the wheel (at the raycast hit point) in world space
    /// </summary>
    public Vector3 velocity { get; private set; }

    /// <summary>
    /// The angle by which the wheel is turning
    /// </summary>
    public float steerAngle { get; set; }

    /// <summary>
    /// The torque applied to the wheel for moving in the forward and backward direction
    /// </summary>
    /// 

    public float motorTorque { get; set; }

    /// <summary>
    /// The torque the brake is applying on the wheel
    /// </summary>
    public float brakeTorque { get; set; }

    /// <summary>
    ///Revolutions per minute of the wheel
    /// </summary>
    public float rpm { get; private set; }

    /// <summary>
    /// Whether the wheel is touching the ground
    /// </summary>
    public bool isGrounded { get; private set; }

    /// <summary>
    /// The distance to which the spring of the wheel is compressed
    /// </summary>
    public float compressionDistance { get; private set; }
    float m_PrevCompressionDist;

    /// <summary>
    /// The ratio of compression distance and suspension distance
    /// 0 when the wheel is entirely uncompressed, 
    /// 1 when the wheel is entirely compressed
    /// </summary>
    public float compressionRatio { get; private set; }

    /// <summary>
    /// The raycast hit point of the wheel
    /// </summary>
    public RaycastHit hit { get { return m_Hit; } }
    RaycastHit m_Hit;

    public float sprungMass => suspensionForce.Length() / 9.81f;

    /// <summary>
    /// The force the spring exerts on the rigidbody
    /// </summary>
    public Vector3 suspensionForce { get; private set; }

    VehicleRigidBody rigidbody;

    public const float k_ExtraRayLength = 1;
    public float RayLength => springLength + radius + k_ExtraRayLength;

    protected Vector3 castDirection = Vector3.Zero;
    protected Vector3 castOrigin = Vector3.Zero;

    [Export]
    public float maxCoeff = 0.7f;
    [Export]
    public float minCoeff = 0.4f;
    [Export]
    public float softness = 10f;
    [Export]
    public float criticalSlip = 15f;

    public override void _Ready()
    {
        if (Engine.EditorHint)
            return;

        rigidbody = GetParent<VehicleRigidBody>();
    }

    public override void _Process(float delta)
    {
        if (Engine.EditorHint)
            return;
            
        lateralFrictionCoeff = EvaluateSlip(getLateralVelocity().Length());
    }


    public float EvaluateSlip(float slip)
    {
        if (slip < criticalSlip - softness / 2)
            return maxCoeff;
        else if (slip > criticalSlip + softness / 2)
            return minCoeff;
        else
        {
            float delta = slip - (criticalSlip - softness / 2);
            return minCoeff + delta * (maxCoeff - minCoeff) / softness;
        }
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Engine.EditorHint)
            return;

        ForceUpdateTransform();

        velocity = rigidbody.GetPointVelocity(GlobalTransform.origin);

        //set rotation
        var rot = RotationDegrees;
        rot.y = steerAngle;
        RotationDegrees = rot;

        CalculateSuspension(delta);
        CalculateFriction(delta);
        CalculateRPM();
    }

    void CalculateRPM()
    {
        float metersPerMinute = rigidbody.LinearVelocity.Length() * 60;
        float wheelCircumference = 2 * Mathf.Pi * radius;
        //RPM = metersPerMinute / wheelCircumference;
    }

    void CalculateSuspension(float delta)
    {
        castDirection = -GlobalTransform.basis.y;
        castOrigin = GlobalTransform.origin + GlobalTransform.basis.y * k_ExtraRayLength;

        isGrounded = WheelRaycast(RayLength, ref m_Hit);
        // If we did not hit, relax the spring and return
        if (!isGrounded)
        {
            m_PrevCompressionDist = compressionDistance;
            compressionDistance = compressionDistance - delta * springRelaxRate;
            compressionDistance = Mathf.Clamp(compressionDistance, 0, springLength);
            return;
        }

        var force = 0.0f;
        compressionDistance = RayLength - hit.distance;
        compressionDistance = Mathf.Clamp(compressionDistance, 0, springLength);
        compressionRatio = compressionDistance / springLength;

        // Calculate the force from the springs compression using Hooke's Law
        float compressionForce = springStrength * compressionRatio;
        force += compressionForce;

        // Calculate the damping force based on compression rate of the spring
        float rate = (m_PrevCompressionDist - compressionDistance) / delta;
        m_PrevCompressionDist = compressionDistance;

        float dampingForce = rate * springStrength * springDamper;
        force -= dampingForce;


        suspensionForce = GlobalTransform.basis.y * force;
        GetViewport().GetNode<Node>("DebugDraw").Call("set_text", "force", GlobalTransform.basis.y);
        GetViewport().GetNode<Node>("DebugDraw").Call("set_text", "forceFloat", force);

        //     GD.Print(suspensionForce);
        // Apply suspension force

        Vector3 hit_point = Transform.basis.Xform(hit.point - rigidbody.Transform.origin);
        rigidbody.AddForceAtPosition(suspensionForce, hit_point);
    }

    bool WheelRaycast(float maxDistance, ref RaycastHit hit)
    {
        var space = GetWorld().DirectSpaceState;
        var to = castOrigin + (castDirection.Normalized() * maxDistance);

        var exclusion = new Godot.Collections.Array();
        exclusion.Add(rigidbody.GetRid());

        var result = space.IntersectRay(castOrigin, to, exclusion);

        GetViewport().GetNode<Node>("DebugDraw").Call("draw_line_3d", castOrigin, to, Colors.Red);
        hit = new RaycastHit();

        if (result.Keys.Count > 0)
        {
            hit.isColliding = true;
            hit.normal = (Vector3)result["normal"];
            hit.point = (Vector3)result["position"];
            hit.distance = hit.point.DistanceTo(castOrigin);
            return true;
        }
        else
            return false;
    }

    public Vector3 getLateralVelocity()
    {
        var v = GlobalTransform.basis.x;
        var d = v.Dot(rigidbody.LinearVelocity) * v;

        return d;
    }

    public Vector3 getForwardVelocity()
    {
        var v = -GlobalTransform.basis.z.Normalized();
        var d = v.Dot(rigidbody.LinearVelocity) * v;

        return d;
    }
    void CalculateFriction(float delta)
    {

        if (!isGrounded) return;

        Vector3 right = GlobalTransform.basis.x;
        Vector3 forward = -GlobalTransform.basis.z.Normalized();

        Vector3 sideVelocity = velocity.Project(right);
        Vector3 forwardVelocity = velocity.Project(forward);

        Vector3 slip = (forwardVelocity + sideVelocity) / 2;

        var slipProtected = right.Project(slip); //in global coordinates
        var slipProtectedSide = slip.Project(sideVelocity); //in global coordinates

        float lateralFriction = slipProtected.Length() * suspensionForce.Length() / 9.8f / delta * lateralFrictionCoeff; //is local
        Vector3 slipFriction = -(slip.Project(sideVelocity).Normalized());

        var friction = slipFriction * lateralFriction;


        //GetViewport().GetNode<Node>("DebugDraw").Call("draw_box", rigidbody.ToGlobal(slipProtected), new Vector3(0.1f, 0.1f, 0.1f), Colors.Green);
        //  GetViewport().GetNode<Node>("DebugDraw").Call("draw_box", rigidbody.ToGlobal(slipProtectedSide), new Vector3(0.1f, 0.1f, 0.1f), Colors.Green);

        //  GetViewport().GetNode<Node>("DebugDraw").Call("set_text", "suspensionForce", suspensionForce.Length());
        //rigidbody.AddForceAtPosition(friction, hit.point);

        if (!hit.isColliding)
        {
            GD.PrintErr("fails");
        }

        float motorForce = motorTorque / radius;
        float maxForwardFriction = motorForce * forwardFrictionCoeff;
        float appliedForwardFriction = 0;
        if (motorForce > 0)
            appliedForwardFriction = Mathf.Clamp(motorForce, 0, maxForwardFriction);
        else
            appliedForwardFriction = Mathf.Clamp(motorForce, maxForwardFriction, 0);

        var engineStuff = forward * appliedForwardFriction * engineShaftToWheelRatio;



        Vector3 hit_point = Transform.basis.Xform(hit.point - rigidbody.Transform.origin);

        rigidbody.AddForceAtPosition(engineStuff, hit_point);


    }

}