class_name CCPlayer
extends Node2D


var active:bool = false
var last_position:Vector2 = Vector2.ZERO
var player:Player = null


func _ready() -> void:
	last_position = position
	if GameCore.instance:
		player = GameCore.instance.player


func _physics_process(_delta: float) -> void:
	if not active:
		return
	if last_position != position:
		Player.instance.body.position = position
		last_position = position


func set_active(surface:Statics.DirsSurface = player.gravity_dir, facing:bool = player.facing_left) -> void:
	active = true
	set_direction(surface, facing)


func set_inactive() -> void:
	active = false


#region Player direction setters
func reset_player_direction() -> void:
	player._set_direction(player.home_gravity, player.facing_left)


func look_left() -> void:
	player._set_direction(player.gravity_dir, true)


func look_right() -> void:
	player._set_direction(player.gravity_dir, false)


func set_direction(surface:Statics.DirsSurface, facing:bool = player.facing_left):
	player._set_direction(surface, facing)
#endregion


#region Player actions
func play_anim(anim:String) -> void:
	player._play_anim(anim)


func jump(full:bool = false) -> void:
	var jump_vel:float = player._full_jump() if full else player._jump()
	match player.gravity_dir:
		Statics.DirsSurface.FLOOR: player.body.velocity = Vector2.DOWN * jump_vel
		Statics.DirsSurface.LWALL: player.body.velocity = Vector2.LEFT * jump_vel
		Statics.DirsSurface.RWALL: player.body.velocity = Vector2.RIGHT * jump_vel
		Statics.DirsSurface.CEILING: player.body.velocity = Vector2.UP * jump_vel


func grav_jump(dir:Statics.DirsSurface) -> void:
	player._grav_jump(dir)


func shoot(bullet:int, dir:Vector2) -> void:
	player._shoot(bullet, dir)
#endregion


func do_emote(emote:EmoteLayer.Emotes, duration:float = -1.0) -> void:
	player.emote.emote_from_enum(emote, duration)
