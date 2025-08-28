class_name Boss
extends Enemy


#region Variables
@export var battle_music:MusicManager.Loops = MusicManager.Loops.Boss1
@export var phase_changes:Array[float] = [ 0 ]

var intro_delay:bool = true
var phase:int = 0
var current_anim:String = ""

var health_bar:BossHealthBar = null
#endregion


func _physics_process(delta: float) -> void:
	super(delta)
	
	while phase < phase_changes.size() and health < max_health * phase_changes[phase]:
		advance_phase()


func _damage(health_lost:int, sound:bool = true) -> void:
	super(health_lost, sound)
	if health_bar:
		health_bar.update()


func advance_phase(count:int = 1) -> void:
	phase += count
	play_phase_anim()


func play_phase_anim(anim_name:String = "") -> String:
	if anim_name.strip_edges() == "":
		anim_name = current_anim
	current_anim = anim_name
	anim_name = "p%d_%s" % [ phase, anim_name ]
	sprite.action = anim_name
	return anim_name
