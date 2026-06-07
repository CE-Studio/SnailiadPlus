# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name CutsceneController
extends Node


enum Status {
	OK,
	BUSY,
	FILE_ERROR,
	UNKOWN_ERROR,
}

const MIN_SOUND_ELAPSED:float = 0.0333
const DEFAULT_NEXT_TIMEOUT:float = 3.0

## The currently active instance of the [CutsceneController] class
static var instance:CutsceneController
## Fallback actor to reference during cutscenes, usually set as whichever actor started a cutscene
static var object_id:String
## The actor currently being targeted for action calls
static var active_object_id:String
## The current [DialogueResource] script attached to the active cutscene
static var current_scene:DialogueResource
## The current [AnimationPlayer] tied to the active cutscene
static var current_animator:AnimationPlayer
## Set to [code]true[/code] if a cutscene is currently active
static var running := false
## Records whether or not the dialogue box is currently open
static var _boxtrack := false
## The time in seconds since the last time the dialogue sound was played
static var _sound_elapsed:float = 0.0
## The time in seconds any non-controlling dialogue waits before advancing to the next line
static var _next_line_timeout:float = DEFAULT_NEXT_TIMEOUT
## Will be set if the currently printed line needs to be skipped or not as per an external call
static var _remote_skip_flag:bool = false

## A small arrow texture drawn on the dialogue box when advancing dialogue is available
@onready var advancearrow: Control = $CanvasLayer/Control/text/PanelContainer/advancearrow
## Marks the screen position the dialogue box should rest at when in the top half of the screen
@onready var toptargpos:Control = $CanvasLayer/Control/toptargpos
## Marks the screen position the dialogue box should rest at when in the bottom half of the screen
@onready var bottargpos:Control = $CanvasLayer/Control/bottargpos
## A small icon that shows whenever a cutscene currently has control over the player
@onready var notif:TextureRect = $CanvasLayer/TextureRect
## The text component of the dialogue box
## @deprecated
@onready var textbox:Control = $CanvasLayer/Control/text
## The sprite component of the dialogue box
@onready var textbg:Sprite2D = $CanvasLayer/Control/text/PanelContainer/Sprite2D
## The animation component that makes the dialogue box appear and disappear
@onready var effects:AnimationPlayer = $effects
## Plays a sound whenever a new character is added to the dialogue box
@onready var sound:AudioStreamPlayer = $AudioStreamPlayer
## The text component of the dialogue box
@onready var texlabel:DialogueLabel  = $CanvasLayer/Control/text/PanelContainer/HBoxContainer/VBoxContainer/DialogueLabel


## Adds the given flag to this instance's flag array
static func store_flag(flag:StringName) -> void:
	assert(flag.is_valid_ascii_identifier(), "Invalid flag name!")
	if is_instance_valid(instance):
		instance.set_meta(flag, true)


## Removes the given flag from this instance's flag array
static func clear_flag(flag:StringName) -> void:
	if is_instance_valid(instance):
		instance.set_meta(flag, null)


## Checks if the given flag is currently within this instance's flag array
static func has_flag(flag:StringName) -> bool:
	if is_instance_valid(instance):
		return instance.has_meta(flag)
	return false


## Saves the current instance's flag array
static func save_flags() -> Array[StringName]:
	if is_instance_valid(instance):
		return instance.get_meta_list()
	return []


## Loads the current instance's flag array
static func load_flags(flags:Array[StringName]) -> void:
	if is_instance_valid(instance):
		var old_flags := instance.get_meta_list()
		for i in old_flags:
			instance.set_meta(i, null)
	for i in flags:
		store_flag(i)


func _process(delta: float) -> void:
	if is_instance_valid(Player.instance):
		if Player.instance.is_in_top_half_of_screen():
			textbox.position = textbox.position.lerp(bottargpos.position, delta * 10.0)
		else:
			textbox.position = textbox.position.lerp(toptargpos.position, delta * 10.0)
	textbox.position = textbox.position.lerp(toptargpos.position, delta * 10.0)
	if running:
		_sound_elapsed += delta
	
	if _remote_skip_flag:
		if running:
			if texlabel.is_typing:
				texlabel.skip_typing()
			StaticProcess.cut_advance.emit()
		_remote_skip_flag = false


func _ready() -> void:
	instance = self


## Standalone process subroutine for the dialogue box. Called once to open it, after which
## it remains open until it exhausts all dialogue in the current cutscene script
static func _process_dia() -> void:
	if not is_instance_valid(instance):
		return
	reset_textbox_color()
	set_sound()
	running = true
	current_scene.reset_state()
	var line:DialogueLine = await current_scene.get_next_dialogue_line()
	while is_instance_valid(line) and running:
		print(line.text)
		print(line.next_id)
		instance.texlabel.dialogue_line = line
		instance.texlabel.type_out()
		print("wait for typing")
		await instance.texlabel.finished_typing
		print("wait for next")
		if SInput.cutscene_has_control:
			instance.advancearrow.show()
			await StaticProcess.cut_advance
			instance.advancearrow.hide()
		else:
			await StaticProcess.get_tree().create_timer(_next_line_timeout).timeout
			_next_line_timeout = DEFAULT_NEXT_TIMEOUT
		print("wait for line")
		line = await current_scene.get_next_dialogue_line(line.next_id)
	print("done")
	running = false
	await hide_textbox()
	unlock_input()


## Shows the dialogue box
static func show_textbox() -> void:
	if is_instance_valid(instance):
		if not _boxtrack:
			_boxtrack = true
			instance.effects.play(&"appear")
			await instance.effects.animation_finished


## Hides the dialogue box
static func hide_textbox() -> void:
	if is_instance_valid(instance):
		if _boxtrack:
			_boxtrack = false
			instance.effects.play_backwards(&"appear")
			await instance.effects.animation_finished


## Revokes control from the player by marking the input flag for an active cutscene as true
static func lock_input() -> void:
	SInput.cutscene_has_control = true
	if is_instance_valid(instance):
		instance.notif.visible = true


## Returns control to the player by marking the input flag for an active cutscene as false
static func unlock_input() -> void:
	SInput.cutscene_has_control = false
	if is_instance_valid(instance):
		instance.notif.visible = false


## Marks the given actor as the currently active object, making it able to receive action calls
static func set_actor(actor:String) -> void:
	active_object_id = actor


## Passes a glide call to the currently active actor, which it will handle in its own script
static func glide_to(_position:Vector2, _duration:float) -> void:
	var actor := get_actor(active_object_id)
	if not is_instance_valid(actor):
		return
	actor.glide_to(_position, _duration)


## Passes an action call to the currently active actor, which it will handle in its own script
static func perform_action(action:String, force:bool) -> void:
	var actor := get_actor(active_object_id)
	if not is_instance_valid(actor):
		return
	actor.perform_action(action, force)


## Finds and returns an actor based on its [String] identifier
static func get_actor(id:String) -> CutsceneControllable:
	for i in CutsceneControllable.actors:
		if i.identifier == id:
			return i
	return null


## Returns [code]true[/code] if the given actor is to the left of the given X position
static func actor_left_of(id:String, x:float) -> bool:
	var actor:CutsceneControllable = get_actor(id)
	if not is_instance_valid(actor):
		return false
	return actor.position.x < x


## Returns [code]true[/code] if the given actor is to the right of the given X position
static func actor_right_of(id:String, x:float) -> bool:
	var actor:CutsceneControllable = get_actor(id)
	if not is_instance_valid(actor):
		return false
	return actor.position.x > x


## Returns [code]true[/code] if the given actor is above the given Y position
static func actor_above(id:String, y:float) -> bool:
	var actor:CutsceneControllable = get_actor(id)
	if not is_instance_valid(actor):
		return false
	return actor.position.y < y


## Returns [code]true[/code] if the given actor is below the given Y position
static func actor_below(id:String, y:float) -> bool:
	var actor:CutsceneControllable = get_actor(id)
	if not is_instance_valid(actor):
		return false
	return actor.position.y > y


## Begins a cutscene in accordance with the given dialogue resource
static func start(dia:DialogueResource, anim:AnimationPlayer, initiator:String) -> void:
	if running:
		return
	if not is_instance_valid(dia):
		return
	current_scene = dia
	current_animator = anim
	object_id = initiator
	_sound_elapsed = MIN_SOUND_ELAPSED
	_process_dia()


## Plays an animation in this instance's attached [AnimationPlayer]
static func trigger_animation(anim:String, wait:bool) -> void:
	if is_instance_valid(instance):
		if instance.current_animator.has_animation(anim):
			instance.current_animator.play(anim)
			if wait:
				await instance.current_animator.animation_finished


## Plays an animation in this instance's attached [AnimationPlayer] in reverse
static func trigger_animation_backwards(anim:String, wait:bool) -> void:
	if is_instance_valid(instance):
		if instance.current_animator.has_animation(anim):
			instance.current_animator.play_backwards(anim)
			if wait:
				await instance.current_animator.animation_finished


## Sets the color of the dialogue box
static func set_textbox_color(_col:String) -> void:
	if is_instance_valid(instance):
		instance.textbg.self_modulate = Color(_col)


## Returns the dialogue box to a default color
static func reset_textbox_color() -> void:
	if is_instance_valid(instance):
		instance.textbg.self_modulate = Color("#004457")


## Sets the frame shape of the dialogue box
static func set_textbox_frame(_frame:int) -> void:
	if is_instance_valid(instance):
		instance.textbg.id = _frame


## Sets the dialogue box's color and shape to one of six presets based on the player character requested
static func set_textbox_player_preset(_player:Player.Players) -> void:
	match _player:
		Player.Players.SNAILY:
			set_textbox_color("#004457")
			set_textbox_frame(0)
		Player.Players.SLUGGY:
			set_textbox_color("#005919")
			set_textbox_frame(0)
		Player.Players.UPSIDE:
			set_textbox_color("#ae7c2a")
			set_textbox_frame(0)
		Player.Players.LEGGY:
			set_textbox_color("#008991")
			set_textbox_frame(2)
		Player.Players.BLOBBY:
			set_textbox_color("#960087")
			set_textbox_frame(1)
		Player.Players.LEECHY:
			set_textbox_color("#606060")
			set_textbox_frame(0)


## Focuses the camera on a specific point in world space, which it will focus on until given a new target
static func set_camera_focus_pos(_position:Vector2) -> void:
	UICore.instance.cam.set_cam_mode(CamControl.CamStates.TARGET_POINT, _position)


## Focuses the camera on a specific node, which it will track until given a new target
static func set_camera_focus_node(_node:Node2D) -> void:
	UICore.instance.cam.set_cam_mode(CamControl.CamStates.TARGET_ENTITY, _node)


## Focuses the camera on the player, returning it to default follow behavior
static func set_camera_focus_player() -> void:
	UICore.instance.cam.set_cam_mode()


## Retrieves a default dialogue sound to use based on a numerical NPC identifier
static func _idmod(id:int) -> int:
	if id == 39:
		return 4
	return id % 4


## Sets the sound played when drawing dialogue to a given [AudioStream] based on its filename.
## Can also set a default sound based on a numerical identifier 
static func set_sound(id := "-1") -> void:
	if is_instance_valid(instance):
		const pth := "res://Assets/Sounds/Sfx"
		const typ := [".ogg", ".wav", ".mp3"]
		if id == "":
			instance.sound.stream = preload("uid://b3ixpp7kjia5k")
		if id == "-1":
			id = object_id
		if id.is_valid_int():
			var idi := id.to_int()
			instance.sound.stream = [
				preload("uid://b3ixpp7kjia5k"),
				preload("uid://dsn0n1srqm0ow"),
				preload("uid://b60650li5d52q"),
				preload("uid://dqs24itjujvno"),
				preload("uid://d5d21n1acuqg"),
			][_idmod(idi)]
		else:
			id = id.remove_chars("/.\\,<>|\'[]{}-=_+()*&^%$#@!~`?;:")
			for i in typ:
				var ipth = pth + id + i
				if ResourceLoader.exists(ipth):
					instance.sound.stream = load(ipth)
					return
			instance.sound.stream = preload("uid://b3ixpp7kjia5k")


## Sets a custom duration for any non-controlling dialogue to wait before advancing
static func set_next_timeout(time:float) -> void:
	_next_line_timeout = time


## Called whenever a dialogue sound is played
func _on_dialogue_label_spoke(_letter: String, _letter_index: int, _speed: float) -> void:
	if _sound_elapsed < MIN_SOUND_ELAPSED or _letter == "\n" or _letter == " ":
		return
	sound.play()
	_sound_elapsed = 0.0


func _input(event: InputEvent) -> void:
	if event.is_action_pressed(&"skipTalking"):
		if texlabel.is_typing and SInput.cutscene_has_control:
			texlabel.skip_typing()
		else:
			StaticProcess.cut_advance.emit()


## Called externally to skip a line of dialogue
static func remote_skip_line() -> void:
	_remote_skip_flag = true
