@icon("res://Editor/ico/EnemyBullet.svg")
class_name EnemyBullet
extends Node2D


#region Variables
var normalized_dir:Vector2 = Vector2.ZERO
var life_timer:float = 0.0
var max_life_time:float = 1.6
var velocity:float = 0.0
var velocity_init:float = 0.0
var damage:int = 0
var rapid_mult:float = 0.0
var despawn_offscreen:bool = false
var collide_with_world:bool = false
var source_enemy:Enemy
var has_been_parried:bool = false
var intersecting_player:bool = false

var collide_with_wall:bool
var single_hit:bool

enum PBulletInteractions {
	ALWAYS_DESTROY,
	DESTROY_PARALLEL,
	DESTROY_PARALLEL_WIDE,
	DESTROY_PERPENDICULAR,
	DESTROY_PERPENDICULAR_WIDE
}

var pbullets_that_i_destroy:Array = []
var pbullets_that_destroy_me:Array = []
var pbullet_interaction:PBulletInteractions = PBulletInteractions.ALWAYS_DESTROY

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var area:Area2D = $"Area2D"
@onready var box:CollisionShape2D = $"Area2D/Box"
@onready var sfx:AudioStreamPlayer = $"AudioGroup/Shoot"
@onready var vis:VisibleOnScreenNotifier2D = $"VisibleOnScreenNotifier2D"
#endregion


func _spawn(dir:Vector2, speed:float) -> void:
	normalized_dir = dir
	velocity_init = speed
	sfx.play()
	area.connect("area_entered", _on_pbullet_collision)
	area.connect("body_entered", _on_body_entered)
	area.connect("body_exited", _on_body_exited)


func _process(delta: float) -> void:
	if intersecting_player and not GameCore.instance.player.stunned and not has_been_parried:
		GameCore.instance.player.adjust_health(-damage)
	
	life_timer += delta
	if (life_timer > max_life_time
	or (despawn_offscreen and life_timer >= 0.25 and not vis.is_on_screen())):
		_despawn()


func parry_reshoot() -> void:
	normalized_dir = Vector2(source_enemy.position - position).normalized()
	life_timer = 0.0
	velocity = velocity_init
	damage = damage * 32 * (Statics.get_shell_level() + 1)


func _on_body_entered(_body) -> void:
	if _body.get_parent() is Player:
		intersecting_player = true
	elif collide_with_world:
		_despawn(true)


func _on_body_exited(_body) -> void:
	if _body.get_parent() is Player:
		intersecting_player = false


func _on_pbullet_collision(_area:Area2D) -> void:
	var bullet = _area.get_parent()
	if bullet is PlayerBullet:
		var i_destroy:bool = pbullets_that_i_destroy.has(bullet.type)
		var destroys_me:bool = pbullets_that_destroy_me.has(bullet.type)
		var angle = rad_to_deg(normalized_dir.angle_to(bullet.normalized_dir))
		angle = abs(angle)
		var destroy_flag
		match pbullet_interaction:
			PBulletInteractions.ALWAYS_DESTROY:
				destroy_flag = true
			PBulletInteractions.DESTROY_PARALLEL:
				destroy_flag = angle <= 22.5 or angle >= 157.5
			PBulletInteractions.DESTROY_PARALLEL_WIDE:
				destroy_flag = angle <= 67.5 or angle >= 112.5
			PBulletInteractions.DESTROY_PERPENDICULAR:
				destroy_flag = angle >= 67.5 and angle <= 112.5
			PBulletInteractions.DESTROY_PERPENDICULAR_WIDE:
				destroy_flag = angle >= 22.5 and angle <= 157.5
		if destroy_flag:
			if i_destroy:
				bullet._despawn(true)
			if destroys_me:
				_despawn(true)


func _despawn(loudly:bool = false) -> void:
	queue_free()
