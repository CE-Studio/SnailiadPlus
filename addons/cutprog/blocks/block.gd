@tool
class_name ProgramBlock
extends HBoxContainer


var label:Label
var else_label:Label
var outsocket:Outsocket
var sockets:Array[Insocket]
var socket_box:VBoxContainer
var str:LineEdit
var flt:SpinBox
var opt:OptionButton
var av_script:Callable
var has_script := false


func setup(nm:String, return_type:int, socket_types:Array, has_if:bool, has_else:bool, adv_script) -> void:
	label = $VBoxContainer/Container/PanelContainer/HBoxContainer/Label
	label.text = nm
	else_label = $VBoxContainer/VBoxContainer/if/HBoxContainer/Panel/Label
	outsocket = $TextureRect
	socket_box = $VBoxContainer/Container/sockets
	outsocket.type = return_type
	str = $VBoxContainer/Container/PanelContainer/HBoxContainer/str
	flt = $VBoxContainer/Container/PanelContainer/HBoxContainer/float
	opt = $VBoxContainer/Container/PanelContainer/HBoxContainer/opt
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
	scr_setup([])


func reset() -> void:
	if has_script:
		av_script.call(self, [])


func scr_setup(arr:Array[CutsceneControllable]) -> void:
	if has_script:
		av_script.call(self, arr)


func simpsetup(inp:Array) -> void:
	setup(inp[0], inp[1], inp[2], inp[3], inp[4], inp[5])
