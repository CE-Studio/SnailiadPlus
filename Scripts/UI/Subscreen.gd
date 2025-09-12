class_name Subscreen
extends Node2D


#region Variables
@onready var body:JsonSprite2D = $Body
@onready var player_icon:JsonSprite2D = $"PlayerIcon"

@export var separators:Array[JsonSprite2D] = []
#endregion


func _ready() -> void:
	var this_char = int(Statics.current_profile["character"])
	body.action = str(this_char)
	player_icon.action = str(this_char)
	var name_text:SnailyText = $"Name"
	name_text.set_snaily_text("char_full_%d" % this_char)
	player_icon.position.x = name_text.position.x + name_text.get_width()
	for sep in separators:
		sep.action = "anim"
