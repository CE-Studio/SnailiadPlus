class_name Canon
extends Enemy


#region Variables
const AIM_TIMEOUT:float = 0.25
const SHOT_TIMEOUT:float = 4.0
const SHOT_SPEED:float = 140.0
const TICK_MULT:float = 0.6

var aim_timeout:float = 0.0
var shot_timeout:float = 0.0
var base_dir:Statics.DirsSurface = Statics.DirsSurface.FLOOR

@onready var base_spr:JsonSprite2D = $"Base"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.CANON
	hitbox = $"Area2D"
	sprite = $"Body"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	if display_mode:
		sprite.action = "UL_idle"
		base_spr.action = "floor_idle"
	else:
		match base_dir:
			Statics.DirsSurface.FLOOR:
				sprite.action = "U_idle"
				base_spr.action = "floor_idle"
			Statics.DirsSurface.LWALL:
				sprite.action = "R_idle"
				base_spr.action = "lwall_idle"
			Statics.DirsSurface.RWALL:
				sprite.action = "L_idle"
				base_spr.action = "rwall_idle"
			Statics.DirsSurface.CEILING:
				sprite.action = "D_idle"
				base_spr.action = "ceiling_idle"
		aim_timeout = fmod(position.x / 38.2, 0.25)
		shot_timeout = fmod(SHOT_TIMEOUT + position.x / 96.0, 6.0)


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	aim_timeout -= delta
	if aim_timeout <= 0.0:
		aim_timeout = AIM_TIMEOUT
		#tick_aim
	if vis.is_on_screen():
		shot_timeout -= delta * TICK_MULT
		if shot_timeout <= 0.0:
			shot_timeout = SHOT_TIMEOUT
			#shoot
