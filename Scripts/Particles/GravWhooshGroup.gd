# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


const EXTENTS:float = 20.0
const COUNT:int = 8
const COUNT_FLAT:int = 4
const BUMP_DIST:float = 8.0

var direction:Statics.DirsSurface = Statics.DirsSurface.FLOOR
var flat:bool = false


func _spawn(_data:Array) -> void:
	super(_data)
	
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENTITIES_ALL
	or option == Statics.ParticleOptions.ALL):
		queue_free()
		return
	
	if _data.size() > 0 and _data[0] is Statics.DirsSurface:
		direction = _data[0]
	if _data.size() > 1 and _data[1] is bool:
		flat = _data[1]
	
	for i in range(COUNT_FLAT if flat else COUNT):
		var rand_pos:Vector2 = position + Vector2(
			randf_range(-EXTENTS, EXTENTS),
			randf_range(-EXTENTS, EXTENTS),
		)
		if flat:
			if direction == Statics.DirsSurface.LWALL or direction == Statics.DirsSurface.RWALL:
				rand_pos.x = position.x + (-BUMP_DIST if direction == Statics.DirsSurface.LWALL else BUMP_DIST)
			else:
				rand_pos.y = position.y + (BUMP_DIST if direction == Statics.DirsSurface.FLOOR else -BUMP_DIST)
		if not flat:
			match direction:
				Statics.DirsSurface.FLOOR: rand_pos += Vector2(0, BUMP_DIST)
				Statics.DirsSurface.LWALL: rand_pos += Vector2(BUMP_DIST, 0)
				Statics.DirsSurface.RWALL: rand_pos += Vector2(-BUMP_DIST, 0)
				Statics.DirsSurface.CEILING: rand_pos += Vector2(0, -BUMP_DIST)
		var new_whoosh:Particle = Statics.spawn_particle("GravWhoosh", Room.Layers.GROUND, rand_pos, [direction])
		if randf() <= 0.5:
			new_whoosh.z_index *= -1
