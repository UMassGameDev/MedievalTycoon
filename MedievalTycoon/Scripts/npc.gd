extends CharacterBody3D


# NODES
@onready var model: Node3D = $Model
@onready var grid_map: GridMap = get_node("/root/Node3D/GridMap")
@onready var pathfinder = get_node("/root/Node3D/Pathfinder")

# CONSTANTS
const SPEED = 5.0

# VARIABLES
var move_dir;
var target_pos = Vector2i(0, 0);
var last_updated_time = 0;

func _update_movement(delta: float):
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta
	
	# Pathfinding start and end position
	var npc_block_pos: Vector3i = round((model.global_position + Vector3(-0.5, 0, -0.5)) / 1.6)
	if Time.get_ticks_msec() - last_updated_time > 10000:
		target_pos = Vector2i(randi_range(-6, 5), randi_range(-5, 4))
		last_updated_time = Time.get_ticks_msec()
	
	# Calculate path
	var path = pathfinder.FindPath(Vector2i(npc_block_pos.x, npc_block_pos.z), target_pos);

	if path != null and path.size() > 0:
		# Follow path
		move_dir = Vector2(path[0]) - Vector2(model.global_position.x - 0.5, model.global_position.z - 0.5) / 1.6
	else:
		move_dir = Vector2i(0, 0);
	
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
