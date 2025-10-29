extends Particle


#region Variables
const DUST_COUNT:int = 16
const SPIN_SPEED:float = TAU
const LEGACY_START_RADIUS:float = 234.0
const LEGACY_INWARD_SPEED:float = 90.0
const DYNAMIC_EASE_TIME:float = 0.8
const DYNAMIC_EASE_DELAY:float = 0.06
const DYNAMIC_MAX_RADIUS:float = 256.0
const DYNAMIC_MIN_RADIUS:float = 64.0
const DYNAMIC_MAIN_SHRINK_DELAY:float = 1.6
const DYNAMIC_SHRINK_MULT:float = 1.0
const DYNAMIC_SPIN_MULT:float = 0.25
const DYNAMIC_SPIN_MOD_MAX:float = 1.2
const DYNAMIC_SPIN_MOD_TIME:float = 2.6
const TYPES:Array = [ "normal", "ice", "gravity", "metal", "magnet", "corkscrew", "angel" ]

var legacy_anim:bool = false
var dynamic_main_mult:float = 1.0
var dynamic_spin_mod:float = 0.0
var radii:Array[float] = []
var elapsed:float = 0.0
var type:int = 0
var dusts:Array[Particle] = []
var rand_i:int = randi_range(0, DUST_COUNT - 1)
var restore_input:bool = false
var update_player_shell:int = Statics.get_shell_level()
var fade_music:bool = false
#endregion


func _spawn(_data:Array) -> void:
	super(_data)
	
	if _data.size() > 0 and _data[0] is int and not legacy_anim:
		type = _data[0]
	if _data.size() > 1 and _data[1] is bool:
		fade_music = _data[1]
	if _data.size() > 2 and _data[2] is bool:
		restore_input = _data[2]
	if _data.size() > 3 and _data[3] is int:
		update_player_shell = _data[3]
	
	for i in range(DUST_COUNT):
		dusts.append(Statics.spawn_particle("Dust", Room.Layers.GROUND, position))
		dusts[i].sprite.action = TYPES[type]
		radii.append(LEGACY_START_RADIUS if legacy_anim else DYNAMIC_MAX_RADIUS)
	_tick_legacy(0.0)
	if fade_music:
		GameCore.instance.music_manager.set_global_volume(0.0)


func _physics_process(delta:float) -> void:
	elapsed += delta
	position = GameCore.instance.player.position
	if legacy_anim:
		_tick_legacy(delta)
	else:
		_tick_dynamic(delta)


func _tick_legacy(delta:float) -> void:
	radii[0] -= delta * LEGACY_INWARD_SPEED
	if radii[0] <= 0.0:
		_despawn()
	else:
		for i in range(dusts.size()):
			var angle:float = TAU / DUST_COUNT * i + elapsed * SPIN_SPEED
			dusts[i].position = position + (Vector2(cos(angle), sin(angle)) * radii[0])


func _tick_dynamic(_delta:float) -> void:
	var main_radius_lerp:float = clampf((elapsed - DYNAMIC_MAIN_SHRINK_DELAY) * DYNAMIC_SHRINK_MULT, 0.0, 1.0)
	dynamic_main_mult = lerpf(1.0, 0.0, main_radius_lerp * main_radius_lerp * main_radius_lerp)
	
	var spin_mod_lerp:float = elapsed / DYNAMIC_SPIN_MOD_TIME
	dynamic_spin_mod = 1 + lerpf(0.0, DYNAMIC_SPIN_MOD_MAX, spin_mod_lerp * spin_mod_lerp * spin_mod_lerp)
	
	if dynamic_main_mult == 0.0:
		_despawn()
	else:
		for i in range(dusts.size()):
			var angle:float = TAU / DUST_COUNT * rand_i + elapsed * SPIN_SPEED * DYNAMIC_SPIN_MULT * dynamic_spin_mod
			var this_radius_mult:float = clampf(elapsed * DYNAMIC_EASE_TIME - DYNAMIC_EASE_DELAY * abs(-i + DUST_COUNT), 0.0, 1.0)
			radii[i] = lerpf(DYNAMIC_MAX_RADIUS, DYNAMIC_MIN_RADIUS, 1 - pow(1 - this_radius_mult, 5))
			dusts[i].position = position + (Vector2(cos(angle), sin(angle)) * radii[i] * dynamic_main_mult)
			rand_i = (rand_i + 1) % DUST_COUNT


func _despawn() -> void:
	if fade_music:
		GameCore.instance.music_manager.set_fade(1.0, 1.0, 0.75)
	for dust in dusts:
		dust.queue_free()
	if type != 0:
		Statics.spawn_particle("Transformation", Room.Layers.GROUND, position, [TYPES[type]])
	if restore_input:
		SInput.read_inputs = true
	GameCore.instance.player.update_shell_displayed(update_player_shell)
	queue_free()
