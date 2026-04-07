extends Node2D


#region Variables
const SPR_SIZE:int = 16
const FADE_ALPHA:float = 0.1
const FADE_MIN:float = 16.0
const FADE_DISTANCE:float = 128.0

var elapsed:float = 0.0
var effect_dir:Vector2 = Vector2.DOWN
var anim_prefix:String = "floor_"
var anim_path:String = ""

@export var segments:int = 1
@export var surface:Statics.DirsSurface

@onready var sprites:Array[JsonSprite2D] = [ $"Sprite" ]
#endregion


func _ready() -> void:
	if segments < 1:
		segments = 1
	
	anim_path = sprites[0].texture_path
	
	var extend_dir:Vector2 = Vector2.RIGHT
	match surface:
		Statics.DirsSurface.LWALL:
			effect_dir = Vector2.LEFT
			extend_dir = Vector2.DOWN
			anim_prefix = "lwall_"
		Statics.DirsSurface.RWALL:
			effect_dir = Vector2.RIGHT
			extend_dir = Vector2.DOWN
			anim_prefix = "rwall_"
		Statics.DirsSurface.CEILING:
			effect_dir = Vector2.UP
			anim_prefix = "ceiling_"
	
	segments -= 1
	sprites[0].position -= extend_dir * segments * (SPR_SIZE * 0.5)
	var origin:Vector2 = sprites[0].position
	for i in range(segments):
		var j:int = i + 1
		var new_main:JsonSprite2D = JsonSprite2D.new()
		new_main.texture_path = anim_path
		add_child(new_main)
		sprites.append(new_main)
		new_main.position = origin + extend_dir * j * SPR_SIZE
	for spr in sprites:
		spr.action = anim_prefix + "intro"
		spr._process(0.0)


func _process(_delta: float) -> void:
	#pass
	if Player.instance:
		var p_pos:Vector2 = Player.instance.position
		for spr in sprites:
			spr.modulate.a = FADE_ALPHA
			var dist:float = spr.global_position.distance_to(p_pos)
			if dist < FADE_DISTANCE:
				var weight:float = inverse_lerp(FADE_MIN, FADE_DISTANCE, dist)
				spr.modulate.a = lerpf(1.0, FADE_ALPHA, clampf(weight, 0.0, 1.0))
