@tool
class_name IceSpike
extends Hazard


@export var direction:Statics.DirsSurface = Statics.DirsSurface.FLOOR:
	set(value):
		if value == Statics.DirsSurface.NONE:
			value = Statics.DirsSurface.FLOOR
		direction = value
		if Engine.is_editor_hint():
			var sprite = $"JsonSprite2D/MarkerSprite"
			match direction:
				Statics.DirsSurface.FLOOR:
					sprite.rotation_degrees = 0.0
				Statics.DirsSurface.LWALL:
					sprite.rotation_degrees = 90.0
				Statics.DirsSurface.RWALL:
					sprite.rotation_degrees = -90.0
				Statics.DirsSurface.CEILING:
					sprite.rotation_degrees = 180.0


func _ready() -> void:
	super()
	var sprite = $"JsonSprite2D"
	match direction:
		Statics.DirsSurface.FLOOR:
			sprite.action = "D"
		Statics.DirsSurface.LWALL:
			sprite.action = "L"
		Statics.DirsSurface.RWALL:
			sprite.action = "R"
		Statics.DirsSurface.CEILING:
			sprite.action = "U"
