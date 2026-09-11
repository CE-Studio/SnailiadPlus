# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ToggleSnailyButton
extends SnailyButton


#region Variables
@export var button_text:String = ""
@export var toggled_on:bool = false:
	set(value):
		toggled_on = value
		toggle_box.texture.region = Rect2(0, 22 if value else 0, 22, 22)

signal button_toggled(value)

## The [SnailyText] label displayed over the button
@onready var text:SnailyText
## The [TextureRect] that displays whether this button's corresponding setting is set to true or false
@onready var toggle_box:TextureRect
#endregion


func _ready() -> void:
	super._ready()
	text = $"HBoxContainer/PanelContainer/MarginContainer/SnailyText"
	toggle_box = $"HBoxContainer/TextureRect"
	
	text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	text.add_shadow(1)
	if not Engine.is_editor_hint():
		if button_text.strip_edges() == "":
			text.set_snaily_text(tr(&"Text!!"))
		else:
			text.set_snaily_text(button_text.replace("\\n", "\n"))
		text.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
	toggled_on = toggled_on


func _process(_delta: float) -> void:
	if focused and not disabled and life_frames >= REQ_LIFE_FRAMES and parent_layer.menu.read_inputs:
		if (mouse_over and (SInput.input_just_pressed(SInput.Inputs.UI_CLICK))
		or SInput.input_just_pressed(SInput.Inputs.JUMP)):
			toggled_on = !toggled_on
			button_toggled.emit(toggled_on)
			sfx_select.play()
	super._process(_delta)


## Sets the setting text
func set_text(_text:String) -> void:
	text.set_snaily_text(_text)
