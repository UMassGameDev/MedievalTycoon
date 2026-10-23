extends Area3D

func take_food(body: Node3D):
	if body.name == "Player": #If the thingy that touched the NPC is a player
		if body.isCarryingFood == true: #If player is carrying food
			body.isCarryingFood = false #Not anymore!
			get_parent().queue_free() #Kills the NPC until we actually program them to leave

#When player touches Hitbox
func _on_body_entered(body: Node3D) -> void: #If an object touches the object
	take_food(body)
