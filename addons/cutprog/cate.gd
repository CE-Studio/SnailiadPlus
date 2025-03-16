@tool
class_name Cate
extends VBoxContainer


func _on_button_toggled(toggled_on: bool) -> void:
	$VBoxContainer.visible = toggled_on


func setname(str:String, col:Color) -> void:
	$Button.text = str
	$Button.self_modulate = col


func add_item(node:Control) -> void:
	$VBoxContainer.add_child(node)
