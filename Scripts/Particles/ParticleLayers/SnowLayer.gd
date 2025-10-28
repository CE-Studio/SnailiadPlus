class_name SnowLayer
extends ParticleLayer


const FLAKES_PER_UNIT:int = 60

@export_range(0.1, 16.0, 0.1) var density:float = 1:
	set(value):
		density = value
		particle_count = roundi(FLAKES_PER_UNIT * density)


func spawn() -> void:
	var option:int = ProjectSettings.get_setting("game/world/particles")
	if not (option == Statics.ParticleOptions.ENVIRONMENTS
	or option == Statics.ParticleOptions.FLASH
	or option == Statics.ParticleOptions.ALL):
		queue_free()
	else:
		super()
