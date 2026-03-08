extends Marker3D

var npc_scenes = [
	load("res://NPCs/Farmer_01.tscn"),
	load("res://NPCs/Farmer_02.tscn"),
	load("res://NPCs/Farmer_03.tscn"),
	load("res://NPCs/Hunter_01.tscn"),
	load("res://NPCs/Knight_01.tscn")
]
var spawned_npcs = []
var max_npcs: int = 2


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if spawned_npcs.size() < max_npcs:
		var new_npc = npc_scenes.pick_random().instantiate()
		spawned_npcs.append(new_npc)
		add_child(new_npc)
