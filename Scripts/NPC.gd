@tool
class_name NPC
extends CutsceneControllable


#region Variables
@export var my_id:int
@export_enum("Left", "Right", "Face player:-1") var face_mode:int = -1
@export_enum("Floor", "Left wall", "Right wall", "Ceiling") var surface:int = 0
@export_enum(
	"Snail", "Slug", "Cone-shell snail", "Spikey shell snail",
	"Spikey cone-shell snail", "Large-shelled snail", "Turtle"
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
			facing_left = GameCore.instance.player.position.x < position.x
		Statics.DirsSurface.LWALL:
			body.set_deferred("rotation_degrees", 90.0)
			facing_left = GameCore.instance.player.position.y < position.y
		Statics.DirsSurface.RWALL:
			body.set_deferred("rotation_degrees", 270.0)
			facing_left = GameCore.instance.player.position.y > position.y
		Statics.DirsSurface.CEILING:
			body.set_deferred("rotation_degrees", 180.0)
			facing_left = GameCore.instance.player.position.x > position.x
	if face_mode == 0:
		facing_left = true
	elif face_mode == 1:
		facing_left = false
	play_anim("idle")
	sprite._process(0.0)


func _process(_delta: float) -> void:
	if GameCore.instance == null:
		return
	if not Engine.is_editor_hint():
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


func play_anim(state:String) -> void:
	var anim = str(animation_set) + "."
	match surface:
		Statics.DirsSurface.FLOOR: anim += "floor."
		Statics.DirsSurface.LWALL: anim += "lwall."
		Statics.DirsSurface.RWALL: anim += "rwall."
		Statics.DirsSurface.CEILING: anim += "ceiling."
	anim += "left." if facing_left else "right."
	anim += state
	sprite.action = anim


#region Cutscene functions
func impulse(direction:Vector2) -> bool:
	return false


func glide_to(position:Vector2, duration:float) -> bool:
	return false


func get_dialogue_icon() -> Texture:
	return null


func fake_input(event:InputEventAction, hold_for:float) -> bool:
	return false


func look_at_position(pos:Vector2) -> bool:
	return false


func look_at_local(pos:Vector2) -> bool:
	return false


func look_at_node(node:Node2D) -> bool:
	return false


func lock_inputs(locked:bool) -> bool:
	return false


func has_item(ID:Item.ItemTypes) -> bool:
	return false


func can_perform_action(action:String) -> bool:
	return false


func perform_action(action:String, force:bool) -> bool:
	return false
#endregion
