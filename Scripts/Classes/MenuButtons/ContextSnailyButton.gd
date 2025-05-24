@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ContextSnailyButton
extends SnailyButton


#region Variables
@export var text_id:String = ""
@export var subtext_id:String = ""
@export var quick_load_layer:String = ""
@export var back_one_layer:bool = false

signal button_pressed(value)

@onready var text:SnailyText
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
		if text_id.strip_edges() == "":
			text.set_snaily_text("Text!!")
		else:
			text.set_snaily_text(Statics.get_text(text_id))
		if subtext_id.strip_edges() == "":
			subtext.set_snaily_text("Text!!")
		else:
			subtext.set_snaily_text(Statics.get_text(subtext_id))
		text.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		subtext.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(_delta: float) -> void:
	if focused and not disabled:
		if (mouse_over and (Input.is_action_just_pressed("UIClick"))
		or Input.is_action_just_pressed("Jump")):
			if quick_load_layer.strip_edges() != "":
				button_pressed.emit(quick_load_layer)
			else:
				button_pressed.emit(0)


func set_text(_text:String) -> void:
	text.set_snaily_text(_text)


func set_subtext(_text:String) -> void:
	subtext.set_snaily_text(_text)
