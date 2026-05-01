# Copyright 2026 CE-Studio: AGPL-3.0-only
@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ContextSnailyButton
extends SnailyButton


#region Variables
@export var main_text:String = ""
@export var sub_text:String = ""
@export var quick_load_layer:String = ""
@export var back_one_layer:bool = false

signal button_pressed(value)

## Main [SnailyText] component
@onready var text:SnailyText
## Smaller context [SnailyText] component
@onready var subtext:SnailyText
#endregion


func _ready() -> void:
	super._ready()
	text = $"MarginContainer/VBoxContainer/BigContainer/BigSnailyText"
	subtext = $"MarginContainer/VBoxContainer/SmallContainer/SmallSnailyText"
	
	text.set_alignment(HORIZONTAL_ALIGNMENT_LEFT, VERTICAL_ALIGNMENT_TOP)
	text.add_shadow(1)
	subtext.set_alignment(HORIZONTAL_ALIGNMENT_LEFT, VERTICAL_ALIGNMENT_TOP)
	subtext.add_shadow(1)
	if not Engine.is_editor_hint():
		if main_text.strip_edges() == "":
			text.set_snaily_text(tr(&"Text!!"))
		else:
			text.set_snaily_text(main_text.replace("\\n", "\n"))
		if sub_text.strip_edges() == "":
			subtext.set_snaily_text(tr(&"Text!!"))
		else:
			subtext.set_snaily_text(sub_text.replace("\\n", "\n"))
		text.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		subtext.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(_delta: float) -> void:
	if focused and not disabled and life_frames >= REQ_LIFE_FRAMES:
		if (mouse_over and (SInput.input_just_pressed(SInput.Inputs.UI_CLICK))
		or SInput.input_just_pressed(SInput.Inputs.UI_ACCEPT)):
			if quick_load_layer.strip_edges() != "":
				button_pressed.emit(quick_load_layer)
			else:
				button_pressed.emit(0)
	super._process(_delta)


## Sets the main text
func set_text(_text:String) -> void:
	text.set_snaily_text(_text)


## Sets the context text
func set_subtext(_text:String) -> void:
	subtext.set_snaily_text(_text)
