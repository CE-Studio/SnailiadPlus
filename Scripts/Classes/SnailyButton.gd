@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name SnailyButton
extends PanelContainer

#region Variables
const FOCUS_HOVER_OFFSET:Vector2 = Vector2(0, -3)
const MOVE_RATE:float = 12.0

@export var text_id:String = ""

var focused:bool = false
var origin:Vector2
var focus_hover_rate:Vector2
var focus_hover_range:Vector2
var focus_hover_timers:Vector2

@onready var text:SnailyText = $MarginContainer/SnailyText
@onready var sfx_focus:AudioStreamPlayer = $"AudioGroup/Focus"
@onready var sfx_select:AudioStreamPlayer = $"AudioGroup/Select"
#endregion


func _ready() -> void:
	origin = position
	focus_hover_rate = Vector2(randf_range(0.25, 1.0), randf_range(0.25, 1.0))
	focus_hover_range = Vector2(randi_range(1, 5), randi_range(1, 3))
	if text_id == "":
		text.set_snaily_text("Text!!")
	else:
		text.set_snaily_text(Statics.get_text(text_id))
	text.add_shadow(1)


func _process(delta: float) -> void:
	focus_hover_timers.x += delta * focus_hover_rate.x
	focus_hover_timers.y += delta * focus_hover_rate.y
	if focused:
		var focus_hover_pos = Vector2(sin(focus_hover_timers.x) * focus_hover_range.x,
		cos(focus_hover_timers.y) * focus_hover_range.y)
		position = position.lerp(origin + focus_hover_pos + FOCUS_HOVER_OFFSET, MOVE_RATE)


func on_focus() -> void:
	focused = true
	sfx_focus.play()
