@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ToggleSnailyButton
extends SnailyButton


#region Variables
@export var text_id:String = ""
@export var toggled_on:bool = false:
	set(value):
		toggled_on = value
		toggle_box.texture.region = Rect2(0, 22 if value else 0, 22, 22)

signal button_toggled(value)

@onready var text:SnailyText
@onready var toggle_box:TextureRect
#endregion


func _ready() -> void:
	super._ready()
	text = $"HBoxContainer/PanelContainer/MarginContainer/SnailyText"
	toggle_box = $"HBoxContainer/TextureRect"
	
	text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	text.add_shadow(1)
	if not Engine.is_editor_hint():
		if text_id.strip_edges() == "":
			text.set_snaily_text("Text!!")
		else:
			text.set_snaily_text(Statics.get_text(text_id))
		text.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
	toggled_on = toggled_on


func _process(_delta: float) -> void:
	if focused and not disabled:
		if (mouse_over and (Input.is_action_just_pressed("UIClick"))
		or Input.is_action_just_pressed("Jump")):
			toggled_on = !toggled_on
			button_toggled.emit(toggled_on)
			sfx_select.play()


func set_text(_text:String) -> void:
	text.set_snaily_text(_text)
