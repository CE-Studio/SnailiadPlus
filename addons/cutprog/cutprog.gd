@tool
extends EditorPlugin


var dock:Control


func _enter_tree() -> void:
	dock = preload("res://addons/cutprog/programmer.tscn").instantiate()
	dock.ep= self
	add_control_to_bottom_panel(dock, "Cutscene Programmer")


func _exit_tree() -> void:
	remove_control_from_bottom_panel(dock)
	dock.free()
