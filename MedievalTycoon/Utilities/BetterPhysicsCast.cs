using System;
using Godot;
using Godot.Collections;

public struct RaycastHit {
   public Vector3 Position;
   public Vector3 Normal;
   public Node3D Collider;
   public ulong ColliderId;
   public Rid Rid; 
   public int Shape;
}

public partial class BetterPhysicsCast : Node3D {
   private static BetterPhysicsCast _instance;
   
   private PhysicsDirectSpaceState3D _spaceState;
   public override void _Ready() {
      _instance = this; 
      
      _spaceState = GetWorld3D().DirectSpaceState;
   }
   
   public static bool Raycast(Vector3 origin, Vector3 direction, float length, Array<Rid> exclude, out RaycastHit hit) {
      var parameters = new PhysicsRayQueryParameters3D {
         From = origin,
         To = origin + direction.Normalized() * length,
         Exclude = exclude,
      };
      
      return Raycast(parameters, out hit);
   }

   
   public static bool Raycast(Vector3 origin, Vector3 direction, float length, out RaycastHit hit) {
      var parameters = new PhysicsRayQueryParameters3D {
         From = origin,
         To = origin + direction.Normalized() * length,
      };
      
      return Raycast(parameters, out hit);
   }

   public static bool Raycast(PhysicsRayQueryParameters3D parameters, out RaycastHit hit) {
      if (_instance == null) { throw new InvalidOperationException("BetterPhysicsCast instance is not initialized. Make sure a BetterPhysicsCast node is present in the scene tree."); }
      return _instance._Raycast(parameters, out hit);
   }
   
   /// <summary>
   /// Internal raycast method
   /// </summary>
   /// <param name="parameters"> the raycast parameters </param>
   /// <param name="hit"> the hit result, if any </param>
   /// <returns> true if something was hit, false otherwise </returns>
   private bool _Raycast(PhysicsRayQueryParameters3D parameters, out RaycastHit hit) {
      Dictionary result = _spaceState.IntersectRay(parameters);
      
      if (result.Count == 0) {
         hit = default;
         return false;
      }
      
      hit = new RaycastHit {
         Position = (Vector3)result["position"],
         Normal = (Vector3)result["normal"],
         Collider = (Node3D)result["collider"],
         ColliderId = (ulong)result["collider_id"],
         Rid = (Rid)result["rid"],
         Shape = (int)result["shape"],
      };
      
      return true;
   }
}