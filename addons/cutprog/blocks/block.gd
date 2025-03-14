@tool
class_name ProgramBlock
extends HBoxContainer


var label:Label
var else_label:Label
var outsocket:Outsocket
var sockets:Array[Insocket]
var socket_box:VBoxContainer


func setup(socket_types:Array[int], return_type:int, has_if:bool, has_else:bool):
	label = $VBoxContainer/Container/PanelContainer/Label
	else_label = $VBoxContainer/VBoxContainer/if/HBoxContainer/Panel/Label
	outsocket = $TextureRect
	socket_box = $VBoxContainer/Container/sockets
	outsocket.type = return_type
	for i in socket_types:
		var ns := Insocket.new()
		ns.type = i
		sockets.append(ns)
		socket_box.add_child(ns)
	if has_if:
		$VBoxContainer/VBoxContainer/if.show()
	if has_else:
		$VBoxContainer/VBoxContainer/else.show()
		$VBoxContainer/VBoxContainer/if/HBoxContainer/Panel/Label.show()
