@icon("res://Editor/ico/Room.svg")
class_name Room
extends Node2D


#region Variables
#region Export
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


#region Internals
@onready var layer_entity:Node2D = $"EntityLayer"
@onready var map_entity:Node2D = $"EntityLayer/Map"
@onready var layer_fg2:Node2D = $"FG2Layer"
@onready var map_fg2:Node2D = $"FG2Layer/Map"
@onready var layer_fg1:Node2D = $"FG1Layer"
@onready var map_fg1:Node2D = $"FG1Layer/Map"
@onready var layer_ground:Node2D = $"GroundLayer"
@onready var map_ground:Node2D = $"GroundLayer/Map"
@onready var layer_bg1:Node2D = $"BG1Layer"
@onready var map_bg1:Node2D = $"BG1Layer/Map"
@onready var layer_bg2:Node2D = $"BG2Layer"
@onready var map_bg2:Node2D = $"BG2Layer/Map"
@onready var layer_sky:Node2D = $"SkyLayer"
@onready var map_sky:Node2D = $"SkyLayer/Map"
#endregion
#endregion


func instance():
	pass


func _process(delta):
	pass
