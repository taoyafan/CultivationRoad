using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseUtils 
{
    public static Vector3 GetMouseWorldPosition(float zValue = 0f)
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return Vector3.zero;
        }
        
        Plane dragPlane = new(camera.transform.forward, new Vector3(0, 0, zValue));
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
