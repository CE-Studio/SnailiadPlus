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
static var active_object_id:String
static var current_scene:DialogueResource
static var current_animator:AnimationPlayer
static var running := false


@onready var notif:TextureRect = $CanvasLayer/TextureRect


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


static func _process_dia() -> void:
	running = true
	current_scene.reset_state()
	var line:DialogueLine = await current_scene.get_next_dialogue_line()
	while is_instance_valid(line) and running:
		print(line.text)
		print(line.next_id)
		line = await current_scene.get_next_dialogue_line(line.next_id)
		await StaticProcess.cut_advance
	running = false
	SInput.cutscene_has_control = false
	instance.notif.visible = false


static func lock_input() -> void:
	SInput.cutscene_has_control = true
	if is_instance_valid(instance):
		instance.notif.visible = true


static func unlock_input() -> void:
	SInput.cutscene_has_control = false
	if is_instance_valid(instance):
		instance.notif.visible = false


static func set_actor(actor:String) -> void:
	active_object_id = actor


static func perform_action(action:String, force:bool) -> void:
	var actor := get_actor(active_object_id)
	if not is_instance_valid(actor):
			return
	if not actor.can_perform_action(action):
		return
	actor.perform_action(action, force)


static func get_actor(id:String) -> CutsceneControllable:
	for i in CutsceneControllable.actors:
		if i.identifier == id:
			return i
	return null


static func start(dia:DialogueResource, anim:AnimationPlayer, initiator:String) -> void:
	if running:
		return
	if not is_instance_valid(dia):
		return
	current_scene = dia
	current_animator = anim
	object_id = initiator
	_process_dia()
