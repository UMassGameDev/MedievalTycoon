extends CharacterBody3D


# NODES
@onready var model: Node3D = $Model
@onready var grid_map: GridMap = get_node("/root/Node3D/GridMap")
@onready var pathfinder = get_node("/root/Node3D/Pathfinder")
@onready var npc_spawner = get_node("/root/Node3D/NPCSpawner")

# CONSTANTS
const SPEED = 5.0

# VARIABLES
var move_dir;
var target_pos = Vector2i(0, 0); # Default target position used when no empty chair is found
var last_updated_time = 0;
var target_reached: bool = false;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	_find_chair()

# Find a random chair to pathfind to
func _find_chair():
	# Find empty chairs in the room
	var chair_candidates: Array = [];
	for z in range(-5, 5):
		for x in range(-6, 6):
			if grid_map.get_cell_item(Vector3i(x, 0, z)) == 0:
				chair_candidates.append([x, z])
	
	if len(chair_candidates) > 0:
		# Pick a random chair and set it to target_pos
		var chair = chair_candidates.pick_random()
		target_pos = Vector2i(chair[0], chair[1])

func _update_movement(delta: float):
	# Add the gravity.
	if not is_on_floor() and not target_reached:
		velocity += get_gravity() * delta
	
	# Pathfinding start position
	var npc_block_pos: Vector3i = round((model.global_position + Vector3(-0.5, 0, -0.5)) / 1.6)
	
	# Pick random pathfinding end position every 10 seconds
	#if Time.get_ticks_msec() - last_updated_time > 10000:
	#	target_pos = Vector2i(randi_range(-6, 5), randi_range(-5, 4))
	#	last_updated_time = Time.get_ticks_msec()
	
	# Calculate path
	var path = pathfinder.FindPath(Vector2i(npc_block_pos.x, npc_block_pos.z), target_pos);

	move_dir = Vector2i(0, 0);
	if path != null:
		if path.size() > 0:
			# Follow path
			move_dir = Vector2(path[0]) - Vector2(model.global_position.x - 0.5, model.global_position.z - 0.5) / 1.6
		elif !target_reached:
			# Target has been reached
			target_reached = true
			# Fill chair
			grid_map.set_cell_item(Vector3i(target_pos[0], 0, target_pos[1]), 2, 0)
			# Spawn new NPC
			npc_spawner.max_npcs += 1
	
	# Prevent NPC from going outside
	# This is now done by Pathfinder.cs
	#if (model.global_position.x < -10 and move_dir.x < 0):
	#	move_dir = Vector2i(0, 0);
	
	# Move npc using move_dir
	var direction := (transform.basis * Vector3(move_dir.x, 0, move_dir.y)).normalized()
	if direction:
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
		# Rotate npc
		var target_rotation_y = atan2(move_dir.x, move_dir.y)
		model.rotation.y = lerp_angle(model.rotation.y, target_rotation_y, delta * 25.0)
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)
		velocity.z = move_toward(velocity.z, 0, SPEED)

	move_and_slide()

func _physics_process(delta: float) -> void:
	_update_movement(delta)
