using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class GizmoWheel : EditorSpatialGizmoPlugin
{
    public GizmoWheel() : base()
    {
        CreateMaterial("front", new Color(1, 1, 1), false, false, true);
        CreateMaterial("rear", new Color(1, 1, 1), false, false, true);
        CreateHandleMaterial("handle_mat");
    }

    public override bool HasGizmo(Spatial spatial)
    {
        return spatial is VehicleRigidWheel;
    }

    public override void Redraw(EditorSpatialGizmo gizmo)
    {
        if(!Engine.EditorHint)
            return;
            
        return;

        gizmo.Clear();

        var spatial = gizmo.GetSpatialNode() as VehicleRigidWheel;
        var t = spatial.GlobalTransform;

        var p1 = t.origin + t.basis.y * VehicleRigidWheel.k_ExtraRayLength;
        var p2 = t.origin - t.basis.y * (spatial.RayLength - VehicleRigidWheel.k_ExtraRayLength);

        var lines = new List<Vector3>();
        lines.Add(spatial.ToLocal(p1));
        lines.Add(spatial.ToLocal(p2));


        int skip = 10;
        gizmo.AddLines(lines.ToArray(), GetMaterial("front", gizmo), false, Colors.Red);
        lines.Clear();

        for (int ix = 0; ix < 360; ix += skip)
        {
            var ra = Mathf.Deg2Rad(ix);
            var rb = Mathf.Deg2Rad(ix + skip);

            var a = new Vector2(Mathf.Sin(ra), Mathf.Cos(ra)) * spatial.radius;
            var b = new Vector2(Mathf.Sin(rb), Mathf.Cos(rb)) * spatial.radius;

            lines.Add(new Vector3(0, a.x, a.y));
            lines.Add(new Vector3(0, b.x, b.y));
        }


        gizmo.AddLines(lines.ToArray(), GetMaterial("front", gizmo), false, Colors.Yellow);

    
    }




}
