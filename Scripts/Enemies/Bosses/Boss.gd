class_name Boss
extends Enemy


#region Variables
@export var battle_music:MusicManager.Loops = MusicManager.Loops.Boss1
@export var phase_changes:Array[float] = [ 0 ]

const DEATH_WIGGLE_RANGE:float = 2.0
const DEATH_WIGGLE_TIME:float = 1.8

var intro_delay:bool = true
var phase:int = 0
var current_anim:String = ""
var in_death_anim:bool = false
var nodes_to_wiggle:Array = []
var death_timer:float = 0.0

var health_bar:BossHealthBar = null
#endregion


func _physics_process(delta: float) -> void:
	super(delta)
	
	while phase < phase_changes.size() and health < max_health * phase_changes[phase]:
		advance_phase()
	
	if in_death_anim:
		for node in nodes_to_wiggle:
			node.position = Vector2(
				randf_range(-DEATH_WIGGLE_RANGE, DEATH_WIGGLE_RANGE),
				randf_range(-DEATH_WIGGLE_RANGE, DEATH_WIGGLE_RANGE)
			)
		if death_timer <= 0.0:
			kill()
		death_timer -= delta


func _damage(health_lost:int, sound:bool = true) -> void:
	super(health_lost, sound)
	if health_bar:
		health_bar.update()


func advance_phase(count:int = 1) -> void:
	phase += count
	play_phase_anim()


func play_phase_anim(anim_name:String = "", set_as_current:bool = true) -> String:
	sprite.action = get_phase_anim(anim_name)
	if set_as_current and anim_name.strip_edges() != "":
		current_anim = anim_name
	return anim_name


func get_phase_anim(anim_name:String = "", prefix:String = "") -> String:
	if anim_name.strip_edges() == "":
		anim_name = current_anim
	return "p%d_%s" % [ phase, prefix + anim_name ]


func kill() -> void:
	if in_death_anim:
		super()
	else:
		in_death_anim = true
		ai_active = false
		can_damage = false
		invulnerable = true
		death_timer = DEATH_WIGGLE_TIME
