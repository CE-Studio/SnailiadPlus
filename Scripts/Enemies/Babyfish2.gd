# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name Babyfish2
extends Enemy


#region Variables
const MOVE_TIMEOUT:float = 1.7
const SPEED:float = 80.0
const DECEL:float = SPEED * 0.6

var elapsed:float = 0.0
var move_timeout:float = 0.0
var facing_left:bool = false
var last_velocity:float = 0.0
#endregion

func _ready() -> void:
	my_type = EnemyTypes.BABYFISH
	super.spawn()
	
	if not display_mode:
		move_timeout = MOVE_TIMEOUT * 0.25
		facing_left = randf() < 0.5
		play_anim("idle")


func configure_display_mode(_credits:bool = false, _data:int = 0) -> void:
	elapsed = randf_range(0, TAU)
	sprite.play("display")
	sprite.flip_h = randf() < 0.5


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
	var real = self
	if real is CharacterBody2D:
		elapsed += delta
		position.y = origin.y + 4 * sin(elapsed * (1.86 if display_mode else 2.0))
		if vis.is_on_screen() and not display_mode:
			move_timeout -= delta
			if move_timeout <= 0:
				facing_left = GameCore.instance.player.position.x < position.x
				if randf() > 0.8:
					facing_left = not facing_left
				real.velocity.x = SPEED * (-1 if facing_left else 1)
				move_timeout = MOVE_TIMEOUT
				play_anim("swim")
			real.move_and_slide()
			if real.is_on_wall():
				real.velocity.x = -last_velocity
				facing_left = not facing_left
				play_anim("swim")
			real.velocity.x -= DECEL * delta * (-1 if real.velocity.x < 0 else 1)
			last_velocity = real.velocity.x


func play_anim(state:String) -> void:
	sprite.play(state + ("_left" if facing_left else "_right"))
	sprite.flip_h = facing_left
