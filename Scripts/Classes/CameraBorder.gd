@icon("uid://el86xs2vjg0b")
class_name CameraBorder
extends Path2D


func _ready() -> void:
	assert(global_position == Vector2.ZERO, "Camera border node MUST be centered")
	UICore.instance.cam.border = self


func get_closest_point_to(pos:Vector2) -> Vector2:
	if Geometry2D.is_point_in_polygon(pos, curve.get_baked_points()):
		return Vector2(pos)
	return curve.get_closest_point(pos)


func get_bound_extremes() -> Vector4:
	var lower:Vector2 = Vector2(999999, 999999)
	var upper:Vector2 = Vector2(-999999, -999999)
	for i in curve.point_count:
		var point = curve.get_point_position(i)
		if point.x < lower.x:
			lower.x = point.x
		if point.x > upper.x:
			upper.x = point.x
		if point.y < lower.y:
			lower.y = point.y
		if point.y > upper.y:
			upper.y = point.y
	return Vector4(lower.x, lower.y, upper.x, upper.y)


func get_bound_size() -> Vector2:
	var bounds = get_bound_extremes()
	return Vector2(
		abs(bounds.z - bounds.x),
		abs(bounds.w - bounds.y)
	)


func get_center() -> Vector2:
	var bounds = get_bound_extremes()
	return Vector2(
		(bounds.x + bounds.z) * 0.5,
		(bounds.y + bounds.w) * 0.5
	)
