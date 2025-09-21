using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using UnityEngine;

namespace Shiv.Utilities
{
    public static class PlayerDetection
    {
        public static Player? FindPlayerInCone(Player searcher, float maxDistance, float coneAngle)
        {
            if (searcher.CameraTransform == null) return null;
            
            Vector3 searcherPosition = searcher.CameraTransform.position;
            Vector3 searcherForward = searcher.CameraTransform.forward;
            
            Player? closestPlayer = null;
            float closestDistance = float.MaxValue;
            
            if (Plugin.Instance.Config.Debug)
            {
                Log.Info($"Searching for players from {searcher.Nickname} at position {searcherPosition}, forward: {searcherForward}");
            }
            
            foreach (var target in Player.List)
            {
                if (target == searcher || !target.IsAlive) continue;
                
                Vector3 directionToTarget = (target.Position - searcherPosition).normalized;
                float distance = Vector3.Distance(searcherPosition, target.Position);
                
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Checking player {target.Nickname} at distance {distance:F2}m");
                }
                
                if (distance > maxDistance) continue;
                
                float dot = Vector3.Dot(searcherForward, directionToTarget);
                float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
                
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Player {target.Nickname}: Distance={distance:F2}m, Angle={angle:F1}°, Dot={dot:F3}");
                }
                
                if (angle <= coneAngle / 2f)
                {
                    Vector3[] rayDirections = GetRayDirections(searcher, directionToTarget);
                    bool foundTarget = false;
                    
                    foreach (Vector3 rayDirection in rayDirections)
                    {
                        if (Physics.Raycast(searcherPosition, rayDirection, out RaycastHit hit, distance))
                        {
                            if (Plugin.Instance.Config.Debug)
                            {
                                Log.Info($"Raycast hit: {hit.collider.name} at distance {hit.distance:F2}m");
                            }
                            
                            if (IsPlayerCollider(hit.collider, target))
                            {
                                foundTarget = true;
                                if (Plugin.Instance.Config.Debug)
                                {
                                    Log.Info($"Found player {target.Nickname} through raycast at distance {distance:F2}m (Hit: {hit.collider.name})");
                                }
                                break;
                            }
                        }
                    }
                    
                    if (!foundTarget)
                    {
                        if (!Physics.Raycast(searcherPosition, directionToTarget, distance))
                        {
                            foundTarget = true;
                            if (Plugin.Instance.Config.Debug)
                            {
                                Log.Info($"Found player {target.Nickname} with direct line of sight at distance {distance:F2}m");
                            }
                        }
                    }
                    
                    if (foundTarget && distance < closestDistance)
                    {
                        closestPlayer = target;
                        closestDistance = distance;
                    }
                }
            }
            
            if (Plugin.Instance.Config.Debug)
            {
                if (closestPlayer != null)
                {
                    Log.Info($"Final target: {closestPlayer.Nickname} at distance {closestDistance:F2}m");
                }
                else
                {
                    Log.Info("No players found in search cone");
                }
            }
            
            return closestPlayer;
        }
        
        private static Vector3[] GetRayDirections(Player searcher, Vector3 directionToTarget)
        {
            if (!Plugin.Instance.Config.EnhancedWallDetection)
            {
                return new Vector3[] { directionToTarget };
            }
            
            var directions = new List<Vector3> { directionToTarget };
            
            int maxDirections = Math.Min(Plugin.Instance.Config.MaxRayDirections, 5);
            
            if (maxDirections >= 2)
            {
                directions.Add((directionToTarget + searcher.CameraTransform.right * 0.1f).normalized);
            }
            if (maxDirections >= 3)
            {
                directions.Add((directionToTarget - searcher.CameraTransform.right * 0.1f).normalized);
            }
            if (maxDirections >= 4)
            {
                directions.Add((directionToTarget + searcher.CameraTransform.up * 0.1f).normalized);
            }
            if (maxDirections >= 5)
            {
                directions.Add((directionToTarget - searcher.CameraTransform.up * 0.1f).normalized);
            }
            
            return directions.ToArray();
        }
        
        private static bool IsPlayerCollider(Collider hitCollider, Player targetPlayer)
        {
            if (hitCollider.gameObject == targetPlayer.GameObject || 
                hitCollider.gameObject == targetPlayer.ReferenceHub.gameObject)
            {
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Direct GameObject match: {hitCollider.name}");
                }
                return true;
            }
            
            if (hitCollider.transform.IsChildOf(targetPlayer.Transform) || 
                hitCollider.transform.IsChildOf(targetPlayer.ReferenceHub.transform))
            {
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Child of player Transform: {hitCollider.name}");
                }
                return true;
            }
            
            if (hitCollider.transform.root == targetPlayer.Transform.root)
            {
                if (Plugin.Instance.Config.Debug)
                {
                    Log.Info($"Same root as player: {hitCollider.name} (Root: {hitCollider.transform.root.name})");
                }
                return true;
            }
            
            Transform current = hitCollider.transform;
            while (current != null)
            {
                if (current.gameObject == targetPlayer.GameObject || 
                    current.gameObject == targetPlayer.ReferenceHub.gameObject)
                {
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"Found in parent hierarchy: {hitCollider.name} -> {current.name}");
                    }
                    return true;
                }
                current = current.parent;
            }
            
            string colliderName = hitCollider.name.ToLower();
            if (colliderName.Contains("head") || colliderName.Contains("neck") || 
                colliderName.Contains("body") || colliderName.Contains("chest") ||
                colliderName.Contains("arm") || colliderName.Contains("leg") ||
                colliderName.Contains("hand") || colliderName.Contains("foot"))
            {
                float distanceToPlayer = Vector3.Distance(hitCollider.transform.position, targetPlayer.Position);
                if (distanceToPlayer < 2.0f)
                {
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"Body part detected: {hitCollider.name} at distance {distanceToPlayer:F2}m from player");
                    }
                    return true;
                }
            }
            
            if (hitCollider.gameObject.layer == targetPlayer.GameObject.layer)
            {
                float distanceToPlayer = Vector3.Distance(hitCollider.transform.position, targetPlayer.Position);
                if (distanceToPlayer < 1.5f)
                {
                    if (Plugin.Instance.Config.Debug)
                    {
                        Log.Info($"Same layer and close: {hitCollider.name} at distance {distanceToPlayer:F2}m from player");
                    }
                    return true;
                }
            }
            
            if (Plugin.Instance.Config.Debug)
            {
                Log.Info($"No match found for collider: {hitCollider.name} (Layer: {hitCollider.gameObject.layer})");
            }
            
            return false;
        }
    }
}
