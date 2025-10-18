class_name Preloader
extends Node2D


#region Variables
var thread:Thread = Thread.new()
var unprocessed_marker_positions:Array = []
@export var gui_debug := false
@onready var file_label:Label = %FileLabel
@onready var progress_bar:ProgressBar = %ProgressBar
#endregion


func _ready() -> void:
	%"JsonSprite2D".action = "idle"
	if gui_debug:
		return
	SInput.rebind_all()
	if Minimap.unprocessed_marker_positions.size() == 0:
		if OS.has_feature("nothreads"):
			_read_rooms_log_markers()
		else:
			thread.start(_read_rooms_log_markers)
			while thread.is_alive():
				await  get_tree().process_frame
			thread.wait_to_finish()
		Minimap.unprocessed_marker_positions = unprocessed_marker_positions
	if Statics.shortcut_load_game_scene:
		Statics.shortcut_load_game_scene = false
		get_tree().call_deferred("change_scene_to_file", "res://Scenes/GameScene.tscn")
	else:
		get_tree().call_deferred("change_scene_to_file", "res://Scenes/MenuScene.tscn")


func _read_rooms_log_markers() -> void:
	var base_path:String = Statics.ROOM_PATH
	base_path = base_path.split("%s")[0]
	var all_rooms:Array = _grab_files_recursive(base_path)
	unprocessed_marker_positions.resize(Minimap.DEFAULT_MAP.size())
	unprocessed_marker_positions.fill(Minimap.MarkerTypes.NONE)
	var count_max := all_rooms.size()
	var count:int = 0
	progress_bar.set_thread_safe(&"max_value", count_max)
	for room in all_rooms:
		count += 1
		progress_bar.set_thread_safe(&"value", count)
		file_label.set_thread_safe(&"text", room)
		var room_scene = load(room).instantiate()
		if room_scene is Room:
			var room_children:Array = _grab_nodes_recursive(room_scene)
			for child in room_children:
				if child is SavePoint or child is Item or child is Boss:
					var screen_pos = Minimap.world_position_to_screen_coordinate(child.position)
					screen_pos += room_scene.minimap_offset
					var array_i = screen_pos.x + (screen_pos.y * Minimap.MAP_SIZE.x)
					if child is SavePoint:
						unprocessed_marker_positions[array_i] = Minimap.MarkerTypes.SAVE
					if child is Item:
						unprocessed_marker_positions[array_i] = [ Minimap.MarkerTypes.ITEM, child.location_id ]
					if child is Boss:
						unprocessed_marker_positions[array_i] = Minimap.MarkerTypes.BOSS
		room_scene.queue_free()


func _grab_files_recursive(path:String, files:Array = []) -> Array:
	for file in DirAccess.get_files_at(path):
		if file.get_extension() == "remap":
			file = file.get_basename()
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
