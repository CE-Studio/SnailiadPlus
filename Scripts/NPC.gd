@tool
class_name NPC
extends CutsceneControllable


#region Variables
const GRAVITY:float = 1200.0
const TERMINAL_VELOCITY:float = 500.0

@export var my_id:int
@export_enum("Left", "Right", "Face player:-1") var face_mode:int = -1
@export_enum("Floor", "Left wall", "Right wall", "Ceiling") var surface:int = 0
@export_enum("Floor", "Left wall", "Right wall", "Ceiling") var fall_direction:int = 0
@export_enum(
	"Snail", "Slug", "Cone-shell snail", "Spikey shell snail",
	"Spikey cone-shell snail", "Large-shelled snail", "Turtle",
	"Detect from ID:-1"
) var animation_set:int = 0
@export var inventory:Array[int] = []

var facing_left:bool = false
var process_ai:bool = true

var cc_glide_origin:Vector2 = Vector2.ZERO
var cc_glide_target:Vector2 = Vector2.ZERO
var cc_glide_duration:float = 0.0
var cc_glide_active:bool = false
var cc_glide_elapsed:float = 0.0
var cc_lookat_node:Node2D = null
var cc_lookat_pos:Vector2 = Vector2.ZERO

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var body:CharacterBody2D = $"CharacterBody2D"
@onready var sfx_jump:AudioStreamPlayer = $"Jump"
var colorized_sprite:Texture2D
#endregion


func spawn() -> void:
	cc_lookat_pos = position
	if face_mode == 0:
		facing_left = true
		look_left()
	elif face_mode == 1:
		facing_left = false
		look_right()
	elif GameCore.instance:
		cc_lookat_node = GameCore.instance.player
	match surface:
		Statics.DirsSurface.FLOOR:
			body.set_deferred("rotation_degrees", 0.0)
			body.up_direction = Vector2.UP
			if cc_lookat_node:
				facing_left = cc_lookat_node.position.x < position.x
		Statics.DirsSurface.LWALL:
			body.set_deferred("rotation_degrees", 90.0)
			body.up_direction = Vector2.RIGHT
			if cc_lookat_node:
				facing_left = cc_lookat_node.position.y < position.y
		Statics.DirsSurface.RWALL:
			body.set_deferred("rotation_degrees", 270.0)
			body.up_direction = Vector2.LEFT
			if cc_lookat_node:
				facing_left = cc_lookat_node.position.y > position.y
		Statics.DirsSurface.CEILING:
			body.set_deferred("rotation_degrees", 180.0)
			body.up_direction = Vector2.DOWN
			if cc_lookat_node:
				facing_left = cc_lookat_node.position.x > position.x
	body.position = position
	play_anim("idle")
	sprite._process(0.0)
	UICore.instance.darkness_layer.add_source(self, 32)


func _process(_delta: float) -> void:
	if GameCore.instance == null or Engine.is_editor_hint() or not process_ai:
		return

	var lookat_pos = cc_lookat_pos
	if cc_lookat_node:
		lookat_pos = cc_lookat_node.position
	match surface:
		Statics.DirsSurface.FLOOR:
			if facing_left and lookat_pos.x > position.x:
				facing_left = false
				play_anim("turnground")
			elif not facing_left and lookat_pos.x < position.x:
				facing_left = true;
				play_anim("turnground")
		Statics.DirsSurface.LWALL:
			if facing_left and lookat_pos.y > position.y:
				facing_left = false
				play_anim("turnground")
			elif not facing_left and lookat_pos.y < position.y:
				facing_left = true;
				play_anim("turnground")
		Statics.DirsSurface.RWALL:
			if facing_left and lookat_pos.y < position.y:
				facing_left = false
				play_anim("turnground")
			elif not facing_left and lookat_pos.y > position.y:
				facing_left = true;
				play_anim("turnground")
		Statics.DirsSurface.CEILING:
			if facing_left and lookat_pos.x < position.x:
				facing_left = false
				play_anim("turnground")
			elif not facing_left and lookat_pos.x > position.x:
				facing_left = true;
				play_anim("turnground")


func _physics_process(delta: float) -> void:
	if GameCore.instance == null or Engine.is_editor_hint() or not process_ai:
		return
	
	if cc_glide_active:
		cc_glide_elapsed = clampf(cc_glide_elapsed + delta, 0.0, cc_glide_duration)
		var lerp_rate:float = inverse_lerp(0.0, cc_glide_duration, cc_glide_elapsed)
		body.position = cc_glide_origin.lerp(cc_glide_target, lerp_rate)
		position = body.position
		if cc_glide_elapsed >= cc_glide_duration:
			cc_glide_active = false
		return
	
	match surface:
		0:
			body.velocity.y += GRAVITY * delta
			body.velocity.y = clampf(body.velocity.y, -INF, TERMINAL_VELOCITY)
		1:
			body.velocity.x -= GRAVITY * delta
			body.velocity.x = clampf(body.velocity.x, -TERMINAL_VELOCITY, INF)
		2:
			body.velocity.x += GRAVITY * delta
			body.velocity.x = clampf(body.velocity.x, -INF, TERMINAL_VELOCITY)
		3:
			body.velocity.y -= GRAVITY * delta
			body.velocity.y = clampf(body.velocity.y, -TERMINAL_VELOCITY, INF)
	body.move_and_slide()
	position = body.position


func play_anim(state:String) -> void:
	var anim:String = "npc%d." % my_id
	if animation_set != -1:
		anim = str(animation_set) + "."
	match surface:
		Statics.DirsSurface.FLOOR: anim += "floor."
		Statics.DirsSurface.LWALL: anim += "lwall."
		Statics.DirsSurface.RWALL: anim += "rwall."
		Statics.DirsSurface.CEILING: anim += "ceiling."
	anim += "left." if facing_left else "right."
	anim += state
	sprite.action = anim


func look_left() -> void:
	cc_lookat_node = null
	match surface:
		Statics.DirsSurface.FLOOR: cc_lookat_pos = position + Vector2.LEFT
		Statics.DirsSurface.LWALL: cc_lookat_pos = position + Vector2.UP
		Statics.DirsSurface.RWALL: cc_lookat_pos = position + Vector2.DOWN
		Statics.DirsSurface.CEILING: cc_lookat_pos = position + Vector2.RIGHT


func look_right() -> void:
	cc_lookat_node = null
	match surface:
		Statics.DirsSurface.FLOOR: cc_lookat_pos = position + Vector2.RIGHT
		Statics.DirsSurface.LWALL: cc_lookat_pos = position + Vector2.DOWN
		Statics.DirsSurface.RWALL: cc_lookat_pos = position + Vector2.UP
		Statics.DirsSurface.CEILING: cc_lookat_pos = position + Vector2.LEFT


func set_gravity(new_dir:Statics.DirsSurface) -> void:
	surface = new_dir
	match new_dir:
		Statics.DirsSurface.FLOOR:
			body.set_deferred("rotation_degrees", 0.0)
			body.up_direction = Vector2.UP
		Statics.DirsSurface.LWALL:
			body.set_deferred("rotation_degrees", 90.0)
			body.up_direction = Vector2.RIGHT
		Statics.DirsSurface.RWALL:
			body.set_deferred("rotation_degrees", 270.0)
			body.up_direction = Vector2.LEFT
		Statics.DirsSurface.CEILING:
			body.set_deferred("rotation_degrees", 180.0)
			body.up_direction = Vector2.DOWN


#region Cutscene functions
func impulse(_direction:Vector2) -> bool:
	body.velocity += _direction
	return true


func glide_to(_position:Vector2, _duration:float) -> bool:
	cc_glide_origin = body.position
	cc_glide_target = _position
	cc_glide_duration = _duration
	cc_glide_active = true
	cc_glide_elapsed = 0.0
	return true


func get_dialogue_icon() -> Texture:
	return null


func fake_input(_event:InputEventAction, _hold_for:float) -> bool:
	return false


func look_at_position(_pos:Vector2) -> bool:
	cc_lookat_node = null
	cc_lookat_pos = _pos
	return true


func look_at_local(_pos:Vector2) -> bool:
	cc_lookat_node = null
	cc_lookat_pos = position + _pos
	return true


func look_at_node(_node:Node2D) -> bool:
	cc_lookat_node = _node
	return true


func disable_ai(_locked:bool) -> bool:
	process_ai = _locked
	return true


func has_item(_ID:Item.ItemTypes) -> bool:
	if _ID >= inventory.size():
		return false
	var has:bool = inventory[_ID as int] > 0
	return has


func give_item(_id:Item.ItemTypes, quantity:int) -> bool:
	while _id > inventory.size() - 1:
		inventory.append(0)
	inventory[_id as int] += quantity
	return true


func take_item(_id:Item.ItemTypes, _quantity:int) -> bool:
	if _id >= inventory.size():
		return false
	if inventory[_id] > _quantity:
		inventory[_id] = 0
		return false
	inventory[_id] -= _quantity
	return true


func can_perform_action(_action:String) -> bool:
	match _action:
		"jump":
			return true
	return false


func perform_action(_action:String, _force:bool) -> bool:
	if not (can_perform_action(_action) or _force):
		return false
	match _action:
		"jump":
			if surface == fall_direction:
				match surface:
					Statics.DirsSurface.FLOOR: body.velocity.y = -500
					Statics.DirsSurface.LWALL: body.velocity.x = -500
					Statics.DirsSurface.RWALL: body.velocity.x = 500
					Statics.DirsSurface.CEILING: body.velocity.y = 500
			else:
				set_gravity(fall_direction)
				play_anim("idle")
			sfx_jump.play()
			return true
	return false
#endregion


func _on_character_body_2d_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if Engine.is_editor_hint():
		return
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				Room.instance.start_cutscene(self)
