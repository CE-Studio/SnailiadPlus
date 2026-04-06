extends Node2D


#region Variables
const SPR_SIZE:int = 48

var elapsed:float = 0.0
var effect_dir:Vector2 = Vector2.DOWN
var anim_main:String = "floor_main"
var anim_eff:String = "floor_effect"
var anim_path:String = ""

@export var segments:int = 1
@export var surface:Statics.DirsSurface

@onready var effect_group_0:Node2D = $"EffectGroup0"
@onready var effect_group_1:Node2D = $"EffectGroup1"
@onready var sprites_main:Array[JsonSprite2D] = [ $"Main" ]
@onready var sprites_effect:Array[JsonSprite2D] = [ $"EffectGroup0/Effect", $"EffectGroup1/Effect" ]
#endregion


func _ready() -> void:
	if segments < 1:
		segments = 1
	
	anim_path = sprites_main[0].texture_path
	
	var extend_dir:Vector2 = Vector2.RIGHT
	match surface:
		Statics.DirsSurface.LWALL:
			effect_dir = Vector2.LEFT
			extend_dir = Vector2.DOWN
			anim_main = "lwall_main"
			anim_eff = "lwall_effect"
		Statics.DirsSurface.RWALL:
			effect_dir = Vector2.RIGHT
			extend_dir = Vector2.DOWN
			anim_main = "rwall_main"
			anim_eff = "rwall_effect"
		Statics.DirsSurface.CEILING:
			effect_dir = Vector2.UP
			anim_main = "ceiling_main"
			anim_eff = "ceiling_effect"
	
	segments -= 1
	sprites_main[0].position -= extend_dir * segments * (SPR_SIZE * 0.5)
	sprites_effect[0].position -= extend_dir * segments * (SPR_SIZE * 0.5)
	sprites_effect[1].position = sprites_effect[0].position
	var origin:Vector2 = sprites_main[0].position
	for i in range(segments):
		var j:int = i + 1
		var new_main:JsonSprite2D = JsonSprite2D.new()
		new_main.texture_path = anim_path
		add_child(new_main)
		sprites_main.append(new_main)
		new_main.position = origin + extend_dir * j * SPR_SIZE
		var new_eff0:JsonSprite2D = JsonSprite2D.new()
		new_eff0.texture_path = anim_path
		effect_group_0.add_child(new_eff0)
		sprites_effect.append(new_eff0)
		new_eff0.position = new_main.position
		var new_eff1:JsonSprite2D = JsonSprite2D.new()
		new_eff1.texture_path = anim_path
		effect_group_1.add_child(new_eff1)
		sprites_effect.append(new_eff1)
		new_eff1.position = new_main.position
	for spr in sprites_main:
		spr.action = anim_main
	for spr in sprites_effect:
		spr.action = anim_eff
