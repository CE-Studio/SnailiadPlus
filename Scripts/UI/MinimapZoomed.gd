# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name MinimapZoomed
extends Node2D


const MARKER_PATH:String = "res://Assets/Images/UI/MinimapIconsZoomed.json"
const BOUNDS:Vector2 = Vector2(96, 112)
const SPEED:float = 128.0

@export var move_group:Node2D
@export var cell_mask:Sprite2D
@export var map:JsonSprite2D
@export var p_marker:JsonSprite2D
@export var marker_group:Node2D
@export var ctrl_text:SnailyText
@export var arrow_u:JsonSprite2D
@export var arrow_d:JsonSprite2D
@export var arrow_l:JsonSprite2D
@export var arrow_r:JsonSprite2D


func _ready() -> void:
	modulate.a = 0.0
	ctrl_text.set_snaily_text(" bind__UI_ACCEPT - Hide markers\n bind__UI_BACK - Return")


func init(minimap:Minimap) -> void:
	cell_mask.scale = Vector2(16, 16)
	map.scale = Vector2(0.0625, 0.0625)
	
	cell_mask.texture = minimap.cell_mask.texture
	p_marker.position = minimap.player_marker.position * 2.0
	p_marker.action = minimap.player_marker.action
	
	for this_marker in minimap.active_markers:
		var draw_marker:bool = true
		if this_marker.type == minimap.MarkerTypes.ITEM:
			draw_marker = not minimap.empty_locations.has(this_marker.data[0])
		if draw_marker and this_marker.modulate.a == 1.0:
			var new_sprite:JsonSprite2D = JsonSprite2D.new()
			new_sprite.texture_path = MARKER_PATH
			marker_group.add_child(new_sprite)
			new_sprite.action = this_marker.sprite.action
			new_sprite.position = this_marker.position * 2.0


func _process(delta: float) -> void:
	modulate.a = lerpf(modulate.a, 1.0, 32.0 * delta)
	
	move_group.position -= SInput.vector_move(true) * SPEED * delta
	move_group.position.x = clampf(move_group.position.x, -BOUNDS.x, BOUNDS.x)
	move_group.position.y = clampf(move_group.position.y, -BOUNDS.y, BOUNDS.y)
	
	var hide_markers:bool = SInput.check_input(SInput.Inputs.UI_ACCEPT, false)
	marker_group.modulate.a = 0.0 if hide_markers else 1.0
	p_marker.modulate.a = 0.2 if hide_markers else 1.0
	
	arrow_u.modulate.a = 1.0 if move_group.position.y < BOUNDS.y else 0.0
	arrow_d.modulate.a = 1.0 if move_group.position.y > -BOUNDS.y else 0.0
	arrow_l.modulate.a = 1.0 if move_group.position.x < BOUNDS.x else 0.0
	arrow_r.modulate.a = 1.0 if move_group.position.x > -BOUNDS.x else 0.0
