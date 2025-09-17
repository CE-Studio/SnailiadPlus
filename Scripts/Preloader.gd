class_name Preloader
extends Node2D


#region Variables

#endregion


func _ready() -> void:
	$"JsonSprite2D".action = "idle"
	if Minimap.unprocessed_marker_positions.size() == 0:
		await _read_rooms_log_markers()
	get_tree().call_deferred("change_scene_to_file", "res://Scenes/MenuScene.tscn")


func _read_rooms_log_markers() -> void:
	var base_path:String = Statics.ROOM_PATH
	base_path = base_path.split("%s")[0]
	var all_rooms:Array = _grab_files_recursive(base_path)
	Minimap.unprocessed_marker_positions.resize(Minimap.DEFAULT_MAP.size())
	Minimap.unprocessed_marker_positions.fill(Minimap.MarkerTypes.NONE)
	for room in all_rooms:
		var room_scene = load(room).instantiate()
		if room_scene is Room:
			var room_children:Array = _grab_nodes_recursive(room_scene)
			for child in room_children:
				if child is SavePoint or child is Item or child is Boss:
					var screen_pos = Minimap.world_position_to_screen_coordinate(child.position)
					screen_pos += room_scene.minimap_offset
					var array_i = screen_pos.x + (screen_pos.y * Minimap.MAP_SIZE.x)
					if child is SavePoint:
						Minimap.unprocessed_marker_positions[array_i] = Minimap.MarkerTypes.SAVE
					if child is Item:
						Minimap.unprocessed_marker_positions[array_i] = [ Minimap.MarkerTypes.ITEM, child.location_id ]
					if child is Boss:
						Minimap.unprocessed_marker_positions[array_i] = Minimap.MarkerTypes.BOSS


func _grab_files_recursive(path:String, files:Array = []) -> Array:
	for file in DirAccess.get_files_at(path):
		files.append(path + file)
	for dir in DirAccess.get_directories_at(path):
		files.append_array(_grab_files_recursive(path + dir + "/"))
	return files


func _grab_nodes_recursive(root:Node, nodes:Array = []) -> Array:
	for node in root.get_children():
		nodes.append(node)
		if node.get_child_count() > 0:
			nodes.append_array(_grab_nodes_recursive(node))
	return nodes
