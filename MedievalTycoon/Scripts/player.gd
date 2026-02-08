extends CharacterBody3D


# NODES
@onready var model: Node3D = $PlayerModel


# CONSTANTS
const SPEED = 5.0


func _physics_process(delta: float) -> void:
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta

	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("Left", "Right", "Up", "Down")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		velocity.x = direction.x * SPEED
		velocity.z = direction.z * SPEED
		# Rotate player
		var target_rotation_y = atan2(input_dir.x, input_dir.y)
		model.rotation.y = lerp_angle(model.rotation.y, target_rotation_y, delta * 25.0)
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)
		velocity.z = move_toward(velocity.z, 0, SPEED)

	move_and_slide()
