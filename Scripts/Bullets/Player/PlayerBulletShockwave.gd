# Copyright 2026 CE-Studio: AGPL-3.0-only
extends PlayerBullet


## Used to find ground in the direction of fire
@onready var cast:RayCast2D = $"Area2D/GroundCast"


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.RIGHT
	super._spawn(dir, rapid_shot, power_shot)
	velocity = 70 if powered else 48
	velocity_init = velocity
	cooldown /= rapid_shot
	_spin_to_surface()
	#region Direction animation
	var surface:Statics.DirsSurface = Player.instance.gravity_dir
	var anim_surface:String = _get_surface_string(surface)
	var anim_dir:String = "R" if dir.x > 0.0 else "L"
	if surface == Statics.DirsSurface.LWALL or surface == Statics.DirsSurface.RWALL:
		anim_dir = "D" if dir.y > 0.0 else "U"
	var anim_name = "_".join([anim_surface, anim_dir])
	if powered:
		anim_name += "_power"
	sprite.play(anim_name)
	if anim_dir == "L" or anim_surface == "lwall":
		sprite.flip_h = true
	if anim_dir == "U" or anim_surface == "ceiling":
		sprite.flip_v = true
	#endregion
	return cooldown


func _physics_process(delta: float) -> void:
	position += velocity * normalized_dir * delta
	velocity += velocity_init * 15.0 * delta
	super(delta)
	if (not cast.is_colliding() and life_timer > 0.0625) or Statics.solid_at_world_pos(box_normal.global_position):
		despawn(true)
