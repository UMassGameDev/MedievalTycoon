// Call the FindPath function to calculate a path
// using A* on y layer 1 of the GridMap where -1 is an empty cell.
// Followed: https://www.youtube.com/watch?v=i0x5fj4PqP4
// The current implementation does not seem to work on an infinite grid.

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Followed: https://www.youtube.com/watch?v=i0x5fj4PqP4
public class NodeBase {
	public NodeBase Connection { get; private set; }
	public float G { get; private set; }
	public float H { get; private set; }
	public float F => G + H;
	public Vector2I coords;
	public GridMap gridMap;
	public Dictionary<Vector2I, NodeBase> nodeCache;
	public bool walkable;
	
	public NodeBase(Vector2I coords, GridMap gridMap, Dictionary<Vector2I, NodeBase> nodeCache) {
		this.coords = coords;
		this.gridMap = gridMap;
		this.nodeCache = nodeCache;
		
		// Only inside the room is accessible
		if (coords.X < -6 || coords.X > 5 || coords.Y < -5 || coords.Y > 4) {
			// Where the door is
			if (coords.X == -7 && (coords.Y == 0 || coords.Y == -1)) {
				this.walkable = true;
			}
			// Out of bounds
			else {
				this.walkable = false;
			}
		}
		else {
			// Walkable if the tile is empty
			this.walkable = gridMap.GetCellItem(new Vector3I(coords.X, 1, coords.Y)) == -1;
		}
	}
	
	public void SetConnection(NodeBase nodeBase) => Connection = nodeBase;
	public void SetG(float g) => G = g;
	public void SetH(float h) => H = h;
	
	public List<NodeBase> GetNeighbors() {
		List<NodeBase> neighbors = new List<NodeBase>();
		
		for (int ry = -1; ry <= 1; ++ry) {
			for (int rx = -1; rx <= 1; ++rx) {
				if (rx == 0 && ry == 0) continue;
				
				Vector2I neighborCoords = coords + new Vector2I(rx, ry);
				if (!nodeCache.TryGetValue(neighborCoords, out NodeBase neighbor)) {
					neighbor = new NodeBase(neighborCoords, gridMap, nodeCache);
					nodeCache[neighborCoords] = neighbor;
				}
				
				// Add extra walkable check (skip diagonals if blocked)
				if (neighbor.walkable && rx != 0 && ry != 0) {
					if (gridMap.GetCellItem(new Vector3I(coords.X + rx, 1, coords.Y)) != -1 || gridMap.GetCellItem(new Vector3I(coords.X, 1, coords.Y + ry)) != -1) {
						continue;
					}
				}
				
				neighbors.Add(neighbor);
			}
		}
		
		return neighbors;
	}
	
	public float GetDistance(NodeBase otherNode) {
		float distance = 0;
		
		int dx = Math.Abs(otherNode.coords.X - this.coords.X);
		int dy = Math.Abs(otherNode.coords.Y - this.coords.Y); // This is actually for the z-axis
		
		if (dx < dy) {
			distance = dx * 1.414f + dy - dx;
		}
		else {
			distance = dy * 1.414f + dx - dy;
		}
		
		return distance;
	}
}

public partial class Pathfinder : Node3D {
	private GridMap gridMap;
	private Dictionary<Vector2I, NodeBase> nodeCache;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		gridMap = GetNode<GridMap>("/root/Node3D/GridMap");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	//public override void _Process(double delta) {}
	
	// Followed: https://www.youtube.com/watch?v=i0x5fj4PqP4
	public Godot.Collections.Array FindPath(Vector2I start, Vector2I target) {
		nodeCache = new Dictionary<Vector2I, NodeBase>();
		
		NodeBase startNode = new NodeBase(start, gridMap, nodeCache);
		nodeCache[start] = startNode;
		
		NodeBase targetNode = new NodeBase(target, gridMap, nodeCache);
		
		var toSearch = new List<NodeBase>() { startNode };
		var processed = new List<NodeBase>();
		
		while (toSearch.Any()) {
			var current = toSearch[0];
			foreach (var t in toSearch)
				if (t.F < current.F || t.F == current.F && t.H < current.H)
					current = t;
			
			processed.Add(current);
			toSearch.Remove(current);
			
			if (current.coords == targetNode.coords) {
				NodeBase currentPathTile = nodeCache[targetNode.coords];
				Godot.Collections.Array path = new Godot.Collections.Array();
				while (currentPathTile != startNode) {
					path.Add(currentPathTile.coords);
					currentPathTile = currentPathTile.Connection;
				}
				path.Reverse();
				return path;
			}
			
			foreach (var neighbor in current.GetNeighbors().Where(t => t.walkable && !processed.Contains(t))) {
				var inSearch = toSearch.Contains(neighbor);
				
				var costToNeighbor = current.G + current.GetDistance(neighbor);
				
				if (!inSearch || costToNeighbor < neighbor.G) {
					neighbor.SetG(costToNeighbor);
					neighbor.SetConnection(current);
					
					if (!inSearch) {
						neighbor.SetH(neighbor.GetDistance(targetNode));
						toSearch.Add(neighbor);
					}
				}
			}
		}
		
		return null;
	}
}
