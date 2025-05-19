class_name InteractionRaycaster extends RayCast3D

@export var interact_distance: float = 4.0

var interact_cast_result
var last_cast_position

func _ready() -> void:
	target_position = target_position.normalized() * interact_distance

func _process(delta: float) -> void:
	pass

func _physics_process(delta: float) -> void:
	cast()

func cast() -> void:
	var current_cast_result = get_collider()
	var current_cast_position = get_collision_point()

	if is_colliding():
		last_cast_position = current_cast_position
		#print(last_cast_position)

	if current_cast_result != interact_cast_result:
		if interact_cast_result and interact_cast_result.has_user_signal("unfocused"):
			interact_cast_result.emit_signal("unfocused")
		
		interact_cast_result = current_cast_result
		#print(interact_cast_result)

		if interact_cast_result:
			if interact_cast_result.has_user_signal("focused"):
				interact_cast_result.emit_signal("focused")
			
			#Globals.player.on_interact_target_changed()

		
