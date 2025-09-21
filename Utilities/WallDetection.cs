using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using UnityEngine;

namespace Shiv.Utilities
{
    public static class WallDetection
    {
        public static bool IsLookingAtWall(Player player)
        {
            if (player.CameraTransform == null) return false;
            
            Vector3 forward = player.CameraTransform.forward;
            Vector3 origin = player.CameraTransform.position;
            
            Vector3[] rayDirections = GetRayDirections(player, forward);
            float[] raycastDistances = { 0.3f, 0.6f, 1.0f, 1.5f, Plugin.Instance.Config.WallDetectionDistance };
            
            foreach (float distance in raycastDistances)
            {
                foreach (Vector3 direction in rayDirections)
                {
                    if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
                    {
                        if (IsValidWall(hit))
                        {
                            if (Plugin.Instance.Config.Debug)
                            {
                                Log.Info($"Wall detected: {hit.collider.name} (Tag: {hit.collider.tag}, Layer: {hit.collider.gameObject.layer}, Distance: {hit.distance:F2})");
                            }
                            return true;
                        }
                    }
                }
            }
            
            if (Plugin.Instance.Config.Debug)
            {
                Log.Info($"No wall detected for {player.Nickname} within {Plugin.Instance.Config.WallDetectionDistance}m");
            }
            
            return false;
        }
        
        private static Vector3[] GetRayDirections(Player player, Vector3 forward)
        {
            if (!Plugin.Instance.Config.EnhancedWallDetection)
            {
                return new Vector3[] { forward };
            }
            
            var directions = new List<Vector3> { forward };
            
            int maxDirections = Math.Min(Plugin.Instance.Config.MaxRayDirections, 5);
            
            if (maxDirections >= 2)
            {
                directions.Add((forward + player.CameraTransform.right * 0.1f).normalized);
            }
            if (maxDirections >= 3)
            {
                directions.Add((forward - player.CameraTransform.right * 0.1f).normalized);
            }
            if (maxDirections >= 4)
            {
                directions.Add((forward + player.CameraTransform.up * 0.1f).normalized);
            }
            if (maxDirections >= 5)
            {
                directions.Add((forward - player.CameraTransform.up * 0.1f).normalized);
            }
            
            return directions.ToArray();
        }
        
        private static bool IsValidWall(RaycastHit hit)
        {
            string objectName = hit.collider.gameObject.name.ToLower();
            string objectTag = hit.collider.tag.ToLower();
            int objectLayer = hit.collider.gameObject.layer;
            
            if (objectTag == "floor" || objectTag == "ceiling" || 
                objectName.Contains("floor") || objectName.Contains("ceiling") ||
                objectName.Contains("ground") || objectName.Contains("roof"))
            {
                return false;
            }
            
            if (objectName.Contains("player") || objectName.Contains("item") || 
                objectName.Contains("pickup") || objectName.Contains("scp"))
            {
                return false;
            }
            
            if (objectLayer == 0 || objectLayer == 8 || objectLayer == 9 || objectLayer == 10)
            {
                Vector3 hitNormal = hit.normal;
                float verticalComponent = Math.Abs(Vector3.Dot(hitNormal, Vector3.up));
                
                if (verticalComponent > Plugin.Instance.Config.VerticalSurfaceThreshold)
                {
                    return false;
                }
                
                return true;
            }
            
            if (objectTag == "wall" || objectTag == "door" || objectTag == "window" || 
                objectTag == "structure")
            {
                return true;
            }
            
            if (objectName.Contains("wall") || objectName.Contains("door") || 
                objectName.Contains("window") || objectName.Contains("structure") ||
                objectName.Contains("concrete") || objectName.Contains("metal") ||
                objectName.Contains("brick") || objectName.Contains("stone"))
            {
                return true;
            }
            
            if (hit.collider.isTrigger == false && hit.collider.bounds.size.magnitude > 0.1f)
            {
                Vector3 hitNormal = hit.normal;
                float verticalComponent = Math.Abs(Vector3.Dot(hitNormal, Vector3.up));
                
                return verticalComponent < Plugin.Instance.Config.VerticalSurfaceThreshold;
            }
            
            return false;
        }
    }
}
