extends Particle


const EXTENTS:float = 20.0
const COUNT:int = 8
const BUMP_DIST:float = 8.0

var direction:Statics.DirsSurface = Statics.DirsSurface.FLOOR


func _spawn(_data:Array) -> void:
	super(_data)
	
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENTITIES_ALL
	or option == Statics.ParticleOptions.ALL):
		queue_free()
		return
	
	if _data.size() > 0 and _data[0] is Statics.DirsSurface:
		direction = _data[0]
	
	for i in range(COUNT):
		var rand_pos:Vector2 = position + Vector2(
			randf_range(-EXTENTS, EXTENTS),
			randf_range(-EXTENTS, EXTENTS),
		)
		match direction:
			Statics.DirsSurface.FLOOR: rand_pos += Vector2(0, BUMP_DIST)
			Statics.DirsSurface.LWALL: rand_pos += Vector2(BUMP_DIST, 0)
			Statics.DirsSurface.RWALL: rand_pos += Vector2(-BUMP_DIST, 0)
			Statics.DirsSurface.CEILING: rand_pos += Vector2(0, -BUMP_DIST)
		var new_whoosh:Particle = Statics.spawn_particle("GravWhoosh", Room.Layers.GROUND, rand_pos, [direction])
		if randf() <= 0.5:
			new_whoosh.z_index *= -1
