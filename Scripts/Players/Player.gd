class_name Player
extends CutsceneControllable
## The core script for all player characters.
##
## This core controls values that most if not all player characters are expected
## to make use of, as well as the default movement behavior and any health
## management


#region Global control
## The position occupied by the player on the last frame.
var last_position:Vector2
## The size of the player's normal hitbox on the last frame.
var last_box_size:Vector2
## The direction the player character currently considers downward.
var gravity_dir:Statics.DirsSurface
## The gravity direction seen by the player on the last frame.
var last_gravity:Statics.DirsSurface
## The player's default gravity state to return to if circumstances require.
var home_gravity:Statics.DirsSurface
var current_surface:Statics.DirsSurface
var facing_left:bool
var selected_weapon:int
var armed:bool
var health:int
var max_health:int
var stunned:bool
var in_death_cutscene:bool
var underwater:bool
var velocity:Vector2
var last_rel_vel:Vector2
var grounded:bool
var grounded_last_frame:bool
var shelled:bool
var ungrounded_via_hop:bool
var last_distance:float
var toggle_mode_active:bool
var speed_mod:float = 1.0
var jump_mod:float = 1.0
var gravity_mod:float = 1.0
var holding_jump:bool = false
var holding_shell:bool = false
var axis_flag:bool
var against_wall:bool
var fire_cooldown:float
var fire_mode:bool = false
var idle_timer:Timer
var is_idling:bool
var read_i_speed:int
var read_i_jump:int
var jump_buffer_counter:float
var coyote_time_counter:float
var last_point_before_hop:float
var force_face_x:int
var force_face_y:int
var grav_shock_state:int
var grav_shock_timer:float
var time_since_shell:float
var box_difference:float
var box_adjust:Array = [
	Vector2.UP * 1.5,
	Vector2.RIGHT * 1.5,
	Vector2.LEFT * 1.5,
	Vector2.DOWN * 1.5,
]
var override_box_disable:bool
#endregion


#region Character control
# Movement control vars
# Any var tagged with "[I]" (as in "item") follows this scheme:
# -1 = always, -2 = never, any item ID = item-bound, -3 = disabled by Gravity Lock
# Item scheme variables can contain multiple values, denoting an assortment of items
# that can fulfill a given check
# Example: setting hopWhileMoving to [ [ 4, 7 ], 8 ] will make Snaily hop along the
# ground if they find either (High Jump AND Ice Snail) OR Gravity Snail

var default_gravity:Statics.DirsSurface
# Determines the default direction gravity pulls the player
var can_jump
# [I] Determines if the player can jump
var can_swap_gravity
# [I] Determines if the player can change their gravity state
var retain_gravity_on_airborne
# [I] Determines whether or not player keeps their current gravity when in the air
var can_gravity_jump_opposite
# [I] Determines if the player can change their gravity mid-air to the opposite direction
var can_gravity_jump_adjacent
# [I] Determines if the player can change their gravity mid-air relatively left or relatively right
var can_gravity_shock
# [I] Determines if the player is capable of using Gravity Shock
var shellable
# [I] Determines if the player can retract into a shell
var hop_while_moving
# [I] Determines if the player bounces along the ground when they move
var hop_power
# The power of a walking bounce
var can_round_inner_corners
# [I] Determines if the player can round inside corners
var can_round_outer_corners
# [I] Determines if the player can round outside corners
var can_round_opposite_outer_corners
# [I] Determines if the player can round outside corners opposite the default gravity
var stick_to_walls_when_hurt
# [I] Determines if the player returns to their default gravity when taking damage
var run_speed
# Contains the speed at which the player moves with each shell upgrade
var jump_power
# Contains the player's jump power with each shell upgrade. The second half of the array assumes High Jump
var gravity
# Contains the gravity scale with each shell upgrade
var terminal_velocity
# Contains the player's terminal velocity with each shell upgrade
var jump_floatiness
# Contains how floaty the player's jump is when the jump button is held with each shell upgrade + High Jump
var weapon_cooldowns
# Contains the cooldown in seconds of each weapon. The second half of the array assumes Rapid Fire
var apply_rapid_fire_multiplier:bool
# Determines if collecting Rapid Fire affects bullet velocity
var time_until_idle:float
# Determines how long the player must remain idle before playing an idle animation
#public List<Particle> idleParticles; // -------------------------- Contains every particle used in the player's idle animation so that they can be despawned easily
var hitbox_size_normal:Vector2
# The size of the player's hitbox
var hitbox_size_shell:Vector2
# The size of the player's hitbox while in their shell
var hitbox_offset_normal:Vector2
# The offset of the player's hitbox
var hitbox_offset_shell:Vector2
# The offset of the player's hitbox while in their shell
var unshell_adjust:float
# The amount the player's position is adjusted by when unshelling near a wall
var shell_turnaround_adjust:float
# The amount the player's position is adjusted when turning around in the air while shelled
var coyote_time:float
# How long after leaving the ground via falling the player is still able to jump for
var jump_buffer:float
# How long after pressing the jump button the player will continue to try to jump, in case of an early press
var grav_shock_charge_time:float
# How long it takes for Gravity Shock to fire off after charging
var grav_shock_charge_mult:float
# A fractional multiplier applied to Gravity Shock's charge time when Rapid Fire has been acquired
var grav_shock_speed:float
# How fast Gravity Shock travels
var grav_shock_steering:float
# How fast Gravity Shock can be steered perpendicular to its fire direction
var damage_multiplier:float
# A fractional multiplier applied to any damage taken to increase/decrease characters' defense
var health_gain_from_parry:int
# How much health you recover from a Perfect Parry
#endregion


#region Animation control
enum AnimStates {
	IDLE,
	WALK,
	JUMP,
	FALL,
	SHELL,
}
var current_state:AnimStates = AnimStates.IDLE
#endregion


var sprite:JsonSprite2D
var body:CharacterBody2D
var box_normal:CollisionShape2D
var box_shell:CollisionShape2D
var sfx_jump:AudioStreamPlayer
var sfx_shell:AudioStreamPlayer
var cast_group:Node2D
var corner_cast:RayCast2D
var ground_casts:Array
var front_casts:Array
var ceil_casts:Array


var debug_print_adjustments:bool = false


# _ready() is called every time this script is instanced
# It's used here to initialize certain variables and node references
func _ready():
	super()
	sprite = $"JsonSprite2D"
	body = $"CharacterBody2D"
	box_normal = $"CharacterBody2D/NormalRect"
	box_shell = $"CharacterBody2D/ShellRect"
	Statics.player = self
	body.position = position
	box_shell.disabled = true
	sfx_jump = $"AudioGroup/Jump"
	sfx_shell = $"AudioGroup/Shell"
	cast_group = $"CastGroup"
	corner_cast = $"CastGroup/RoundCornerCast"
	ground_casts = [
		$"CastGroup/GroundCast0",
		$"CastGroup/GroundCast1",
		$"CastGroup/GroundCast2",
		$"CastGroup/GroundCast3",
	]
	front_casts = [
		$"CastGroup/FrontCast0",
		$"CastGroup/FrontCast1",
		$"CastGroup/FrontCast2",
	]
	ceil_casts = [
		$"CastGroup/CeilingCast0",
		$"CastGroup/CeilingCast1",
		$"CastGroup/CeilingCast2"
	]
	
	var rect = box_normal.shape.get_rect()
	box_difference = ((rect.size.x - rect.size.y) * 0.5) + 1
	
	max_health = 3 + Statics.check_item(Item.ItemTypes.HEART_CONTAINER)
	max_health *= Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
	health = max_health


#region Movement
# This function is called once every frame
# It's used here to control player movement
func _process(delta):
	super(delta)
	
	if Statics.noclip_mode:
		box_normal.disabled = true
		box_shell.disabled = true
		var move_speed:float = 160.0
		if Input.get_action_raw_strength("Jump"):
			move_speed = 400.0
		var move_dir = Vector2(Input.get_axis("Left", "Right"), Input.get_axis("Up", "Down"))
		body.velocity = move_dir * move_speed
		body.move_and_slide()
		position = body.position
		return
	
	if override_box_disable:
		box_normal.disabled = true
		box_shell.disabled = true
	else:
		box_normal.disabled = shelled
		box_shell.disabled = not shelled
	# To start things off, we decrease the fire cooldown,
	# and increase the coyote time and jump buffer as necessary
	fire_cooldown = clampf(fire_cooldown - delta, 0.0, INF)
	if Input.is_action_pressed("Jump"):
		jump_buffer_counter += delta
	else:
		jump_buffer_counter = 0.0
	if (not body.is_on_floor()):
		coyote_time_counter += delta
	else:
		coyote_time_counter = 0.0
	# We increment the Gravity Shock timer in case that happens to be active
	if grav_shock_state < 0:
		grav_shock_state += 1
	if grav_shock_state > 0:
		grav_shock_timer += delta
	else:
		grav_shock_timer = 0
	# We update our home direction assuming gravity keep
	# behavior is set to any state change
	if Statics.data_general["grav_keep_type"] != 1:
		home_gravity = default_gravity
	
	# Next, we target a different block of movement code dependent on our current gravity
	# Under typical circumstances, each gravity case would be the same with just a few directionally-dependent values adjusted,
	# but they're referenced separately like this in case a certain character needs a unique case for a particular direction
	if not in_death_cutscene:
		read_i_speed = Statics.get_shell_level()
		read_i_jump = read_i_speed + (4 if Statics.check_item(Item.ItemTypes.HIGH_JUMP) else 0)
		match gravity_dir:
			Statics.DirsSurface.FLOOR:
				_case_down(delta)
			Statics.DirsSurface.LWALL:
				_case_left(delta)
			Statics.DirsSurface.RWALL:
				_case_right(delta)
			Statics.DirsSurface.CEILING:
				_case_up(delta)
			_:
				_case_down(delta)
		if body.velocity.x == INF or body.velocity.x == -INF:
			body.velocity.x = 0
		if body.velocity.y == INF or body.velocity.y == -INF:
			body.velocity.y = 0
	
	if Statics.data_general["shoot_mode"]:
		if Input.is_action_just_pressed("Shoot"):
			fire_mode = not fire_mode
	else:
		fire_mode = Input.is_action_pressed("Shoot")
	if (fire_mode or Input.is_action_pressed("Strafe")) and selected_weapon > 0 and fire_cooldown == 0.0:
		#region Get direction
		var vector_aim = Vector2(Input.get_axis("AimL", "AimR"), Input.get_axis("AimU", "AimD"))
		var vector_raw = Vector2(Input.get_axis("Left", "Right"), Input.get_axis("Up", "Down"))
		var vector_out:Vector2
		if vector_aim != Vector2.ZERO:
			vector_out = vector_aim
		elif vector_raw != Vector2.ZERO:
			vector_out = (Vector2(Statics.VECTOR_DIAG.x * vector_raw.x,
			Statics.VECTOR_DIAG.y * vector_raw.y).normalized())
		else:
			match gravity_dir:
				Statics.DirsSurface.FLOOR:
					vector_out = Vector2.LEFT if facing_left else Vector2.RIGHT
				Statics.DirsSurface.LWALL:
					vector_out = Vector2.UP if facing_left else Vector2.DOWN
				Statics.DirsSurface.RWALL:
					vector_out = Vector2.DOWN if facing_left else Vector2.UP
				Statics.DirsSurface.CEILING:
					vector_out = Vector2.RIGHT if facing_left else Vector2.LEFT
		#endregion
		fire_cooldown = _shoot(selected_weapon, vector_out)
	
	last_position = position + box_normal.position
	last_box_size = box_shell.shape.size if shelled else box_normal.shape.size
	last_gravity = gravity_dir
	grounded_last_frame = grounded


func reset_position(pos:Vector2) -> void:
	global_position = pos.round()
	body.global_position = pos


# The floor case for player movement
# Input  - time since the last frame
func _case_down(delta:float):
	_case_default(delta, Statics.DirsSurface.FLOOR)


# The left wall case for player movement
# Input  - time since the last frame
func _case_left(delta:float):
	_case_default(delta, Statics.DirsSurface.LWALL)


# The right wall case for player movement
# Input  - time since the last frame
func _case_right(delta:float):
	_case_default(delta, Statics.DirsSurface.RWALL)


# The ceiling case for player movement
# Input  - time since the last frame
func _case_up(delta:float):
	_case_default(delta, Statics.DirsSurface.CEILING)


# The default case for player movement, set up to be compatible with all four surfaces
# Input  - time since the last frame
#        - the surface to consider as relatively down
func _case_default(delta:float, surface:Statics.DirsSurface):
	var input_axis_x:float = Input.get_axis("Left", "Right")
	var input_axis_y:float = Input.get_axis("Up", "Down")
	var rel_axis:Vector2
	var rel_vel:Vector2
	var rel_down_pressed:bool
	var rel_vectors:Array
	var remapped_dirs:Array
	var suppress_wall_grab:bool = false
	#region Set relative
	match surface:
		Statics.DirsSurface.FLOOR:
			rel_axis = Vector2(input_axis_x, input_axis_y)
			rel_vel = Vector2(body.velocity.x, body.velocity.y)
			rel_down_pressed = Input.is_action_just_pressed("Down")
			rel_vectors = [
				Vector2.DOWN,
				Vector2.LEFT,
				Vector2.RIGHT,
				Vector2.UP
			]
			remapped_dirs = [
				Statics.DirsSurface.FLOOR,
				Statics.DirsSurface.LWALL,
				Statics.DirsSurface.RWALL,
				Statics.DirsSurface.CEILING
			]
		Statics.DirsSurface.LWALL:
			rel_axis = Vector2(input_axis_y, -input_axis_x)
			rel_vel = Vector2(body.velocity.y, -body.velocity.x)
			rel_down_pressed = Input.is_action_just_pressed("Left")
			rel_vectors = [
				Vector2.LEFT,
				Vector2.UP,
				Vector2.DOWN,
				Vector2.RIGHT
			]
			remapped_dirs = [
				Statics.DirsSurface.LWALL,
				Statics.DirsSurface.CEILING,
				Statics.DirsSurface.FLOOR,
				Statics.DirsSurface.RWALL
			]
		Statics.DirsSurface.RWALL:
			rel_axis = Vector2(-input_axis_y, input_axis_x)
			rel_vel = Vector2(-body.velocity.y, body.velocity.x)
			rel_down_pressed = Input.is_action_just_pressed("Right")
			rel_vectors = [
				Vector2.RIGHT,
				Vector2.DOWN,
				Vector2.UP,
				Vector2.LEFT
			]
			remapped_dirs = [
				Statics.DirsSurface.RWALL,
				Statics.DirsSurface.FLOOR,
				Statics.DirsSurface.CEILING,
				Statics.DirsSurface.LWALL
			]
		Statics.DirsSurface.CEILING:
			rel_axis = Vector2(-input_axis_x, -input_axis_y)
			rel_vel = Vector2(-body.velocity.x, -body.velocity.y)
			rel_down_pressed = Input.is_action_just_pressed("Up")
			rel_vectors = [
				Vector2.UP,
				Vector2.RIGHT,
				Vector2.LEFT,
				Vector2.DOWN
			]
			remapped_dirs = [
				Statics.DirsSurface.CEILING,
				Statics.DirsSurface.RWALL,
				Statics.DirsSurface.LWALL,
				Statics.DirsSurface.FLOOR
			]
	last_rel_vel = rel_vel
	#endregion
	
	rel_vel.x = rel_axis.x * run_speed[read_i_speed] * speed_mod
	if rel_axis.x != 0.0 and grounded and current_state != AnimStates.WALK:
		current_state = AnimStates.WALK
		_play_anim("walk")
	if rel_axis.x == 0.0 and grounded and current_state == AnimStates.WALK:
		current_state = AnimStates.IDLE
		_play_anim("idle")
	if ((rel_axis.x < 0.0 and not facing_left) or
	(rel_axis.x > 0.0 and facing_left)):
		_set_direction(remapped_dirs[Statics.DirsSurface.FLOOR], not facing_left)
		match current_state:
			AnimStates.IDLE:
				_play_anim("turnground")
			AnimStates.WALK:
				_play_anim("turnground")
			AnimStates.JUMP:
				_play_anim("turnjump")
			AnimStates.FALL:
				_play_anim("turnfall")
			AnimStates.SHELL:
				_play_anim("turnshell")
		if shelled and not grounded:
			var adjust_amount = unshell_adjust
			if facing_left:
				adjust_amount = -adjust_amount
			body.translate(rel_vectors[Statics.DirsSurface.RWALL] * adjust_amount)
	
	var aim_vector = Vector2(Input.get_axis("AimL", "AimR"), Input.get_axis("AimU", "AimD"))
	if (shelled and (Input.is_action_pressed("Shoot") or Input.is_action_pressed("Strafe"))
	or aim_vector != Vector2.ZERO):
		_toggle_shell()
	
	if grounded:
		var unshell_on_jump:bool = false
		if ((Input.is_action_just_pressed("Jump") or
		(Input.is_action_pressed("Jump") and (jump_buffer_counter < jump_buffer))) and 
		not _check_ceil_casts()[0]):
			if (not _check_ability(retain_gravity_on_airborne)) and gravity_dir != home_gravity and grounded_last_frame:
				var adjust_position = Vector2.ZERO
				adjust_position = _get_adjust_position(surface, home_gravity, rel_vectors[Statics.DirsSurface.CEILING], 4.0)
				suppress_wall_grab = true
				if debug_print_adjustments:
					print("^ Airborne from jump off wall or ceiling")
				_set_direction(home_gravity, not facing_left)
				if adjust_position != Vector2.ZERO:
					body.position = adjust_position
					position = body.position
				coyote_time_counter = coyote_time
				jump_buffer_counter = jump_buffer
				rel_vel = Vector2.ZERO
			else:
				rel_vel.y = jump_power[read_i_jump] * jump_mod
			grounded = false
			sfx_jump.play()
			current_state = AnimStates.JUMP
			_play_anim("jump")
			jump_buffer_counter = jump_buffer
			coyote_time_counter = coyote_time
			if shelled:
				unshell_on_jump = true
		if (rel_vel.x != 0.0 and shelled) or unshell_on_jump:
			_toggle_shell()
			if current_state != AnimStates.JUMP:
				current_state = AnimStates.IDLE
		if not _check_ground_casts()[0]:
			grounded = false
			if debug_print_adjustments:
				print("We've left the ground")
	if not grounded:
		var suppress_next_check:bool = false
		if (rel_axis.x != 0.0 and rel_axis.y > 0.0 and grounded_last_frame and
		corner_cast.is_colliding() and not _check_ground_casts()[0]):
			var opposite_dir = _get_dir_opposite(home_gravity)
			var new_gravity = _get_dir_adjacent_ccw(surface) if facing_left else _get_dir_adjacent_cw(surface)
			if (_check_ability(can_swap_gravity) and (
			((gravity_dir == opposite_dir or new_gravity == opposite_dir) and _check_ability(can_round_opposite_outer_corners)) or 
			((gravity_dir != opposite_dir and new_gravity != opposite_dir) and _check_ability(can_round_outer_corners)))):
				var rel_wall = Statics.DirsSurface.RWALL if facing_left else Statics.DirsSurface.LWALL
				var adjust_position = _get_adjust_position(surface, new_gravity, rel_vectors[rel_wall])
				if debug_print_adjustments:
					print("^ Airborne from rounding outer corner")
				_set_direction(new_gravity, facing_left)
				body.position = adjust_position
				position = body.position
				rel_vel.x = 0.0
				grounded = true
				_play_anim("shell" if shelled else "walk")
				suppress_next_check = true
		if ((not _check_ability(retain_gravity_on_airborne)) and
		gravity_dir != home_gravity and grounded_last_frame and not suppress_next_check):
			var new_left = facing_left
			var adjust_position = Vector2.ZERO
			if _get_dir_opposite(surface) != home_gravity:
				adjust_position = _get_adjust_position(surface, home_gravity, rel_vectors[Statics.DirsSurface.FLOOR])
				if debug_print_adjustments:
					print("^ Airborne from walking off wall or ceiling")
				facing_left = not facing_left
			_set_direction(home_gravity, new_left)
			if adjust_position != Vector2.ZERO:
				body.position = adjust_position
				position = body.position
			coyote_time_counter = coyote_time
			jump_buffer_counter = jump_buffer
			_play_anim("fall")
		else:
			rel_vel.y += gravity[read_i_jump] * gravity_mod
			if rel_vel.y < 0.0 and not Input.is_action_pressed("Jump"):
				rel_vel.y = Statics.integrate(rel_vel.y, 0.0, jump_floatiness[read_i_speed], delta)
			rel_vel.y = clampf(rel_vel.y, -INF, terminal_velocity[read_i_jump])
			if (rel_vel.y > 0.0 and
			(current_state == AnimStates.IDLE or current_state == AnimStates.JUMP)):
				current_state = AnimStates.FALL
				_play_anim("fall")
			if (Input.is_action_just_pressed("Jump") and (coyote_time_counter < coyote_time) and 
			not _check_ceil_casts()[0]):
				rel_vel.y = jump_power[read_i_jump] * jump_mod
				grounded = false
				sfx_jump.play()
				current_state = AnimStates.JUMP
				_play_anim("jump")
				jump_buffer_counter = jump_buffer
				coyote_time_counter = coyote_time
	
	if body.is_on_wall() and rel_axis.x != 0.0 and not suppress_wall_grab:
		if ((rel_axis.y < 0.0 or (rel_axis.y > 0.0 and not grounded)) and not _check_ceil_casts()[0]
		and _check_ability(can_swap_gravity) and _check_ability(can_round_inner_corners)):
			rel_vel = Vector2.ZERO
			var new_gravity
			if facing_left:
				new_gravity = _get_dir_adjacent_cw(gravity_dir)
			else:
				new_gravity = _get_dir_adjacent_ccw(gravity_dir)
			var new_left
			if rel_axis.y < 0.0:
				new_left = facing_left
			else:
				new_left = not facing_left
			var rel_against_wall = Statics.DirsSurface.LWALL if facing_left else Statics.DirsSurface.RWALL
			var adjust_position = _get_adjust_position(surface, new_gravity, rel_vectors[rel_against_wall])
			if debug_print_adjustments:
				print("^ Grounded from grabbing wall")
			_set_direction(new_gravity, new_left)
			body.position = adjust_position
			position = body.position
			grounded = true
			current_state = AnimStates.WALK
			_play_anim("walk" if grounded else "land")
	
	if rel_down_pressed and rel_vel.x == 0 and _check_ability(shellable):
		_toggle_shell()
	
	#region Restore relative
	match surface:
		Statics.DirsSurface.FLOOR:
			body.velocity = rel_vel
		Statics.DirsSurface.LWALL:
			body.velocity = Vector2(-rel_vel.y, rel_vel.x)
		Statics.DirsSurface.RWALL:
			body.velocity = Vector2(rel_vel.y, -rel_vel.x)
		Statics.DirsSurface.CEILING:
			body.velocity = -rel_vel
	#endregion
	
	body.move_and_slide()
	if not grounded and (body.is_on_floor() or body.is_on_ceiling()):
		if surface == Statics.DirsSurface.FLOOR or surface == Statics.DirsSurface.CEILING:
			body.velocity.y = 0.0
		else:
			body.velocity.x = 0.0
		if body.is_on_floor():
			grounded = true
			if not shelled:
				current_state = AnimStates.IDLE
				_play_anim("land")
		elif body.is_on_ceiling() and rel_axis.y < 0.0 and _check_ability(can_swap_gravity):
			grounded = true
			var adjust_position = _get_adjust_position(surface, _get_dir_opposite(surface), rel_vectors[_get_dir_opposite(surface)])
			if debug_print_adjustments:
				print("^ Grounded from grabbing ceiling")
			_set_direction(_get_dir_opposite(surface), not facing_left)
			body.position = adjust_position
			current_state = AnimStates.IDLE if rel_axis.x == 0.0 else AnimStates.WALK
			_play_anim("land")
	
	if body.velocity == Vector2.ZERO:
		position = body.position.round() #Stops strange jitter from camera smoothing
	else:
		position = body.position
#endregion


#region Player utilities
# Takes a character ability as an input and checks to see if the ability's values
# allow for the ability to be executed.
# Input  - an ability array
# Output - true if the ability should be considered, false otherwise
func _check_ability(ability:Array) -> bool:
	var found = false
	for i in ability:
		if Statics.is_number(i):
			if i == -1:
				found = true
		else:
			for j in i:
				if j == -1:
					found = true
	return found


# Rotates and flips the character's hitboxes and raycasts to match any of the eight
# valid directions a character can face
# Input  - the surface the player should consider as relatively down
#        - whether or not the player should face relatively left
#        - whether or not the target surface should be set as the new "home" gravity
func _set_direction(surface:Statics.DirsSurface, flipped:bool, set_home:bool = false):
	gravity_dir = surface
	if set_home:
		home_gravity = surface
	facing_left = flipped
	match surface:
		Statics.DirsSurface.FLOOR:
			body.rotation_degrees = 0.0
			body.up_direction = Vector2.UP
			cast_group.rotation_degrees = 0.0
		Statics.DirsSurface.LWALL:
			body.rotation_degrees = 90.0
			body.up_direction = Vector2.RIGHT
			cast_group.rotation_degrees = 90.0
		Statics.DirsSurface.CEILING:
			body.rotation_degrees = 180.0
			body.up_direction = Vector2.DOWN
			cast_group.rotation_degrees = 180.0
		Statics.DirsSurface.RWALL:
			body.rotation_degrees = 270.0
			body.up_direction = Vector2.LEFT
			cast_group.rotation_degrees = 270.0
	body.scale = Vector2(-1 if flipped else 1, 1)
	cast_group.scale = Vector2(-1 if flipped else 1, 1)


# Inverts the player's current shell state
func _toggle_shell():
	_set_shell(not shelled)


# Sets the player's current shell state to a specific
# Input  - true to enter shell, false to exit shell
func _set_shell(state:bool):
	shelled = state
	box_normal.disabled = state
	box_shell.disabled = not state
	if state:
		sfx_shell.play()
		_play_anim("shell")
		current_state = AnimStates.SHELL
	else:
		_play_anim("unshell")


# Takes in specific surface data to figure out a vector to adjust the player's position by,
# then translates the player's CharacterBody2D by that amount.
# Usually called for gravity changes requiring a surface alignment.
# Input  - the player's current surface, for keeping the hitbox centered
#        - the player's target surface, for keeping the hitbox centered
#        - the direction in which to apply the biggest nudge
#        - any additional adjustment to the distance nudged
func _translate_adjust(surface:Statics.DirsSurface, new_surface:Statics.DirsSurface, surface_vector:Vector2, adjustment:float = 0.0):
	var adjust_vector = (surface_vector * (box_difference + adjustment)) + box_adjust[surface] - box_adjust[new_surface]
	body.translate(adjust_vector)


# Takes in specific surface data to figure out a vector to adjust the player's position by,
# then outputs the position that a nudge from _translate_adjust() would place the CharacterBody2D
# Usually called for gravity changes requiring a surface alignment.
# Input  - the player's current surface, for keeping the hitbox centered
#        - the player's target surface, for keeping the hitbox centered
#        - the direction in which to apply the biggest nudge
#        - any additional adjustment to the distance nudged
# Output - the position a nudge performed by _translate_adjust() would place the CharacterBody2D at
func _get_adjust_position(surface:Statics.DirsSurface, new_surface:Statics.DirsSurface, surface_vector:Vector2, adjustment:float = 0.0) -> Vector2:
	var adjust_vector = Vector2.ZERO
	if surface == new_surface or surface == _get_dir_opposite(new_surface):
		adjust_vector = surface_vector * (1.0 if adjustment == 0.0 else adjustment)
	else:
		adjust_vector = surface_vector * (box_difference + adjustment)
	adjust_vector += box_adjust[new_surface] - box_adjust[surface]
	var target_pos = body.position + adjust_vector
	if debug_print_adjustments:
		print(adjust_vector)
	return target_pos


# Takes an action name, considers the current state of the player, and sets the player's
# JsonSprite2D animation appropriately
# Input  - the action to perform
func _play_anim(action:String):
	var full_action = ""
	
	full_action += "0."
	
	match gravity_dir:
		Statics.DirsSurface.FLOOR:
			full_action += "floor."
		Statics.DirsSurface.LWALL:
			full_action += "lwall."
		Statics.DirsSurface.RWALL:
			full_action += "rwall."
		Statics.DirsSurface.CEILING:
			full_action += "ceiling."
	full_action += "left." if facing_left else "right."
	
	full_action += action
	if sprite.action != full_action:
		sprite.action = full_action
	#print(full_action)


# Takes a surface direction and outputs the direction 90 degrees clockwise from it
# Input  - the original surface
# Output - the rotated surface
func _get_dir_adjacent_cw(old_dir:Statics.DirsSurface) -> Statics.DirsSurface:
	var new_dir
	match old_dir:
		Statics.DirsSurface.FLOOR:
			new_dir = Statics.DirsSurface.LWALL
		Statics.DirsSurface.LWALL:
			new_dir = Statics.DirsSurface.CEILING
		Statics.DirsSurface.RWALL:
			new_dir = Statics.DirsSurface.FLOOR
		Statics.DirsSurface.CEILING:
			new_dir = Statics.DirsSurface.RWALL
	return new_dir


# Takes a surface direction and outputs the direction 90 degrees counterclockwise from it
# Input  - the original surface
# Output - the rotated surface
func _get_dir_adjacent_ccw(old_dir:Statics.DirsSurface) -> Statics.DirsSurface:
	var new_dir
	match old_dir:
		Statics.DirsSurface.FLOOR:
			new_dir = Statics.DirsSurface.RWALL
		Statics.DirsSurface.LWALL:
			new_dir = Statics.DirsSurface.FLOOR
		Statics.DirsSurface.RWALL:
			new_dir = Statics.DirsSurface.CEILING
		Statics.DirsSurface.CEILING:
			new_dir = Statics.DirsSurface.LWALL
	return new_dir


# Takes a surface direction and outputs the direction 180 degrees from it
# Input  - the original surface
# Output - the opposite surface
func _get_dir_opposite(old_dir:Statics.DirsSurface) -> Statics.DirsSurface:
	var new_dir
	match old_dir:
		Statics.DirsSurface.FLOOR:
			new_dir = Statics.DirsSurface.CEILING
		Statics.DirsSurface.LWALL:
			new_dir = Statics.DirsSurface.RWALL
		Statics.DirsSurface.RWALL:
			new_dir = Statics.DirsSurface.LWALL
		Statics.DirsSurface.CEILING:
			new_dir = Statics.DirsSurface.FLOOR
	return new_dir


# Queries the player's ground RayCast2Ds to see if any of them are colliding with
# a floor, and at what distance if so
# Output - an array of length 2 where
#              [0] is true if a floor was found, false if not
#              [1] is the shortest distance at which a floor was found. INF if no floor was found
func _check_ground_casts() -> Array:
	var hit = false
	var distance = INF
	for i in ground_casts:
		if i.is_colliding():
			hit = true
			var origin = i.global_position
			var collision = i.get_collision_point()
			var new_distance = origin.distance_to(collision)
			if new_distance < distance:
				distance = new_distance
	return [ hit, distance ]


# Queries the player's front RayCast2Ds to see if any of them are colliding with
# a wall, and at what distance if so
# Output - an array of length 2 where
#              [0] is true if a wall was found, false if not
#              [1] is the shortest distance at which a floor was found. INF if no wall was found
func _check_front_casts() -> Array:
	var hit = false
	var distance = INF
	for i in front_casts:
		if i.is_colliding():
			hit = true
			var origin = i.global_position
			var collision = i.get_collision_point()
			var new_distance = origin.distance_to(collision)
			if new_distance < distance:
				distance = new_distance
	return [ hit, distance ]


# Queries the player's ceiling RayCast2Ds to see if any of them are colliding with
# a ceiling, and at what distance if so
# Output - an array of length 2 where
#              [0] is true if a ceiling was found, false if not
#              [1] is the shortest distance at which a floor was found. INF if no ceiling was found
func _check_ceil_casts() -> Array:
	var hit = false
	var distance = INF
	for i in ceil_casts:
		if i.is_colliding():
			hit = true
			var origin = i.global_position
			var collision = i.get_collision_point()
			var new_distance = origin.distance_to(collision)
			if new_distance < distance:
				distance = new_distance
	return [ hit, distance ]


func set_box_disable_override(state:bool) -> void:
	return
	override_box_disable = state
	if state == true:
		box_normal.disabled = true
		box_shell.disabled = true
	else:
		box_normal.disabled = shelled
		box_shell.disabled = not shelled


func adjust_health(amount:int) -> void:
	health += amount
	health = clampi(health, 0, max_health)
	UICore.instance.update_hearts()
	if health == 0:
		die()


func die() -> void:
	pass
#endregion


#region Bullet functions
func _toggle_weapon(id:int) -> void:
	var shifted_id = 1 << id
	if Statics.stack_weapons:
		if selected_weapon & shifted_id > 0:
			selected_weapon -= shifted_id
		else:
			selected_weapon += shifted_id
	else:
		selected_weapon = shifted_id


func _shoot(bullet_id:int, normalized_velocity:Vector2, pos:Vector2 = body.position) -> float:
	var bullet_type:String = ""
	#region Determine bullet type
	if Statics.stack_weapons:
		if selected_weapon & 1 > 0:
			bullet_type += "A"
		if selected_weapon & 2 > 0:
			bullet_type += "B"
		if selected_weapon & 4 > 0:
			bullet_type += "C"
		if selected_weapon & 8 > 0:
			bullet_type += "D"
	else:
		if selected_weapon == 8:
			bullet_type = "BD"
		elif selected_weapon == 4:
			bullet_type = "BC"
		elif selected_weapon == 2:
			bullet_type = "B"
		elif selected_weapon == 1:
			bullet_type = "A"
	#endregion
	var bullet_scene = load("res://Scenes/Entities/Bullets/Player/PlayerBullet" + bullet_type + ".tscn")
	var new_bullet:PlayerBullet = bullet_scene.instantiate()
	var is_power = Statics.check_item(Item.ItemTypes.DEVASTATOR)
	GameCore.instance.current_room.layer_fg1.add_child(new_bullet)
	new_bullet.position = pos
	if pos == body.position:
		new_bullet.position += normalized_velocity * Statics.FRAC_8
	var this_cooldown = new_bullet._spawn(normalized_velocity, 1.0, is_power)
	return this_cooldown
#endregion


#region Cutscene functions
func impulse(direction:Vector2) -> bool:
	return false


func glide_to(position:Vector2, duration:float) -> bool:
	return false


func get_dialogue_icon() -> Texture:
	return null


func fake_input(event:InputEventAction, hold_for:float) -> bool:
	return false


func look_at_position(pos:Vector2) -> bool:
	return false


func look_at_local(pos:Vector2) -> bool:
	return false


func look_at_node(node:Node2D) -> bool:
	return false


func lock_inputs(locked:bool) -> bool:
	return false


func has_item(ID:Statics.Items) -> bool:
	return false


func can_perform_action(action:String) -> bool:
	return false


func perform_action(action:String, force:bool) -> bool:
	return false
#endregion
