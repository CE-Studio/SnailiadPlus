# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


func _spawn(_data:Array) -> void:
	super(_data)
	
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENTITIES_ALL
	or option == Statics.ParticleOptions.ALL):
		queue_free()
		return
	
	var character:String = "snaily"
	match Player.instance.who_i_is:
		Player.Players.SLUGGY: character = "sluggy"
		Player.Players.UPSIDE: character = "upside"
		Player.Players.LEGGY: character = "leggy"
		Player.Players.BLOBBY: character = "blobby"
		Player.Players.LEECHY: character = "leechy"
	var direction:String = "down"
	match Player.instance.gravity_dir:
		Statics.DirsSurface.LWALL: direction = "left"
		Statics.DirsSurface.RWALL: direction = "right"
		Statics.DirsSurface.CEILING: direction = "up"
	var power:String = "power" if Statics.check_item(Item.ItemTypes.METAL_SHELL) else "normal"
	var step:String = "0"
	if _data.size() > 0 and _data[0] is int:
		step = str(abs(_data[0]) % 4)
	var anim_name:String = ".".join([character, direction, power, step])
	if not sprite.sprite_frames.has_animation(anim_name):
		anim_name = ".".join([character, "down", power, step])
	sprite.play(anim_name)
	if character == "leggy":
		if direction == "left": sprite.flip_h = true
		elif direction == "down": sprite.flip_v = true
