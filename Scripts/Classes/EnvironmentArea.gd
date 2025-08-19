@icon("res://Editor/ico/EnvironmentArea.svg")
class_name EnvironmentArea
extends Area2D

#region Variables
var spawn_grace_frames = 4
var contained_bodies:Array = []
var boxes:Array[CollisionShape2D] = []
var polys:Array[Polygon2D] = []
var read_interactions:bool = true
#endregion


func _ready() -> void:
	for child in get_children():
		if child is CollisionShape2D:
			if child.shape is RectangleShape2D:
				boxes.append(child)
		elif child is Polygon2D:
			polys.append(child)
	update_shader_visibility()


func _physics_process(delta: float) -> void:
	if spawn_grace_frames > 0:
		spawn_grace_frames -= 1


func update_shader_visibility() -> void:
	var is_visible = ProjectSettings.get_setting("game/visuals/distortion_shader")
	for poly in polys:
		poly.visible = is_visible
		if is_visible:
			var ratio_id = ProjectSettings.get_setting("display/window/size/aspect_ratio")
			poly.material.set("shader_parameter/aspect_ratio", Statics.ASPECT_RATIOS[ratio_id])


func _on_body_enter(body) -> void:
	if contained_bodies.has(body):
		return
	contained_bodies.append(body)
	if body is Enemy:
		body.environment = self


func _on_body_exit(body) -> void:
	if not contained_bodies.has(body):
		return
	if body.get_parent() is Player:
		if body.get_parent().environment_exit_override > 0:
			return
	contained_bodies.remove_at(contained_bodies.find(body))
	if body is Enemy and body.environment == self:
		body.environment = null


func get_closest_point(in_point:Vector2) -> Array:
	var shortest_distance = -1
	var out_point = Vector2.ZERO
	var normal_dir = Vector2.ZERO
	var closest_box = null
	for box in boxes:
		var rect:RectangleShape2D = box.shape
		var r_top = box.global_position.y - (rect.size.y * 0.5)
		var r_bottom = box.global_position.y + (rect.size.y * 0.5)
		var r_left = box.global_position.x - (rect.size.x * 0.5)
		var r_right = box.global_position.x + (rect.size.x * 0.5)
		
		var d_top = abs(r_top - in_point.y)
		var d_bottom = abs(r_bottom - in_point.y)
		var d_left = abs(r_left - in_point.x)
		var d_right = abs(r_right - in_point.x)
		
		var corner_y = r_top if d_top < d_bottom else r_bottom
		var corner_x = r_left if  d_left < d_right else r_right
		var d_cx = corner_x - in_point.x
		var d_cy = corner_y - in_point.y
		var d_corner = sqrt((d_cx * d_cx) + (d_cy * d_cy))
		
		var d_final = min(d_top, d_bottom, d_left, d_right, d_corner)
		
		var match_edge:bool = false
		if (r_left < in_point.x and in_point.x < r_right
		and r_top < in_point.y and in_point.y < r_bottom):
			match_edge = true
		
		if shortest_distance == -1 or shortest_distance > d_final:
			shortest_distance = d_final
			match d_final:
				d_top:
					normal_dir = Vector2.UP
				d_bottom:
					normal_dir = Vector2.DOWN
				d_left:
					normal_dir = Vector2.LEFT
				d_right:
					normal_dir = Vector2.RIGHT
				d_corner:
					normal_dir = Vector2.UP if corner_y == r_top else Vector2.DOWN
			closest_box = box
			out_point = in_point + (shortest_distance * normal_dir * (1 if match_edge else -1))
	return [ out_point, normal_dir, closest_box ]
