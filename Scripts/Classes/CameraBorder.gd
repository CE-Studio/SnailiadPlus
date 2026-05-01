# Copyright 2026 CE-Studio: AGPL-3.0-only
@icon("uid://el86xs2vjg0b")
@tool
class_name CameraBorder
extends Path2D


@export var point_ratio_adjustments:Array[Vector2i] = []
@export var editor_show_borders:bool = false:
	set(value):
		editor_show_borders = value
		queue_redraw()

const EDITOR_BORDER_WIDTH:int = 3

## Array that tracks the original positions of all points in the border, in order to properly
## offset each point for different aspect ratios.
var point_origins:Array[Vector2i] = []
## Ensures that the curve remains intact while in the editor
var last_state:Curve2D = curve


func _ready() -> void:
	if Engine.is_editor_hint():
		return
	assert(global_position == Vector2.ZERO, "Camera border node MUST be centered")
	if UICore.instance == null:
		return
	UICore.instance.cam.border = self
	for i in curve.point_count:
		point_origins.append(Vector2i(curve.get_point_position(i)))
	while point_ratio_adjustments.size() < curve.point_count:
		point_ratio_adjustments.append(Vector2i.ZERO)
	var current_ratio = ProjectSettings.get_setting("display/window/size/aspect_ratio")
	if current_ratio != 0:
		replot_points(Statics.ASPECT_RATIO_OFFSETS[current_ratio])


func _process(_delta: float) -> void:
	if Engine.is_editor_hint():
		if curve != last_state:
			curve = last_state
			if editor_show_borders:
				queue_redraw()


## Rebuilds the border in accordance with any offsets required by the current aspect ratio
func replot_points(offsets:Vector2i) -> void:
	curve.clear_points()
	for i in range(point_origins.size()):
		var normalized_offset = Vector2i(
			clampi(point_ratio_adjustments[i].x, -1, 1),
			clampi(point_ratio_adjustments[i].y, -1, 1)
		)
		curve.add_point(point_origins[i] + Vector2i(offsets * normalized_offset * 0.5))


## Returns the closest point along the border to the input point in world space
func get_closest_point_to(pos:Vector2) -> Vector2:
	if Geometry2D.is_point_in_polygon(pos, curve.get_baked_points()):
		return Vector2(pos)
	return curve.get_closest_point(pos)


## Returns the lowest and highest points along both axes that the border reaches
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


## Returns the size of the smallest rectangle that fully encloses the border
func get_bound_size() -> Vector2:
	var bounds = get_bound_extremes()
	return Vector2(
		abs(bounds.z - bounds.x),
		abs(bounds.w - bounds.y)
	)


## Returns the center of the border in world space relative to its extremes
func get_center() -> Vector2:
	var bounds = get_bound_extremes()
	return Vector2(
		(bounds.x + bounds.z) * 0.5,
		(bounds.y + bounds.w) * 0.5
	)


func _draw() -> void:
	if Engine.is_editor_hint() and editor_show_borders:
		for i in range(curve.point_count):
			var pos:Vector2 = curve.get_point_position(i)
			var bounds:Vector2 = Statics.VECTOR_CENTER
			draw_line(
				Vector2(pos.x - bounds.x, pos.y - bounds.y),
				Vector2(pos.x - bounds.x, pos.y + bounds.y),
				Color.WHITE, EDITOR_BORDER_WIDTH
			)
			draw_line(
				Vector2(pos.x - bounds.x, pos.y - bounds.y),
				Vector2(pos.x + bounds.x, pos.y - bounds.y),
				Color.WHITE, EDITOR_BORDER_WIDTH
			)
			draw_line(
				Vector2(pos.x + bounds.x, pos.y + bounds.y),
				Vector2(pos.x - bounds.x, pos.y + bounds.y),
				Color.WHITE, EDITOR_BORDER_WIDTH
			)
			draw_line(
				Vector2(pos.x + bounds.x, pos.y + bounds.y),
				Vector2(pos.x + bounds.x, pos.y - bounds.y),
				Color.WHITE, EDITOR_BORDER_WIDTH
			)
