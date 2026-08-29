# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@tool
class_name StompyFoot
extends Enemy


#region Variables
@export var left:bool = false:
	set(value):
		left = value
		$"Area2D/HitBox".scale.x = -1 if value else 1
		if Engine.is_editor_hint() and sprite:
			sprite.flip_h = value

enum FootStates {
	DOWN,
	RAISE,
	UP,
	FALL
}
var state:FootStates = FootStates.UP

var boss:Stompy
#endregion


func _ready() -> void:
	if Engine.is_editor_hint():
		return
	
	my_type = EnemyTypes.NONE
	super.spawn()
	
	play_phase_anim(0, "up")


func set_state(_state:FootStates) -> void:
	state = _state
	play_phase_anim(boss.phase, FootStates.keys()[_state].to_lower())


func update_phase() -> void:
	play_phase_anim(boss.phase, FootStates.keys()[state].to_lower())


func play_phase_anim(phase:int, anim:String) -> void:
	var anim_name:String = "p%d_%s_%s" % [
		phase,
		"left" if left else "right",
		anim
	]
	sprite.play(anim_name)
	sprite.flip_h = left
