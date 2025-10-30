class_name CutsceneController
extends Node


enum Status {
	OK,
	BUSY,
	FILE_ERROR,
	UNKOWN_ERROR,
}


static var instance:CutsceneController
static var object_id:String


static func store_flag(flag:StringName) -> void:
	assert(flag.is_valid_ascii_identifier(), "Invalid flag name!")
	if is_instance_valid(instance):
		instance.set_meta(flag, true)


static func clear_flag(flag:StringName) -> void:
	if is_instance_valid(instance):
		instance.set_meta(flag, null)


static func has_flag(flag:StringName) -> bool:
	if is_instance_valid(instance):
		return instance.has_meta(flag)
	return false


static func save_flags() -> Array[StringName]:
	if is_instance_valid(instance):
		return instance.get_meta_list()
	return []


static func load_flags(flags:Array[StringName]) -> void:
	if is_instance_valid(instance):
		var old_flags := instance.get_meta_list()
		for i in old_flags:
			instance.set_meta(i, null)
	for i in flags:
		store_flag(i)


func _process(_delta: float) -> void:
	pass


func _ready() -> void:
	instance = self


func _sim_step() -> void:
	pass
