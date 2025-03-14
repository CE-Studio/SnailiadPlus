@tool
extends HBoxContainer


var label:Label
var outsocket:Outsocket
var sockets:Array[Insocket]


func setup(socket_types:Array[int]):
	label = $VBoxContainer/Container/PanelContainer/Label
