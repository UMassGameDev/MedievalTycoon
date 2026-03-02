extends CharacterBody3D


# NODES
@onready var model: Node3D = $Model
@onready var grid_map: GridMap = get_node("/root/Node3D/GridMap")

# CONSTANTS
const SPEED = 5.0

# VARIABLES
var move_dir = Vector2i(0, 0);
var last_updated_time = 0;

func _update_movement(delta: float):
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta

	# Pick a random movement direction and handle the movement/deceleration.
	if Time.get_ticks_msec() - last_updated_time > 1000:
		move_dir = [Vector2i(1, 0), Vector2i(0, 1), Vector2i(-1, 0), Vector2i(0, -1), Vector2i(0, 0)].pick_random()
		last_updated_time = Time.get_ticks_msec()
	if (model.global_position.x < -10 and move_dir.x < 0): # Prevent NPC from going outside
		move_dir = Vector2i(0, 0);
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
