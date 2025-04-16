@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name SnailyButton
extends Node2D

#region Variables
@export var text_id:String = ""

@onready var text:SnailyText = $"SnailyText"
@onready var sfx_focus:AudioStreamPlayer = $"AudioGroup/Focus"
@onready var sfx_select:AudioStreamPlayer = $"AudioGroup/Select"
#endregion


func _ready() -> void:
	if text_id == "":
		text.set_text("Text!!")
	else:
		text.set_text(Statics.get_text(text_id))
	text.add_shadow(1)
