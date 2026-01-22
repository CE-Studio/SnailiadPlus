class_name MinimapZoomed
extends Node2D


const MARKER_PATH:String = "res://Assets/Images/UI/MinimapIconsZoomed.json"
const BOUNDS:Vector2 = Vector2(96, 112)
const SPEED:float = 128.0

@onready var move_group:Node2D = $"MoveGroup"
@onready var cell_mask:Sprite2D = $"MoveGroup/CellMask"
@onready var map:JsonSprite2D = $"MoveGroup/CellMask/Map"
@onready var p_marker:JsonSprite2D = $"MoveGroup/PlayerMarker"
@onready var marker_group:Node2D = $"MoveGroup/MarkerGroup"


func _ready() -> void:
	modulate.a = 0.0


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
