extends JsonSprite2D


#region Variables
const MAX_MULT:float = 0.4
const PLAYER_PROX_MULT:float = 0.3
const ADD_OFFSET:float = 0.05
const IMPACT_DECAY:float = 0.8
const IMPACT_FALLOFF:float = 0.15
const IMPACT_ADD_MULT:float = 0.3
const IMPACT_DELAY_MULT:float = Statics.FRAC_64

var rand_cycle_0:float
var rand_cycle_1:float
var player_prox_limit:float = 0.0
var current_prox_add:float = 0.0
var decay_rate:float = 0.0
var col_fade_rate:float = 0.0
var current_col_array:Array = []
var shimmer:bool = true
var impact_add:float = 0.0
var impact_elapsed:float = 0.0
var last_impact_add:float = 0.0
var last_impact_elapsed:float = 0.0

var elapsed:float = 0.0
var intro_fade:float = 4.0
var last_a:float = 0.0
var target_color:Color = Color.BLACK
#endregion


func _ready() -> void:
	super()
	rand_cycle_0 = randf_range(0.25, 1.5)
	rand_cycle_1 = randf_range(0.4, 3.0)
	player_prox_limit = randf_range(24.0, 88.0)
	decay_rate = randf_range(0.2, 1.6)
	col_fade_rate = randf_range(0.6, 1.2)
	set_color(meta["col_intro"])


func _process(delta: float) -> void:
	super(delta)
	
	modulate = modulate.lerp(target_color, col_fade_rate * delta)
	
	var this_a:float = 0.0
	if shimmer:
		this_a = sin(elapsed * rand_cycle_0) * 0.5
		this_a += sin(elapsed * rand_cycle_1) * 0.5
		this_a = clampf(this_a * MAX_MULT + ADD_OFFSET, 0.0, 1.0)
	
	this_a -= (intro_fade * 0.3)
	
	var prox:float = global_position.distance_to(Player.instance.position)
	if prox < player_prox_limit:
		var this_prox:float = inverse_lerp(player_prox_limit, 0.0, prox)
		if this_prox > current_prox_add:
			current_prox_add = this_prox
	this_a += current_prox_add * PLAYER_PROX_MULT
	
	var impact_a:float = 0.0
	if impact_elapsed > 0.0 and impact_elapsed < IMPACT_DECAY:
		impact_a = inverse_lerp(IMPACT_DECAY, 0.0, impact_elapsed)
		impact_a = clampf(impact_a, 0.0, 1.0) * impact_add * IMPACT_ADD_MULT
	if last_impact_elapsed > 0.0 and last_impact_elapsed < IMPACT_DECAY:
		var last_impact_a = inverse_lerp(IMPACT_DECAY, 0.0, last_impact_elapsed)
		last_impact_a = clampf(last_impact_a, 0.0, 1.0) * last_impact_add * IMPACT_ADD_MULT
		if last_impact_a > impact_a:
			impact_a = last_impact_a
	this_a += impact_a
	impact_elapsed += delta
	last_impact_elapsed += delta
	
	modulate.a = this_a
	last_a = this_a
	elapsed += delta
	if intro_fade > 0.0:
		intro_fade = clampf(intro_fade - delta, 0.0, INF)
	current_prox_add = move_toward(current_prox_add, 0.0, decay_rate * delta)


func set_color(col_array:Array, instant:bool = false) -> void:
	if col_array.size() == 0:
		return
	var i:int = randi_range(0, col_array.size() - 1)
	target_color = Color(col_array[i])
	if instant:
		modulate = target_color
		modulate.a = last_a


func update_state(state:String, phase:int) -> void:
	set_color(meta["col_" + state + str(phase)])


func set_impact_flash(impact_point:Vector2) -> void:
	last_impact_add = impact_add
	last_impact_elapsed = impact_elapsed
	var dist:float = impact_point.distance_to(global_position)
	var prox:float = clampf(16.0 - dist * IMPACT_FALLOFF, 0.0, 1.0)
	impact_add = prox
	impact_elapsed = dist * Statics.FRAC_16 * -IMPACT_DELAY_MULT
