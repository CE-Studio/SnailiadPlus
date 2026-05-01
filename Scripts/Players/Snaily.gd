# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
extends Player


# Called when the node enters the scene tree for the first time.
func _ready():
	super()
	who_i_is = Players.SNAILY
	default_gravity = Statics.DirsSurface.FLOOR
	can_jump = [ [-1] ]
	can_swap_gravity = [ [-1, -3] ]
	retain_gravity_on_airborne = [ [Item.ItemTypes.GRAVITY_SHELL] ]
	can_gravity_jump_opposite = [ [Item.ItemTypes.GRAVITY_SHELL] ]
	can_gravity_jump_adjacent = [ [Item.ItemTypes.GRAVITY_SHELL] ]
	can_gravity_shock = [ [Item.ItemTypes.GRAVITY_SHOCK] ]
	shellable = [ [-1] ]
	hop_while_moving = [ [-2] ]
	hop_power = 0.0
	can_round_inner_corners = [ [-1] ]
	can_round_outer_corners = [ [-1] ]
	can_round_opposite_outer_corners = [ [Item.ItemTypes.GRAVITY_SHELL] ]
	stick_to_walls_when_hurt = [ [Item.ItemTypes.GRAVITY_SHELL] ]
	run_speed = [ 138.6667, 138.6667, 138.6667, 176 ]
	jump_power = [ -428, -428, -428, -428, -498, -498, -498, -498 ]
	gravity = [ 1200, 1200, 1200, 1200, 1200, 1200, 1200, 1200 ]
	terminal_velocity = [ 500, 500, 500, 500, 500, 500, 500, 500 ]
	jump_floatiness = [ 4, 4, 4, 4, 4, 4, 4, 4 ]
	#weapon_cooldowns
	apply_rapid_fire_multiplier = true
	time_until_idle = 30.0
	hitbox_size_normal = Vector2(24, 13)
	hitbox_offset_normal = Vector2(0, 1.5)
	hitbox_offset_shell = Vector2(12, 13)
	hitbox_offset_shell = Vector2(-3, 1.5)
	#unshell_adjust
	shell_turnaround_adjust = 0.1667
	coyote_time = 0.125
	jump_buffer = 0.125
	grav_shock_charge_time = 0.75
	grav_shock_charge_mult = 0.5
	grav_shock_speed = 640.0
	grav_shock_steering = 40.0
	damage_multiplier = 1
	shield_particle_offset = Vector2i(-3, 3)
	health_gain_from_parry = 4
	light_radius = 32

	sprite.action = "0.floor.right.idle"

	corner_cast = $"CastGroup/RoundCornerCast"
	ground_casts = [
		$"CastGroup/Normal/GroundCast0",
		$"CastGroup/Normal/GroundCast1",
		$"CastGroup/Normal/GroundCast2",
		$"CastGroup/Normal/GroundCast3",
		$"CastGroup/Shell/GroundCast0",
		$"CastGroup/Shell/GroundCast1",
		$"CastGroup/Shell/GroundCast2",
		$"CastGroup/Shell/GroundCast3",
	]
	front_casts = [
		$"CastGroup/Normal/FrontCast0",
		$"CastGroup/Normal/FrontCast1",
		$"CastGroup/Normal/FrontCast2",
		$"CastGroup/Shell/FrontCast0",
		$"CastGroup/Shell/FrontCast1",
		$"CastGroup/Shell/FrontCast2",
	]
	ceil_casts = [
		$"CastGroup/Normal/CeilingCast0",
		$"CastGroup/Normal/CeilingCast1",
		$"CastGroup/Normal/CeilingCast2",
		$"CastGroup/Shell/CeilingCast0",
		$"CastGroup/Shell/CeilingCast1",
		$"CastGroup/Shell/CeilingCast2",
	]
	normal_casts = [
		$"CastGroup/Normal/GroundCast0",
		$"CastGroup/Normal/GroundCast1",
		$"CastGroup/Normal/GroundCast2",
		$"CastGroup/Normal/GroundCast3",
		$"CastGroup/Normal/FrontCast0",
		$"CastGroup/Normal/FrontCast1",
		$"CastGroup/Normal/FrontCast2",
		$"CastGroup/Normal/CeilingCast0",
		$"CastGroup/Normal/CeilingCast1",
		$"CastGroup/Normal/CeilingCast2",
	]
	shell_casts = [
		$"CastGroup/Shell/GroundCast0",
		$"CastGroup/Shell/GroundCast1",
		$"CastGroup/Shell/GroundCast2",
		$"CastGroup/Shell/GroundCast3",
		$"CastGroup/Shell/FrontCast0",
		$"CastGroup/Shell/FrontCast1",
		$"CastGroup/Shell/FrontCast2",
		$"CastGroup/Shell/CeilingCast0",
		$"CastGroup/Shell/CeilingCast1",
		$"CastGroup/Shell/CeilingCast2",
	]


func tick_death(delta:float) -> void:
	if not in_death_cutscene:
		match gravity_dir:
			Statics.DirsSurface.FLOOR:
				facing_left = facing_left
			Statics.DirsSurface.CEILING:
				facing_left = not facing_left
			Statics.DirsSurface.LWALL:
				facing_left = true
			Statics.DirsSurface.RWALL:
				facing_left = true
		_play_anim("death")
		velocity = Vector2(110 if facing_left else -110, -300)
		sprite.visible = true
	super(delta)
	sprite.position += velocity * delta
	velocity.y += gravity[read_i_jump] * delta
