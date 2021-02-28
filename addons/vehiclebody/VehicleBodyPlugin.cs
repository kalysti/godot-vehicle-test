
#if TOOLS

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[Tool]
public class VehicleBodyPlugin : EditorPlugin
{   
   // GizmoWheel wheel = new GizmoWheel();
    public override void _EnterTree()
    {
        var rigidbodyDataScript = GD.Load<Script>("res://addons/vehiclebody/VehicleRigidBody.cs");
        var vehicleAxleScript = GD.Load<Script>("res://addons/vehiclebody/VehicleRigidWheel.cs");

        var texture = GD.Load<Texture>("res://addons/vehiclebody/icons/wheel.png");

        AddCustomType("VehicleRigidBody", "RigidBody", rigidbodyDataScript, texture);
        AddCustomType("VehicleRigidWheel", "Spatial", vehicleAxleScript, texture);

  //      AddSpatialGizmoPlugin(wheel);
    }
    public override void _ExitTree()
    {
        RemoveCustomType("VehicleRigidBody");
        RemoveCustomType("VehicleRigidWheel");

     //   RemoveSpatialGizmoPlugin(wheel);

    }

    

}
#endif