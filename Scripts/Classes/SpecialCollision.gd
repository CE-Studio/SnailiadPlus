# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
@tool
class_name SpecialCollision
extends StaticBody2D


#region Variables
enum Modes {
	FULL_FORCE,
	FULL_FORCE_DUALEND,
	THREEQUARTER_FORCE,
	THREEQUARTER_FORCE_DUALEND,
	HALF_FORCE,
	HALF_SOLID_EDGE,
	HALF_SOLID_CENTERED,
	QUARTER_FORCE,
	CORNER_SOLID,
	ONEWAY
}

@export var mode:Modes = Modes.FULL_FORCE:
	set(value):
		mode = value
		if Engine.is_editor_hint(): set_sprite_frame()
@export var direction:Statics.DirsCardinal = Statics.DirsCardinal.DOWN:
	set(value):
		direction = value
		if Engine.is_editor_hint(): set_sprite_frame()

## Reference to the currently enabled hitbox
var active_box:CollisionShape2D
## Collective reference to all hitboxes for easy resetting
var all_boxes:Array[CollisionShape2D] = []

## Determines whether or not the player area should detect the player and eject them from inside the collision
var push_player:bool = false
## Determines whether or not the hitbox should turn to face the direction the player is coming at the
## tile from in order to create a dual-ended push collider
var face_player:bool = false
## Will be set if the active hitbox has been turned around from where it starts to face the player
var box_flipped:bool = false
## Will be set to the player body if the player is currently intersecting within this collision tile
var player_intersecting:Variant = null

## The fully square hitbox
@export var box_main:CollisionShape2D
## The three-quarter-size hitbox aligned to an edge
@export var box_three_quarter:CollisionShape2D
## The half-size hitbox aligned to an edge
@export var box_half_edge:CollisionShape2D
## The centered half-size hitbox
@export var box_half_center:CollisionShape2D
## The quarter-size hitbox aligned to an edge
@export var box_quarter:CollisionShape2D
## The quarter-size hitbox aligned to one corner of the main square
@export var box_corner:CollisionShape2D
## The area that detects when the player intersects this tile
@export var area:Area2D
## The collision shape attached to the area
@export var area_box:CollisionShape2D
## The debug sprite that displays what collision mode this tile is set to
@export var sprite:Sprite2D
#endregion


func _ready() -> void:
	area_box.shape = RectangleShape2D.new()
	#set_mode(mode, direction)
	pass


func _process(_delta: float) -> void:
	if face_player:
		var p_pos:Vector2 = Player.instance.position
		var flip_on:bool = false
		var flip_off:bool = false
		match direction:
			Statics.DirsCardinal.DOWN:
				if p_pos.y > position.y and not box_flipped: flip_on = true
				if p_pos.y < position.y and box_flipped: flip_off = true
			Statics.DirsCardinal.LEFT:
				if p_pos.x > position.x and not box_flipped: flip_on = true
				if p_pos.x < position.x and box_flipped: flip_off = true
			Statics.DirsCardinal.RIGHT:
				if p_pos.x < position.x and not box_flipped: flip_on = true
				if p_pos.x > position.x and box_flipped: flip_off = true
			Statics.DirsCardinal.UP:
				if p_pos.y < position.y and not box_flipped: flip_on = true
				if p_pos.y > position.y and box_flipped: flip_off = true
		if flip_on:
			active_box.rotation_degrees = 180
			box_flipped = true
		if flip_off:
			active_box.rotation_degrees = 0
			box_flipped = false
	
	if player_intersecting:
		_push_player(player_intersecting)
	
	sprite.visible = Statics.show_invis_entites


func set_mode(_mode:Modes, _dir:Statics.DirsCardinal) -> void:
	mode = _mode
	direction = _dir
	box_main.one_way_collision = false
	push_player = false
	face_player = false
	box_flipped = false
	set_sprite_frame()
	match _mode:
		Modes.FULL_FORCE:
			set_full()
			box_main.one_way_collision = true
			push_player = true
		Modes.FULL_FORCE_DUALEND:
			set_full()
			box_main.one_way_collision = true
			push_player = true
			face_player = true
		Modes.THREEQUARTER_FORCE:
			set_three_quarter()
			box_three_quarter.one_way_collision = true
			push_player = true
		Modes.THREEQUARTER_FORCE_DUALEND:
			set_three_quarter()
			box_three_quarter.one_way_collision = true
			push_player = true
			face_player = true
		Modes.HALF_FORCE:
			set_half_edge()
			box_half_edge.one_way_collision = true
			push_player = true
		Modes.HALF_SOLID_EDGE:
			set_half_edge()
		Modes.HALF_SOLID_CENTERED:
			set_half_center()
		Modes.QUARTER_FORCE:
			set_quarter()
			box_quarter.one_way_collision = true
			push_player = true
		Modes.CORNER_SOLID:
			set_corner()
		Modes.ONEWAY:
			set_full()
			box_main.one_way_collision = true
	match _dir:
		Statics.DirsCardinal.DOWN: rotation_degrees = 0.0
		Statics.DirsCardinal.LEFT: rotation_degrees = 90.0
		Statics.DirsCardinal.RIGHT: rotation_degrees = -90.0
		Statics.DirsCardinal.UP: rotation_degrees = 180.0
	sprite.rotation_degrees = -rotation_degrees
	match_area_to_active()


#region Box configuration
## Disables all hitboxes
func disable_all() -> void:
	if all_boxes.is_empty():
		catalog_all()
	for box in all_boxes:
		box.disabled = true
		box.one_way_collision = false
		box.rotation_degrees = 0.0


## Fills the collective array with all hitboxes attached to this tile
func catalog_all() -> void:
	all_boxes = [
		box_main,
		box_three_quarter,
		box_half_edge,
		box_half_center,
		box_quarter,
		box_corner
	]


## Sets the main hitbox as active
func set_full() -> void:
	disable_all()
	box_main.disabled = false
	active_box = box_main


## Sets the three-quarter hitbox as active
func set_three_quarter() -> void:
	disable_all()
	box_three_quarter.disabled = false
	active_box = box_three_quarter


## Sets the edge half hitbox as active
func set_half_edge() -> void:
	disable_all()
	box_half_edge.disabled = false
	active_box = box_half_edge


## Sets the centered half hitbox as active
func set_half_center() -> void:
	disable_all()
	box_half_center.disabled = false
	active_box = box_half_center


## Sets the quarter hitbox as active
func set_quarter() -> void:
	disable_all()
	box_quarter.disabled = false
	active_box = box_quarter


## Sets the corner hitbox as active
func set_corner() -> void:
	disable_all()
	box_corner.disabled = false
	active_box = box_corner


## Sets the collision layer of this tile to be that of general world collision
func set_collision_world() -> void:
	collision_layer = 1


## Sets the collision layer of this tile to be that of enemy world collision
func set_collision_enemy() -> void:
	collision_layer = 2


## Aligns the player detection area's position and size to match the active hitbox
func match_area_to_active() -> void:
	area.position = active_box.position
	if active_box:
		area_box.shape.size = active_box.shape.size
		area_box.shape.size -= Vector2(2, 2)
#endregion


## Sets the correct frame to the sprite to mimic this tile's collision mode
func set_sprite_frame() -> void:
	var dir:int = 0
	match direction:
		Statics.DirsCardinal.LEFT: dir = 1
		Statics.DirsCardinal.RIGHT: dir = 2
		Statics.DirsCardinal.UP: dir = 3
	match mode:
		Modes.FULL_FORCE:
			sprite.frame_coords = Vector2i(3, dir)
		Modes.FULL_FORCE_DUALEND:
			sprite.frame_coords = Vector2i(
				3 if (dir == 1 or dir == 2) else 2,
				6
			)
		Modes.THREEQUARTER_FORCE:
			sprite.frame_coords = Vector2i(2, dir)
		Modes.THREEQUARTER_FORCE_DUALEND:
			sprite.frame_coords = Vector2i(dir, 7)
		Modes.HALF_FORCE:
			sprite.frame_coords = Vector2i(1, dir)
		Modes.HALF_SOLID_EDGE:
			sprite.frame_coords = Vector2i(dir, 4)
		Modes.HALF_SOLID_CENTERED:
			sprite.frame_coords = Vector2i(
				1 if (dir == 1 or dir == 2) else 0,
				6
			)
		Modes.QUARTER_FORCE:
			sprite.frame_coords = Vector2i(0, dir)
		Modes.CORNER_SOLID:
			sprite.frame_coords = Vector2i(dir, 5)
		Modes.ONEWAY:
			sprite.frame_coords = Vector2i(dir, 8)


func _on_player_enter(_body) -> void:
	player_intersecting = _body
	_push_player(_body)

func _on_player_exit(_body) -> void:
	player_intersecting = null


func _push_player(body) -> void:
	var difference:Vector2 = Vector2(
		absf(body.global_position.x - active_box.global_position.x),
		absf(body.global_position.y - active_box.global_position.y)
	)
	var p_box_size:Vector2 = Player.instance.get_box_size()
	var t_box_size:Vector2 = _get_active_box_size()
	var final_size:Vector2 = (p_box_size + t_box_size) * 0.5
	var push_pos:Vector2 = body.position
	if push_player:
		match direction:
			Statics.DirsCardinal.DOWN:
				if difference.y > final_size.y:
					return
				push_pos.y = active_box.global_position.y - final_size.y - 2
				if box_flipped:
					push_pos.y = active_box.global_position.y + final_size.y + 2
			Statics.DirsCardinal.LEFT:
				if difference.x > final_size.y:
					return
				push_pos.x = active_box.global_position.x + final_size.x + 2
				if box_flipped:
					push_pos.x = active_box.global_position.x - final_size.x - 2
			Statics.DirsCardinal.RIGHT:
				if difference.x > final_size.y:
					return
				push_pos.x = active_box.global_position.x - final_size.x - 2
				if box_flipped:
					push_pos.x = active_box.global_position.x + final_size.x + 2
			Statics.DirsCardinal.UP:
				if difference.y > final_size.y:
					return
				push_pos.y = active_box.global_position.y + final_size.y + 2
				if box_flipped:
					push_pos.y = active_box.global_position.y - final_size.y - 2
		#print(push_pos)
		body.position = push_pos
		#print(body.position)


func _get_active_box_size() -> Vector2:
	var size:Vector2 = active_box.shape.size
	if direction == Statics.DirsCardinal.LEFT or direction == Statics.DirsCardinal.RIGHT:
		size = Vector2(size.y, size.x)
	return size
