extends Area3D

func give_food(body: Node3D):
	if body.name == "Player": #If the thingy that touched the pixel is a player
		if body.isCarryingFood == false: #If player isn't carrying food
			body.isCarryingFood = true #They are now

#When player touches Hitbox
func _on_body_entered(body: Node3D) -> void: #If an object touches the object
	give_food(body)
