# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name GigaEnvironment
extends Node2D


#region Variables
const STRIPE_START:float = -300.0
const STRIPE_SPEED:float = 256.0
const STRIPE_LOWER:float = 540.0
const STRIPE_RESET:float = 840.0
const STRIPE_SIZE:float = 16.0
const STRIPE_ALPHA:float = 0.8
const STRIPE_FADE_MULT:float = 12.0

var giga:Gigasnail
var state:String = "intro"
var phase:int = 0

var ground_glow:float = 4.0
var ground_stripe_y:float = STRIPE_START

@export var bg:GigaBackground
@export var ground:Array[GigaGround] = []
@export var stars:GigaStarLayer
#endregion


func _process(delta: float) -> void:
	if ground_glow > 0.0:
		ground_glow -= delta
		if ground_glow < 0.0:
			ground_glow = 0.0
	ground_stripe_y += STRIPE_SPEED * delta
	while ground_stripe_y > STRIPE_LOWER:
		ground_stripe_y -= STRIPE_RESET


func connect_giga(_giga:Gigasnail) -> void:
	giga = _giga
	for surface in ground:
		surface.environment = self
		surface.env_linked = true
	bg.environment = self
	stars.environment = self
	stars.spawn()
	for star in stars.active_particles:
		star.boss = giga


func get_stripe_glow_at_y(spr_y:float) -> float:
	var difference:float = abs(spr_y - ground_stripe_y)
	var upper:float = STRIPE_SIZE
	if spr_y < ground_stripe_y:
		upper *= STRIPE_FADE_MULT
	var weight:float = inverse_lerp(0.0, upper, difference)
	weight = clampf(weight, 0.0, 1.0)
	return lerpf(STRIPE_ALPHA, 0.0, weight)


func set_state(_state:String) -> void:
	state = _state
	phase = giga.phase
	for surface in ground:
		surface.update_all_anim()
	bg.update_visible()
	stars.update_mode(_state)


func impact_ground(impact_point:Vector2) -> void:
	for surface in ground:
		surface.impact_ground(impact_point)


func despawn() -> void:
	stars.despawn()
	queue_free()
