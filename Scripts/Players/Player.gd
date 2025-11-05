class_name Player
extends CutsceneControllable
## The core script for all player characters.
##
## This core controls values that most if not all player characters are expected
## to make use of, as well as the default movement behavior and any health
## management


#region Global control
const MAX_STUN_TIMER:float = 1.0
const RESPAWN_INVIN_TIMER:float = 0.25
const MOVE_STEPS:int = 4

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
var stun_timer:float
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
var environment_exit_override:int = 0
var respawn_i_frames:float = 0.0
var outer_allowed:bool = false
var just_jumped:int = 0
var shell_level_displayed:int = 0
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
var shield_particle_offset:Vector2i
# An offset from the center of the player used to align any shield particle effect. Should be set as if the player is on the ground facing right
var health_gain_from_parry:int
# How much health you recover from a Perfect Parry
var light_radius:int
# How large the light emitted by the player should be
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


enum Players {
	SNAILY,
	SLUGGY,
	UPSIDE,
	LEGGY,
	BLOBBY,
	LEECHY,
}


static var player_name:String:
	set(_v):
		pass
	get:
		return Statics.get_character_name_string(Statics.current_profile["character"], false)


static var player_full_name:String:
	set(_v):
		pass
	get:
		return Statics.get_character_name_string(Statics.current_profile["character"], true)


static var player_species:String:
	set(_v):
		pass
	get:
		return Statics.get_character_species_string(Statics.current_profile["character"], false)


static var player_species_plural:String:
	set(_v):
		pass
	get:
		return Statics.get_character_species_string(Statics.current_profile["character"], true)


var sprite:JsonSprite2D
var body:CharacterBody2D
var box_normal:CollisionShape2D
var box_shell:CollisionShape2D
var sfx_jump:AudioStreamPlayer
var sfx_shell:AudioStreamPlayer
var sfx_hurt:AudioStreamPlayer
var sfx_ping:AudioStreamPlayer
var sfx_parry:AudioStreamPlayer
var sfx_death:AudioStreamPlayer
var cast_group:Node2D
var corner_cast:RayCast2D
var ground_casts:Array
var front_casts:Array
var ceil_casts:Array
var normal_casts:Array
var shell_casts:Array
var shield_particle:Particle
var timer_die_fade:Timer
var timer_die_respawn:Timer


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
	sfx_hurt = $"AudioGroup/Hurt"
	sfx_ping = $"AudioGroup/Ping"
	sfx_parry = $"AudioGroup/Parry"
	sfx_death = $"AudioGroup/Die"
	cast_group = $"CastGroup"
	timer_die_fade = $"TimerGroup/DieFadeDelay"
	timer_die_respawn = $"TimerGroup/RespawnDelay"
	
	var rect = box_normal.shape.get_rect()
	box_difference = ((rect.size.x - rect.size.y) * 0.5) + 1
	
	max_health = 3 + Statics.check_item(Item.ItemTypes.HEART_CONTAINER)
	max_health *= Statics.HEALTH_PER_HEART[Statics.current_profile["difficulty"]]
	health = max_health
	
	#var shell_mode = 0 if Statics.stack_shells else 1
	#shell_level_displayed = Statics.get_shell_level()
	if Statics.stack_shells:
		shell_level_displayed = 1 << (Statics.get_shell_level() - 1)
	else:
		shell_level_displayed = Statics.get_shell_level(1)


#region Movement
# This function is called once every frame
func _process(_delta):
	if stun_timer > 0:
		sprite.visible = not sprite.visible
	
	if UICore.instance and not UICore.instance.darkness_layer.sources.has(self):
		UICore.instance.darkness_layer.add_source(self, light_radius)
	
	if Input.is_action_just_pressed("gravity"):
		Statics.spawn_particle("ShellUpEffect", Room.Layers.GROUND, position, [randi_range(1, 6)])


# This function is called on a fixed interval of
# 60 times per second, regardless of framerate
func _physics_process(delta) -> void:
	if Statics.noclip_mode:
		box_normal.disabled = true
		box_shell.disabled = true
		var move_speed:float = 160.0
		if SInput.input_pressed(SInput.Inputs.JUMP):
			move_speed = 400.0
		var move_dir = SInput.vector_move()
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
	if SInput.input_pressed(SInput.Inputs.JUMP):
		jump_buffer_counter += delta
	else:
		jump_buffer_counter = 0.0
	if (not body.is_on_floor()):
		coyote_time_counter += delta
	else:
		coyote_time_counter = 0.0
	if environment_exit_override > 0:
		environment_exit_override -= 1
	# We increment the Gravity Shock timer in case that happens to be active
	if grav_shock_state < 0:
		grav_shock_state += 1
	if grav_shock_state > 0:
		grav_shock_timer += delta
	else:
		grav_shock_timer = 0
	# We update our home direction assuming gravity keep
	# behavior is set to any state change
	if ProjectSettings.get_setting("game/control/gravity_keep") != 1:
		home_gravity = default_gravity
	
	# Here, we control weapon swapping
	if SInput.input_just_pressed(SInput.Inputs.WEAPON0) and Statics.check_item(Item.ItemTypes.BROOM):
		_toggle_weapon(0)
	if SInput.input_just_pressed(SInput.Inputs.WEAPON1) and Statics.check_item(Item.ItemTypes.PEASHOOTER):
		_toggle_weapon(1)
	if (SInput.input_just_pressed(SInput.Inputs.WEAPON2)
	and (Statics.check_item(Item.ItemTypes.BOOMERANG) or Statics.check_item(Item.ItemTypes.SECRET_BOOMERANG))):
		_toggle_weapon(2)
	if (SInput.input_just_pressed(SInput.Inputs.WEAPON3)
	and (Statics.check_item(Item.ItemTypes.RAINBOW_WAVE) or Statics.check_item(Item.ItemTypes.DEBUG_WAVE))):
		_toggle_weapon(3)
	
	# Next, we target a different block of movement code dependent on our current gravity
	# Under typical circumstances, each gravity case would be the same with just a few directionally-dependent values adjusted,
	# but they're referenced separately like this in case a certain character needs a unique case for a particular direction
	if in_death_cutscene:
		tick_death(delta)
	else:
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
	
	if ProjectSettings.get_setting("game/control/toggle_shoot"):
		if SInput.input_just_pressed(SInput.Inputs.SHOOT):
			fire_mode = not fire_mode
	else:
		fire_mode = SInput.input_pressed(SInput.Inputs.SHOOT)
	if ((fire_mode or SInput.input_pressed(SInput.Inputs.STRAFE) or SInput.vector_aim() != Vector2.ZERO)
	and selected_weapon > 0 and fire_cooldown == 0.0 and not in_death_cutscene):
		#region Get direction
		var vector_aim = SInput.vector_aim()
		var vector_raw = SInput.vector_move()
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
	
	if stun_timer > 0:
		stun_timer -= delta
		if stun_timer <= 0:
			stunned = false
			sprite.visible = true
	
	if shield_particle:
		var shield_offset = Vector2(
			shield_particle_offset.x * (-1 if facing_left else 1),
			shield_particle_offset.y
		)
		shield_particle.position = position + _spin_vector_to_surface(shield_offset, gravity_dir)


func reset_position(pos:Vector2) -> void:
	global_position = pos.round()
	body.global_position = pos
	sprite.position = Vector2.ZERO
	in_death_cutscene = false
	_play_anim("idle")


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
	var input_axis:Vector2 = SInput.vector_move() # The raw input vector
	var rel_axis:Vector2 # Input vector, rotated to match the current gravity state
	var rel_vel:Vector2 # Velocity, rotated to match the current gravity state
	var rel_down_pressed:bool # Shortcut boolean check for the relative down input
	var _rel_vectors:Array # Array remapping raw cardinal vectors to match gravity. Index with DirsSurface
	var remapped_dirs:Array # Array remapping DirsSurface references to match gravity. Index with DirsSurface
	var _suppress_wall_grab:bool = false # Boolean that forces wall checks to be ignored
	var aim_vector = SInput.vector_aim()
	#region Set relative
	match surface:
		Statics.DirsSurface.FLOOR:
			rel_axis = input_axis
			rel_vel = Vector2(body.velocity.x, body.velocity.y)
			rel_down_pressed = SInput.input_just_pressed(SInput.Inputs.DOWN)
			_rel_vectors = [
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
			rel_axis = Vector2(input_axis.y, -input_axis.x)
			rel_vel = Vector2(body.velocity.y, -body.velocity.x)
			rel_down_pressed = SInput.input_just_pressed(SInput.Inputs.LEFT)
			_rel_vectors = [
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
			rel_axis = Vector2(-input_axis.y, input_axis.x)
			rel_vel = Vector2(-body.velocity.y, body.velocity.x)
			rel_down_pressed = SInput.input_just_pressed(SInput.Inputs.RIGHT)
			_rel_vectors = [
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
			rel_axis = -input_axis
			rel_vel = Vector2(-body.velocity.x, -body.velocity.y)
			rel_down_pressed = SInput.input_just_pressed(SInput.Inputs.UP)
			_rel_vectors = [
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
	
	# The way physics process works, we want to move as little and as late as possible
	# Therefore, all checks should take place before actual movement, so as to keep things
	# tight and accurate.
	
	# Apply horizontal move speed from input to velocity
	# If grounded and trying to jump, apply jump power and mark not grounded
	# Elif not grounded, apply gravity scale
	# If shelled and basically anything, toggle shell
	# Elif just down and can shell, toggle shell
	# If running against wall:
	# - If grounded and up, enter wall state
	# - If not grounded, enter wall state based on vertical input
	# Elif not grounded
	# - If not home gravity, revert
	# - Else
	# - - Ceiling check
	# - - If grounded last frame
	# - - - Corner check
	# - - Elif ceiling and up and can ceiling, enter ceiling state
	# - - Elif is on ground, zero vertical velocity and mark grounded
	# Move and slide
	
	rel_vel.x = rel_axis.x * run_speed[read_i_speed] * speed_mod
	if SInput.input_pressed(SInput.Inputs.STRAFE):
		rel_vel.x = 0.0
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
	
	just_jumped = clampi(just_jumped - 1, 0, 10)
	if grounded:
		if ((SInput.input_just_pressed(SInput.Inputs.JUMP) or
		(SInput.input_pressed(SInput.Inputs.JUMP) and (jump_buffer_counter < jump_buffer))) and 
		not _check_ceil_casts()[0]):
			if shelled:
				_toggle_shell()
			rel_vel.y = _jump_decide_state()
			outer_allowed = false
			just_jumped = 3
		elif not body.is_on_floor():
			if _check_ground_casts()[0]:
				# Snap to ground with another move call
				rel_vel.y = 720
				#   12 (standard expected length of player casts)
				# * 60 (compensating move_and_slide dividing by physics tick rate)
				grounded = true
				outer_allowed = true
			else:
				grounded = false
	else:
		if (SInput.input_just_pressed(SInput.Inputs.JUMP) and (coyote_time_counter < coyote_time) and 
		not _check_ceil_casts()[0]):
			rel_vel.y = _jump()
		else:
			rel_vel.y += gravity[read_i_jump] * gravity_mod * delta
			if rel_vel.y < 0.0 and not SInput.input_pressed(SInput.Inputs.JUMP):
				rel_vel.y = Statics.integrate(rel_vel.y, 0.0, jump_floatiness[read_i_speed], delta)
			rel_vel.y = clampf(rel_vel.y, -INF, terminal_velocity[read_i_jump])
			if (rel_vel.y > 0.0
			and current_state != AnimStates.FALL and current_state != AnimStates.SHELL):
				current_state = AnimStates.FALL
				_play_anim("fall")
	
	if (shelled and (fire_mode or SInput.input_pressed(SInput.Inputs.STRAFE)
	or (rel_axis.x != 0.0 and grounded) or aim_vector != Vector2.ZERO)):
		_toggle_shell()
	elif (rel_down_pressed and rel_vel.x == 0 and _check_ability(shellable)
	and not (fire_mode or SInput.input_pressed(SInput.Inputs.STRAFE))):
		_toggle_shell()
	
	if (body.is_on_wall() and rel_axis.y != 0 and rel_axis.x == (-1 if facing_left else 1)
	and (_can_grab_wall() or (_can_round_corner_inner() and grounded))):
		var adjustment:Vector2 = Vector2.ZERO
		var new_dir = _get_dir_adjacent_ccw(surface)
		var perform_flip:bool = true
		if facing_left:
			new_dir = _get_dir_adjacent_cw(surface)
		if grounded and rel_axis.y < 0.0 and not _check_ceil_casts()[0] and _can_round_corner_inner():
			adjustment = Vector2(-1 if facing_left else 1, -1) * _get_box_difference()
		elif not grounded:
			grounded = true
			outer_allowed = true
			adjustment = Vector2(-1 if facing_left else 1, 0)
			if _check_ceil_casts()[0]:
				adjustment.y += 1
			if _check_ground_casts()[0]:
				adjustment.y -= 1
			adjustment *= _get_box_difference()
		else:
			perform_flip = false
		if perform_flip:
			_set_direction(new_dir, facing_left)
			body.move_and_collide(_spin_vector_to_surface(adjustment, surface))
			_play_anim("walk")
			current_state = AnimStates.WALK
	elif not grounded:
		if (_can_round_corner_outer()
		and rel_axis.x != 0.0 and rel_axis.y > 0.0 and grounded_last_frame
		and corner_cast.is_colliding() and not _check_ground_casts()[0]):
			var new_dir = _get_dir_adjacent_cw(surface)
			if facing_left:
				new_dir = _get_dir_adjacent_ccw(surface)
			var adjustment:Vector2 = Vector2(1 if facing_left else -1, 1) * _get_box_difference()
			_set_direction(new_dir, facing_left)
			body.move_and_collide(_spin_vector_to_surface(adjustment, surface))
			surface = new_dir
			_play_anim("walk")
			current_state = AnimStates.WALK
			grounded = true
			outer_allowed = true
			rel_vel.y = 720
		elif body.is_on_ceiling():
			if rel_axis.y < 0 and _can_grab_ceiling():
				grounded = true
				outer_allowed = true
				_set_direction(_get_dir_opposite(surface), not facing_left)
				_play_anim("idle" if rel_axis.x == 0.0 else "walk")
				current_state = AnimStates.IDLE if rel_axis.x == 0.0 else AnimStates.WALK
		elif body.is_on_floor() and rel_vel.y >= 0:
			grounded = true
			outer_allowed = true
		elif not _check_ability(retain_gravity_on_airborne) and surface != home_gravity:
			var this_left = facing_left
			if surface == _get_dir_opposite(home_gravity):
				this_left = not this_left
			if _get_dir_opposite(gravity_dir) != home_gravity and gravity_dir != home_gravity:
				rel_vel.x = 0.0
			_set_direction(home_gravity, this_left)
			if not shelled:
				_play_anim("fall")
				current_state = AnimStates.FALL
			coyote_time_counter = coyote_time
			jump_buffer_counter = jump_buffer
	
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
	
	var cur_vel:Vector2 = body.velocity
	var step_vel:Vector2 = cur_vel / MOVE_STEPS
	for i in range(MOVE_STEPS):
		body.velocity = step_vel
		body.move_and_slide()
	body.velocity *= MOVE_STEPS
	position = body.position
	
	if GameCore.instance.current_room.map_ground.get_cell_tile_data(Vector2i(position * Statics.FRAC_16)):
		match surface:
			Statics.DirsSurface.FLOOR:
				if corner_cast.is_colliding():
					body.position += Vector2.UP * 16
				else:
					body.position += 16 * (Vector2.RIGHT if facing_left else Vector2.LEFT)
			Statics.DirsSurface.LWALL:
				if corner_cast.is_colliding():
					body.position += Vector2.RIGHT * 16
				else:
					body.position += 16 * (Vector2.DOWN if facing_left else Vector2.UP)
			Statics.DirsSurface.RWALL:
				if corner_cast.is_colliding():
					body.position += Vector2.LEFT * 16
				else:
					body.position += 16 * (Vector2.UP if facing_left else Vector2.DOWN)
			Statics.DirsSurface.CEILING:
				if corner_cast.is_colliding():
					body.position += Vector2.DOWN * 16
				else:
					body.position += 16 * (Vector2.LEFT if facing_left else Vector2.RIGHT)
		position = body.position
#endregion


#region Movement subfunctions
func _jump() -> float:
	grounded = false
	sfx_jump.play()
	current_state = AnimStates.JUMP
	_play_anim("jump")
	jump_buffer_counter = jump_buffer
	coyote_time_counter = coyote_time
	return jump_power[read_i_jump] * jump_mod


func _jump_and_reorient() -> float:
	grounded = false
	sfx_jump.play()
	jump_buffer_counter = jump_buffer
	coyote_time_counter = coyote_time
	if gravity_dir == _get_dir_opposite(home_gravity):
		_set_direction(home_gravity, not facing_left)
		_test_for_ceiling_reorient_wall_nudge()
	else:
		_push_from_wall()
		_set_direction(home_gravity, facing_left)
	outer_allowed = false
	current_state = AnimStates.FALL
	_play_anim("fall")
	return 0.0


func _jump_decide_state() -> float:
	if gravity_dir != home_gravity and not _check_ability(retain_gravity_on_airborne):
		return _jump_and_reorient()
	else:
		return _jump()


func _push_from_wall() -> void:
	var push_vector:Vector2
	match gravity_dir:
		Statics.DirsSurface.FLOOR:
			push_vector = Vector2.UP * _get_box_difference()
		Statics.DirsSurface.LWALL:
			push_vector = Vector2.RIGHT * _get_box_difference()
		Statics.DirsSurface.RWALL:
			push_vector = Vector2.LEFT * _get_box_difference()
		Statics.DirsSurface.CEILING:
			push_vector = Vector2.DOWN * _get_box_difference()
	body.move_and_collide(push_vector)
	position = body.position


func _test_for_ceiling_reorient_wall_nudge() -> void:
	if not body.is_on_wall():
		return
	var nudge:int = 1
	match gravity_dir:
		Statics.DirsSurface.FLOOR:
			var axis:float = Input.get_axis("left", "right")
			if ((axis < 0 and facing_left) or (axis > 0 and not facing_left)):
				body.position.x += nudge if facing_left else -nudge
		Statics.DirsSurface.LWALL:
			var axis:float = Input.get_axis("up", "down")
			if ((axis < 0 and facing_left) or (axis > 0 and not facing_left)):
				body.position.y += nudge if facing_left else -nudge
		Statics.DirsSurface.RWALL:
			var axis:float = Input.get_axis("down", "up")
			if ((axis < 0 and facing_left) or (axis > 0 and not facing_left)):
				body.position.y += -nudge if facing_left else nudge
		Statics.DirsSurface.CEILING:
			var axis:float = Input.get_axis("right", "left")
			if ((axis < 0 and facing_left) or (axis > 0 and not facing_left)):
				body.position.x += -nudge if facing_left else nudge


func _can_grab_wall() -> bool:
	if stunned and not _check_ability(stick_to_walls_when_hurt):
		return false
	return _check_ability(can_swap_gravity)


func _can_grab_ceiling() -> bool:
	if stunned and not _check_ability(stick_to_walls_when_hurt):
		return false
	return _check_ability(can_swap_gravity)


func _can_round_corner_inner() -> bool:
	if stunned and not _check_ability(stick_to_walls_when_hurt):
		return false
	return _check_ability(can_swap_gravity) and _check_ability(can_round_inner_corners)


func _can_round_corner_outer() -> bool:
	if stunned and not _check_ability(stick_to_walls_when_hurt):
		return false
	if not _check_ability(can_swap_gravity) and not _check_ability(can_round_outer_corners):
		return false
	if not outer_allowed or just_jumped:
		return false
	var can_outer:bool = _check_ability(can_round_opposite_outer_corners)
	if gravity_dir == _get_dir_adjacent_cw(home_gravity):
		return true if facing_left else can_outer
	elif gravity_dir == _get_dir_adjacent_ccw(home_gravity):
		return can_outer if facing_left else true
	elif gravity_dir == _get_dir_opposite(home_gravity):
		return can_outer
	else:
		return true
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
	environment_exit_override = 2
	box_normal.disabled = state
	box_shell.disabled = not state
	for cast in normal_casts:
		cast.enabled = not state
	for cast in shell_casts:
		cast.enabled = state
	if state:
		sfx_shell.play()
		_play_anim("shell")
		current_state = AnimStates.SHELL
		if Statics.check_item(Item.ItemTypes.SHELL_SHIELD):
			shield_particle = Statics.spawn_particle("Shield", Room.Layers.GROUND, position)
	else:
		_play_anim("unshell")
		if shield_particle:
			Statics.spawn_particle("ShieldPop", Room.Layers.GROUND, shield_particle.position)
			shield_particle.queue_free()


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


# Returns the difference of both size axes of the currently active hitbox
func _get_box_difference() -> float:
	var box_shape:RectangleShape2D
	if shelled:
		box_shape = box_shell.shape
	else:
		box_shape = box_normal.shape
	return abs(box_shape.size.x - box_shape.size.y)


# Takes in a vector and a target surface, and rotates the vector from the floor to whatever the target surface is
# Input  - the input vector
#        - the target surface
# Output - the rotated vector
func _spin_vector_to_surface(input:Vector2, surface:Statics.DirsSurface) -> Vector2:
	match surface:
		Statics.DirsSurface.LWALL:
			return Vector2(-input.y, input.x)
		Statics.DirsSurface.RWALL:
			return Vector2(input.y, -input.x)
		Statics.DirsSurface.CEILING:
			return -input
	return input


# Takes an action name, considers the current state of the player, and sets the player's
# JsonSprite2D animation appropriately
# Input  - the action to perform
func _play_anim(action:String):
	var full_action = str(shell_level_displayed) + "."
	
	if action != "death":
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


# Externally called; updates which animation set the player uses based on shell level
# Input  - the level of shell to display
#        - the way in which to use the new shell level where
#               0 - Set bit (sets the displayed shell to the new value via bit index)
#               1 - Set explicit (sets the displayed shell to the new value, as is)
#               2 - Toggle (Flips the specified bit)
func update_shell_displayed(new_shell:int, mode:int) -> void:
	match mode:
		0:
			shell_level_displayed = 1 << (new_shell - 1)
		1:
			shell_level_displayed = new_shell
		2:
			shell_level_displayed = shell_level_displayed ^ new_shell
	_play_anim("idle")


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
	#return
	override_box_disable = state
	if state == true:
		box_normal.set_deferred("disabled", true)
		box_shell.set_deferred("disabled", true)
	else:
		box_normal.set_deferred("disabled", shelled)
		box_shell.set_deferred("disabled", not shelled)


func adjust_health(amount:int, ignore_defense:bool = false) -> void:
	if amount < 0 and in_death_cutscene:
		return
	
	var shielded:bool = false
	if amount < 0 and shelled and Statics.check_item(Item.ItemTypes.SHELL_SHIELD) and not ignore_defense:
		amount = 0
		shielded = true
	health += amount
	health = clampi(health, 0, max_health)
	UICore.instance.update_hearts()
	if health == 0:
		tick_death(0.0)
	elif amount < 0 or shielded:
		if shelled:
			_set_shell(false)
		# Disabled gravity shock
		if not _check_ability(stick_to_walls_when_hurt) and gravity_dir != home_gravity and not _check_ceil_casts()[0]:
			if gravity_dir != _get_dir_opposite(home_gravity):
				_push_from_wall()
			_set_direction(home_gravity, facing_left)
			outer_allowed = false
			_play_anim("fall")
			body.velocity = Vector2.ZERO
		stunned = true
		stun_timer = MAX_STUN_TIMER
		if shielded:
			sfx_ping.play()
		else:
			sfx_hurt.play()


func tick_death(_delta:float) -> void:
	if not in_death_cutscene:
		in_death_cutscene = true
		sfx_death.play()
		timer_die_fade.start()
		timer_die_respawn.start()
		GameCore.instance.music_manager.set_fade(0.0, 1.25)
		respawn_i_frames = RESPAWN_INVIN_TIMER
		override_box_disable = true


func _on_death_fade_timeout() -> void:
	var fade_color:Color = Statics.get_color(Vector2i(0, 0))
	var fade_color_a:Color = fade_color
	fade_color_a.a = 0.0
	UICore.instance.color_cover.set_new_fade(fade_color_a, fade_color, 0.75)


func _on_respawn_timeout() -> void:
	GameCore.instance.music_manager.stop_all()
	GameCore.instance.music_manager.set_global_volume(1.0)
	
	var fade_color:Color = Statics.get_color(Vector2i(0, 0))
	var fade_color_a:Color = fade_color
	fade_color_a.a = 0.0
	UICore.instance.color_cover.set_new_fade(fade_color, fade_color_a, 0.25)
	
	var load_pos = Statics.current_profile["save_coords"]
	if load_pos is String:
		load_pos = str_to_var("Vector2i" + load_pos)
	Statics.load_room = Statics.ROOM_PATH % str(Statics.current_profile["save_room"])
	Statics.load_coords = load_pos
	
	GameCore.instance.spawn_room(Statics.load_room)
	UICore.instance.cam.set_layer_position(Statics.load_coords)
	UICore.instance.clear_area_text()
	UICore.instance.clear_boss_bar()
	adjust_health(999999)
	stun_timer = MAX_STUN_TIMER
	reset_position(Statics.load_coords)
	set_deferred("override_box_disable", false)
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
	Statics.current_profile["equipped_weapons"] = selected_weapon
	UICore.instance.update_weapon_icons()


func _shoot(_bullet_id:int, normalized_velocity:Vector2, pos:Vector2 = body.position) -> float:
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
	if Statics.check_item(Item.ItemTypes.DEVASTATOR):
		bullet_type += "Power"
	var bullet_scene = load("res://Scenes/Entities/Bullets/Player/PlayerBullet" + bullet_type + ".tscn")
	var new_bullet:PlayerBullet = bullet_scene.instantiate()
	GameCore.instance.current_room.layer_fg1.add_child(new_bullet)
	new_bullet.position = pos
	if pos == body.position:
		new_bullet.position += normalized_velocity * Statics.FRAC_8
	var rapid_mult:float = 2.0 if Statics.check_item(Item.ItemTypes.RAPID_FIRE) else 1.0
	var this_cooldown = new_bullet._spawn(normalized_velocity, rapid_mult)
	return this_cooldown
#endregion


#region Cutscene functions
func impulse(_direction:Vector2) -> bool:
	return false


func glide_to(_position:Vector2, _duration:float) -> bool:
	return false


func get_dialogue_icon() -> Texture:
	return null


func fake_input(_event:InputEventAction, _hold_for:float) -> bool:
	return false


func look_at_position(_pos:Vector2) -> bool:
	return false


func look_at_local(_pos:Vector2) -> bool:
	return false


func look_at_node(_node:Node2D) -> bool:
	return false


func lock_inputs(_locked:bool) -> bool:
	return false


func has_item(_ID:Item.ItemTypes) -> bool:
	return false


func can_perform_action(_action:String) -> bool:
	return false


func perform_action(_action:String, _force:bool) -> bool:
	return false
#endregion
