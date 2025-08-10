@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ActionSnailyButton
extends SnailyButton


#region Variables
@export var text_id:String = ""
@export var quick_load_layer:String = ""
@export var back_one_layer:bool = false

signal button_pressed(value)

@onready var text:SnailyText
#endregion


func _ready() -> void:
	super._ready()
	text = $"MarginContainer/SnailyText"
	
	text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	text.add_shadow(1)
	if not Engine.is_editor_hint():
		if text_id.strip_edges() == "":
			text.set_snaily_text("Text!!")
		else:
			text.set_snaily_text(Statics.get_text(text_id))
		text.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(_delta: float) -> void:
	if focused and not disabled and life_frames >= REQ_LIFE_FRAMES:
		if (mouse_over and (SInput.input_just_pressed(SInput.Inputs.UI_CLICK))
		or SInput.input_just_pressed(SInput.Inputs.UI_ACCEPT)):
			if quick_load_layer.strip_edges() != "":
				button_pressed.emit(quick_load_layer)
			else:
				button_pressed.emit(0)
	super._process(_delta)


func set_text(_text:String) -> void:
	text.set_snaily_text(_text)
