@tool
class_name ProgramBlock
extends HBoxContainer


var label:Label
var else_label:Label
var outsocket:Outsocket
var sockets:Array[Insocket]
var socket_box:VBoxContainer


func setup(nm:String, return_type:int, socket_types:Array, has_if:bool, has_else:bool) -> void:
	label = $VBoxContainer/Container/PanelContainer/HBoxContainer/Label
	label.text = nm
	else_label = $VBoxContainer/VBoxContainer/if/HBoxContainer/Panel/Label
	outsocket = $TextureRect
	socket_box = $VBoxContainer/Container/sockets
	outsocket.type = return_type
	for i in socket_types:
		var ns := Insocket.new()
		ns.type = i
		ns.custom_minimum_size = Vector2(16, 24)
		sockets.append(ns)
		socket_box.add_child(ns)
	if has_if:
		$VBoxContainer/VBoxContainer/if.show()
	if has_else:
		$VBoxContainer/VBoxContainer/else.show()
		$VBoxContainer/VBoxContainer/if/HBoxContainer/Panel/Label.show()


func simpsetup(inp:Array) -> void:
	setup(inp[0], inp[1], inp[2], inp[3], inp[4])
