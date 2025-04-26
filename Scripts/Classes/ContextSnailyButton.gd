@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ContextSnailyButton
extends SnailyButton

#region Variables
@export var subtext_id:String = ""

@onready var subtext:SnailyText
#endregion

func _ready() -> void:
	text = $"MarginContainer/VBoxContainer/BigSnailyText"
	subtext = $"MarginContainer/VBoxContainer/SmallSnailyText"
	sfx_focus = $"AudioGroup/Focus"
	sfx_select = $"AudioGroup/Select"
	
	text.set_alignment(HORIZONTAL_ALIGNMENT_LEFT, VERTICAL_ALIGNMENT_TOP)
	text.add_shadow(1)
	subtext.set_alignment(HORIZONTAL_ALIGNMENT_LEFT, VERTICAL_ALIGNMENT_TOP)
	subtext.add_shadow(1)
	if not Engine.is_editor_hint():
		origin = position
		if text_id.strip_edges() == "":
			text.set_snaily_text("Text!!")
		else:
			text.set_snaily_text(Statics.get_text(text_id))
		if subtext_id.strip_edges() == "":
			subtext.set_snaily_text("Text!!")
		else:
			subtext.set_snaily_text(Statics.get_text(subtext_id))
		if can_focus and grab_focus_on_load and not disabled:
			grab_focus()
		if disabled:
			text.modulate = Color8(200, 192, 192)
			subtext.modulate = Color8(200, 192, 192)
		hide_frame = hide_frame


func set_subtext(_text:String) -> void:
	subtext.set_snaily_text(_text)
