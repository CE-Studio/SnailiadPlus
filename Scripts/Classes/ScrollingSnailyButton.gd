@tool
@icon("res://Editor/ico/SnailyButton.svg")
class_name ScrollingSnailyButton
extends Control

#region Variables
const COLOR_ENABLED = Color8(252, 252, 252)
const COLOR_DISABLED = Color8(200, 192, 192)

@export var header_id:String = ""
@export var cycle_options:Array[String] = []
@export var focus_option:int = 0
@export var loop:bool = true
@export var grab_focus_on_load:bool = false
@export var disabled = false
@export var minimum_x:int:
	set(value):
		minimum_x = value
		custom_minimum_size.x = minimum_x

var focused:bool = false
var mouse_over:bool = false
var origin:Vector2
var can_focus:bool = true:
	set(value):
		can_focus = value
		if value and grab_focus_on_load:
			has_played_focus_sound = false
		focus_mode = Control.FOCUS_ALL if value else Control.FOCUS_NONE
var has_played_focus_sound:bool = false
var selected_option:int

var selected:bool = false
var left_neighbor:NodePath
var right_neighbor:NodePath

signal option_cycled(value)

@onready var header:SnailyText
@onready var option:SnailyText
@onready var tex_left:TextureRect
@onready var tex_right:TextureRect
@onready var sfx_focus:AudioStreamPlayer
@onready var sfx_select:AudioStreamPlayer
#endregion

func _ready() -> void:
	header = $"Main/TopText/SnailyText"
	option = $"Main/Scroller/Frame/HBoxContainer/MarginContainer/SnailyText"
	tex_left = $"Main/Scroller/Left"
	tex_right = $"Main/Scroller/Right"
	sfx_focus = $"AudioGroup/Focus"
	sfx_select = $"AudioGroup/Select"
	
	header.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	header.add_shadow(1)
	option.add_shadow(1)
	if not Engine.is_editor_hint():
		origin = position
		if header_id.strip_edges() == "":
			header.set_snaily_text("Text!!")
		else:
			header.set_snaily_text(Statics.get_text(header_id))
		if cycle_options.size() == 0:
			option.set_snaily_text(Statics.get_text("menu_option_scroller_none"))
			selected_option = -1
			disabled = true
		else:
			while focus_option < 0:
				focus_option += cycle_options.size()
			selected_option = focus_option % cycle_options.size()
			option.set_snaily_text(Statics.get_text(cycle_options[selected_option]))
		if can_focus and grab_focus_on_load and not disabled:
			grab_focus()
		header.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		option.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_left.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED
		tex_right.modulate = COLOR_DISABLED if disabled else COLOR_ENABLED


func _process(delta: float) -> void:
	pass



func set_header(_text:String) -> void:
	header.set_snaily_text(_text)


func free_safely() -> void:
	relinquish_focus_neighbors()
	queue_free()


func relinquish_focus_neighbors() -> void:
	var bottom:Control = get_node(focus_neighbor_bottom)
	var left:Control = get_node(focus_neighbor_left)
	var right:Control = get_node(focus_neighbor_right)
	var top:Control = get_node(focus_neighbor_top)
	var next:Control = get_node(focus_next)
	var previous:Control = get_node(focus_previous)
	bottom.focus_neighbor_top = focus_neighbor_top
	top.focus_neighbor_bottom = focus_neighbor_bottom
	left.focus_neighbor_right = focus_neighbor_right
	right.focus_neighbor_left = focus_neighbor_left
	next.focus_previous = focus_previous
	previous.focus_next = focus_next


func _on_mouse_over() -> void:
	mouse_over = true
	if can_focus and not disabled:
		grab_focus()


func _on_mouse_exit() -> void:
	mouse_over = false


func _on_focus() -> void:
	if not Engine.is_editor_hint():
		if not focused:
			focused = true
			if grab_focus_on_load and not has_played_focus_sound:
				sfx_select.play()
				has_played_focus_sound = true
			else:
				sfx_focus.play()


func _on_exit_focus() -> void:
	if not Engine.is_editor_hint():
		if focused:
			focused = false
