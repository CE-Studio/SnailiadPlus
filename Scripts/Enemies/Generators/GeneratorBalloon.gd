# Copyright 2026 CE-Studio: AGPL-3.0-only
# Original code Copyright 2011 Auriplane, used with permission
class_name GeneratorBalloon
extends Enemy


#region Variables
const TIMEOUTS:Array = [
	0.60153, 0.48509, 0.70037, 0.66276, 0.70802, 0.79541, 0.62043, 0.5796, 0.99605, 0.15058,
	0.72121, 0.86851, 0.64371, 0.76708, 0.89401, 0.52828, 0.72309, 0.15963, 0.15116, 0.1799,
	0.27829, 0.40878, 0.92538, 0.45074, 0.18865, 0.59797, 0.4318, 0.94098, 0.23463, 0.29221,
	0.59734, 0.34877, 0.81676, 0.57617, 0.14883, 0.16094, 0.14123, 0.57931, 0.85924, 0.22828,
	0.63834, 0.10387, 0.54746, 0.24897, 0.11105, 0.49748, 0.54746, 0.19405, 0.79792, 0.36023,
	0.53726, 0.78544, 0.60425, 0.83512, 0.01696, 0.10451, 0.01513, 0.78678, 0.51617, 0.24251
]
const BASE_TIMEOUT:float = 8.0
const ENEMY:PackedScene = preload("uid://cjla3pfitceec")

var timeout:float = 0.0
var pointer:int = 0

@onready var spr:Sprite2D = $"Sprite2D"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.GHOSTBALL
	super.spawn()
	
	spr.modulate.a = 0.0
	pointer = abs(floori(position.x * 4 + position.y * 20)) % TIMEOUTS.size()
	timeout = TIMEOUTS[pointer] * BASE_TIMEOUT * (0.5 if hard_mode else 1.0)
	timeout = fmod(timeout, fmod(position.x, 20.0))


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	spr.modulate.a = 0.5 if Statics.show_invis_entites else 0.0
	
	timeout -= delta
	if timeout <= 0.0:
		pointer = (pointer + 1) % TIMEOUTS.size()
		timeout = TIMEOUTS[pointer] * BASE_TIMEOUT * (0.5 if hard_mode else 1.0)
		var cam_center = UICore.instance.get_cam_center_pos()
		var new_balloon:Balloon = ENEMY.instantiate()
		new_balloon.position = cam_center + Vector2(randf_range(-200.0, 200.0), 120 + 48)
		GameCore.instance.current_room.layer_ground.add_child(new_balloon)
		new_balloon.spawn_drift_amount = Ghostball.SPAWN_DRIFT_DIST
