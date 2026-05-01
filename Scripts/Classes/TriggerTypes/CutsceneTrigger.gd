# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name CutsceneTrigger
extends Trigger


@export var actor:CutsceneControllable
@export_group("Conditions")
@export var require_condition:bool = false
@export var conditional_flag:Statics.WorldFlags = Statics.WorldFlags.DEFEATED_BOSS1
@export var conditional_value:Variant
@export_enum("==", "<", "<=", ">", ">=", "!=") var compare_mode:int = 0


func _on_player_enter(_body:Node2D) -> void:
	var can_activate:bool = true
	if require_condition:
		var flag_state:Variant = Statics.get_world_flag(conditional_flag)
		match compare_mode:
			0: can_activate = flag_state == conditional_value
			1: can_activate = flag_state < conditional_value
			2: can_activate = flag_state <= conditional_value
			3: can_activate = flag_state > conditional_value
			4: can_activate = flag_state >= conditional_value
			5: can_activate = flag_state != conditional_value
	if active and actor and can_activate:
		GameCore.instance.current_room.start_cutscene(actor)
	super(_body)
