@icon("res://Editor/ico/Room.svg")
extends Node2D
class_name Room


#region Variables
@export_group("General")
@export_enum(
	"Snail Town", "Mare Carelia", "Spiralis Silere", "Amastrida Abyssus",
	"Lux Lirata", "Shrine of Iris", "Boss Rush", "None:-1"
	) var area_id:int
@export var subarea_id:int = 0
@export var is_bonus_room:bool = false
@export_range(0.0, 1.0) var darkness_level:float = 0.0

@export_group("Layers")
@export_subgroup("Parallax")
@export var fg2_parallax:Vector2
@export var fg1_parallax:Vector2
@export var bg1_parallax:Vector2
@export var bg2_parallax:Vector2
@export var sky_parallax:Vector2
@export_subgroup("Offsets")
@export var fg2_offset:Vector2
@export var fg1_offset:Vector2
@export var bg1_offset:Vector2
@export var bg2_offset:Vector2
@export var sky_offset:Vector2
#endregion
