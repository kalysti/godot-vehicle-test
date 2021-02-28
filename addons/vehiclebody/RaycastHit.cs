using Godot;
using System;

[Serializable]
public class RaycastHit 
{
    public object collider = null;
    public Vector3 point = Vector3.Zero;
    public Vector3 normal = Vector3.Zero;
    public float distance = 0f;
    public bool isColliding = false;

}
