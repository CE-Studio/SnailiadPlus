class_name RoomLoader
extends Node


#region Variables
var load_queue:Array[String] = []
var clear_queue:Array[String] = []
var stored_rooms:Array[Resource] = []
var room_keys:Array[String] = []
#endregion


func _process(_delta):
	if load_queue.size() > 0:
		var new_queue:Array[String] = []
		for i in range(load_queue.size()): # Add rooms that are done loading to the storage array
			if ResourceLoader.load_threaded_get_status(load_queue[i], []) == ResourceLoader.THREAD_LOAD_LOADED:
				stored_rooms.append(ResourceLoader.load_threaded_get(load_queue[i]))
				room_keys.append(load_queue[i])
			else:
				new_queue.append(load_queue[i])
		load_queue = new_queue.duplicate()
	
	if clear_queue.size() > 0:
		var new_queue:Array[String] = []
		for i in range(clear_queue.size()):
			if room_keys.has(clear_queue[i]): # If a room queued for deletion exists, remove it
				var this_i:int = room_keys.find(clear_queue[i])
				#print("Removed %s" % room_keys[this_i])
				room_keys.remove_at(this_i)
				stored_rooms.remove_at(this_i)
			else:
				new_queue.append(clear_queue[i])
		clear_queue = new_queue.duplicate()


func request_load(path:String):
	load_queue.append(path)
	ResourceLoader.load_threaded_request(path)
	#print("Added %s to the queue" % path)


func get_room(path:String) -> Resource:
	if room_keys.has(path): # If the room is loaded, return it from storage
		return stored_rooms[room_keys.find(path)]
	if load_queue.has(path): # Else, if it's in the queue, block thread until it finishes loading
		return ResourceLoader.load_threaded_get(path) 
	return load(path) # Else, load it normally


func request_clear(path:String):
	clear_queue.append(path)
	#print("Attempting to remove %s" % path)
