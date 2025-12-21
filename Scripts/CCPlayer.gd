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


func do_emote(emote:EmoteLayer.Emotes, duration:float = -1.0) -> void:
	player.emote.emote_from_enum(emote, duration)
