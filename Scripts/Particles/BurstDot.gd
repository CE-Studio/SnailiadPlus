# Copyright 2026 CE-Studio: AGPL-3.0-only
extends Particle


#region Variables
const BURST_TIME_VARIANCE:float = 0.25
const COALESCE_TIME_VARIANCE:float = 0.15
const BURST_EASE:float = 0.4
const COALESCE_EASE:float = 4.0

var direction:Vector2 = Vector2.ZERO
var lifetime:float = 0.0
var elapsed:float = 0.0
var origin:Vector2 = Vector2.ZERO
var burst_dist:float = 120.0
var burst_time:float = 0.7
var burst_target:Vector2 = Vector2.ZERO
var coalesce_time:float = 0.4
var coalesce_target:Vector2 = Vector2.ZERO
#endregion


# Data accepted: [ Lifetime (float), Burst distance (float), Coalesce target (Vector2 = Vector2.ZERO) ]
func _spawn(_data:Array) -> void:
	assert(_data.size() > 1,
		"BurstDot spawn data array must contain sufficient data.")
	assert(_data[0] is float,
		"BurstDot spawn data array's first entry (lifetime) must be a float.")
	timer.wait_time = _data[0]
	lifetime = _data[0]
	assert(_data[1] is float,
		"BurstDot spawn data array's second entry (burst distance) must be a float.")
	burst_dist = _data[1]
	if _data.size() > 2:
		assert(_data[2] is Vector2,
			"BurstDot spawn data array's third entry (coalesce target) must be a Vector2.")
		coalesce_target = _data[2]
	
	origin = position
	direction = Vector2.RIGHT.rotated(randf() * TAU)
	burst_target = position + direction * burst_dist
	burst_time += randf_range(-1.0, 1.0) * BURST_TIME_VARIANCE
	burst_time = lerpf(0.0, lifetime, burst_time)
	coalesce_time += randf_range(-1.0, 1.0) * COALESCE_TIME_VARIANCE
	coalesce_time = lerpf(0.0, lifetime, coalesce_time)
	super._spawn(_data)


func _process(delta: float) -> void:
	elapsed += delta
	var burst_weight:float = clampf(inverse_lerp(0.0, burst_time, elapsed), 0.0, 1.0)
	burst_weight = ease(burst_weight, BURST_EASE)
	var coalesce_weight:float = clampf(inverse_lerp(coalesce_time, lifetime, elapsed), 0.0, 1.0)
	coalesce_weight = ease(coalesce_weight, COALESCE_EASE)
	position = origin.lerp(burst_target, burst_weight)
	if coalesce_target != Vector2.ZERO:
		position = position.lerp(coalesce_target, coalesce_weight)
	else:
		modulate.a = lerpf(1.0, 0.0, coalesce_weight)
	super._process(delta)
