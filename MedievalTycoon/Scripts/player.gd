extends CharacterBody3D


# NODES
@onready var model: Node3D = $PlayerModel
@onready var grid_map: GridMap = get_node("/root/Node3D/GridMap")

# EXPORT VARIABLES
@export var isCarryingFood := false #If the player is holding food

# State (mainly used for animations)
@export_group("State")
@export var state = "idle"

#Animation variables
@onready var animation_player: AnimationPlayer = $AnimationPlayer
var last_anim_played := "" #Stores the last played animation that isn't empty so that non-looping animations don't loop

# CONSTANTS
const SPEED = 5.0


func _handle_place_input():
	if Input.is_action_just_pressed("Place"):
		# Rounded position of the player
		var player_block_pos: Vector3i = round((model.global_position + Vector3(-0.5, 0, -0.5)) / 1.6)
		
		# Relative position to place/break a block
		# It is in the direction the player is facing
		var block_place_offset := Vector3i(round(sin(model.rotation.y)), 1, round(cos(model.rotation.y)))

		# Position to place/break the block
		var block_place_position := player_block_pos + block_place_offset
		
		# If the player is facing a block, break it
		# Otherwise, place a block
		if grid_map.get_cell_item(block_place_position) != -1:
			grid_map.set_cell_item(block_place_position, -1, 0)
		else:
			grid_map.set_cell_item(block_place_position, 1, 0)
		
func _update_movement(delta: float):
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

func _physics_process(delta: float) -> void:
	print(isCarryingFood)
	_handle_place_input()
	_update_movement(delta)


## Animation stuff
# Plays an animation based on the current state of the player
func _animation_controller() -> void:
	
	#Gets the animation that's being played right now
	if animation_player.current_animation != "": #If the animation isn't empty / if the animation didn't end
		last_anim_played = animation_player.current_animation #Update the animation to the current animation that's playing
	
	#IDLE ANIM
	if state == "idle":
		animation_player.play("UNFINISHEDAnimLibrary/IdleAnim")
	
	#WALKING ANIM	#TODO: FINISH
	elif state == "walking" and not (last_anim_played == "UNFINISHEDAnimLibrary/WalkAnim"):
		animation_player.play("UNFINISHEDAnimLibrary/WalkAnim")
