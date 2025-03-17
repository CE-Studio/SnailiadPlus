@icon("uid://el86xs2vjg0b")
class_name CameraBorder
extends Path2D


func _ready() -> void:
	assert(global_position == Vector2.ZERO, "Camera border node MUST be centered")
	GameCore.instance.cam_layer.border = self


func get_closest_point_to(pos:Vector2) -> Vector2:
	if Geometry2D.is_point_in_polygon(pos, curve.get_baked_points()):
		return Vector2(pos)
	return curve.get_closest_point(pos)
