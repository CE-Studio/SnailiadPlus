# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name PlayerBulletGravShock
extends PlayerBullet


const SPRITE_ADJUST:float = 16.0


func _spawn(dir:Vector2, rapid_shot:float, power_shot:bool) -> float:
	if dir == Vector2.ZERO:
		dir = Vector2.DOWN
	super._spawn(dir, rapid_shot, power_shot)
	var anim_name:String = "D_"
	match dir:
		Vector2.RIGHT:
			anim_name = "R_"
			sprite.position += Vector2.LEFT * SPRITE_ADJUST
		Vector2.LEFT:
			anim_name = "L_"
			sprite.position += Vector2.RIGHT * SPRITE_ADJUST
		Vector2.UP:
			anim_name = "U_"
			sprite.position += Vector2.DOWN * SPRITE_ADJUST
		_:
			sprite.position += Vector2.UP * SPRITE_ADJUST
	var color:String
	match Player.instance.who_i_is:
		Player.Players.SNAILY: color = "pink" if powered else "blue"
		Player.Players.SLUGGY: color = "white" if powered else "green"
		Player.Players.UPSIDE: color = "pink" if powered else "blue"
		Player.Players.LEGGY: color = "red" if powered else "yellow"
		Player.Players.BLOBBY: color = "red" if powered else "white"
		Player.Players.LEECHY: color = "white" if powered else "green"
	sprite.play(anim_name + color)
	if dir == Vector2.LEFT:
		sprite.flip_h = true
	if dir == Vector2.UP:
		sprite.flip_v = true
	return 0.0
