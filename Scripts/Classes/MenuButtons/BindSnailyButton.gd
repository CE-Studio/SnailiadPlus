@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name BindSnailyButton
extends SnailyButton


#region Variabes
@export var bind:StringName = &""

signal bind_changed

@onready var text:SnailyText = $"PanelContainer/HBoxContainer/PanelContainer/MarginContainer/SnailyText"
@onready var icon:TextureRect = $"PanelContainer/HBoxContainer/IconFrame/Icon"
@onready var icon_shadow:TextureRect = $"PanelContainer/HBoxContainer/IconFrame/IconShadow"
#endregion


func _ready() -> void:
	super()
	
	text.set_snaily_text(bind)
