@tool
class_name NPC
extends CutsceneControllable


#region Variables
const GRAVITY:float = 1200.0
const TERMINAL_VELOCITY:float = 500.0

@export var my_id:int
@export_enum("Left", "Right", "Face player:-1") var face_mode:int = -1
@export_enum("Floor", "Left wall", "Right wall", "Ceiling") var surface:int = 0
@export_enum(
	"Snail", "Slug", "Cone-shell snail", "Spikey shell snail",
	"Spikey cone-shell snail", "Large-shelled snail", "Turtle",
	"Detect from ID:-1"
) var animation_set:int = 0

var facing_left:bool = false

@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var body:CharacterBody2D = $"CharacterBody2D"
var colorized_sprite:Texture2D
#endregion


func spawn() -> void:
	match surface:
		Statics.DirsSurface.FLOOR:
			body.set_deferred("rotation_degrees", 0.0)
			body.up_direction = Vector2.UP
			facing_left = GameCore.instance.player.position.x < position.x
		Statics.DirsSurface.LWALL:
			body.set_deferred("rotation_degrees", 90.0)
			body.up_direction = Vector2.RIGHT
			facing_left = GameCore.instance.player.position.y < position.y
		Statics.DirsSurface.RWALL:
			body.set_deferred("rotation_degrees", 270.0)
			body.up_direction = Vector2.LEFT
			facing_left = GameCore.instance.player.position.y > position.y
		Statics.DirsSurface.CEILING:
			body.set_deferred("rotation_degrees", 180.0)
			body.up_direction = Vector2.DOWN
			facing_left = GameCore.instance.player.position.x > position.x
	body.position = position
	if face_mode == 0:
		facing_left = true
	elif face_mode == 1:
		facing_left = false
	play_anim("idle")
	sprite._process(0.0)
	UICore.instance.darkness_layer.add_source(self, 32)


func _process(_delta: float) -> void:
	if GameCore.instance == null or Engine.is_editor_hint():
		return

	if face_mode == -1:
		var player_pos = GameCore.instance.player.position
		match surface:
			Statics.DirsSurface.FLOOR:
				if facing_left and player_pos.x > position.x:
					facing_left = false
					play_anim("turnground")
				elif not facing_left and player_pos.x < position.x:
					facing_left = true;
					play_anim("turnground")
			Statics.DirsSurface.LWALL:
				if facing_left and player_pos.y > position.y:
					facing_left = false
					play_anim("turnground")
				elif not facing_left and player_pos.y < position.y:
					facing_left = true;
					play_anim("turnground")
			Statics.DirsSurface.RWALL:
				if facing_left and player_pos.y < position.y:
					facing_left = false
					play_anim("turnground")
				elif not facing_left and player_pos.y > position.y:
					facing_left = true;
					play_anim("turnground")
			Statics.DirsSurface.CEILING:
				if facing_left and player_pos.x < position.x:
					facing_left = false
					play_anim("turnground")
				elif not facing_left and player_pos.x > position.x:
					facing_left = true;
					play_anim("turnground")


func _physics_process(delta: float) -> void:
	if GameCore.instance == null or Engine.is_editor_hint():
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


#region Cutscene functions
func impulse(_direction:Vector2) -> bool:
	return false


func glide_to(_position:Vector2, _duration:float) -> bool:
	return false


func get_dialogue_icon() -> Texture:
	return null


func fake_input(_event:InputEventAction, _hold_for:float) -> bool:
	return false


func look_at_position(_pos:Vector2) -> bool:
	return false


func look_at_local(_pos:Vector2) -> bool:
	return false


func look_at_node(_node:Node2D) -> bool:
	return false


func lock_inputs(_locked:bool) -> bool:
	return false


func has_item(_ID:Item.ItemTypes) -> bool:
	return false


func can_perform_action(_action:String) -> bool:
	return false


func perform_action(_action:String, _force:bool) -> bool:
	return false
#endregion


func _on_character_body_2d_input_event(viewport: Node, event: InputEvent, shape_idx: int) -> void:
	if Engine.is_editor_hint():
		return
	if event is InputEventMouseButton:
		if event.button_index == MOUSE_BUTTON_LEFT:
			if event.pressed:
				Room.instance.start_cutscene(self)
