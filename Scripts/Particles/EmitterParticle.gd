# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name EmitterParticle
extends Sprite2D


#region Variables
## The local position that this particle starts at
var origin:Vector2 = Vector2.ZERO
## The amount of time in seconds that this particle has been active for
var lifetime:float = 0.0
## The maximum amount of time in seconds that this particle can exist for before deactivating
var max_lifetime:float = 0.0
## The color this particle starts at when spawned
var start_color:Color = Color.WHITE
## The color this particle ends at when deactivating
var end_color:Color = Color.WHITE
## If set, this particle will treat its [code]dir_radial[/code] as a rotation
## about the [code]origin[/code], rather than a direction to travel in
var orbital:bool = false
## The direction in degrees from [code]Vector2.RIGHT[/code] that this particle is traveling in.
## If [code]orbital[/code] is set, this instead acts as a rotation in degrees from
## [code]Vector2.RIGHT[/code] about this particle's [code]origin[/code]
var dir_radial:float = 0.0
## The linear forward velocity of the particle in units per second
var vel_radial:float = 0.0
## The velocity in degrees per second that this particle's [dir_radial] changes by
var vel_rotary:float = 0.0
#endregion


func init_particle(_from:Vector2, _life:float, _start_color:Color, _end_color:Color) -> void:
	visible = true
	origin = _from
	position = origin
	lifetime = 0.0
	max_lifetime = _life
	start_color = _start_color
	modulate = _start_color
	end_color = _end_color


func _process(delta: float) -> void:
	if lifetime >= max_lifetime:
		visible = false
		return
	
	var weight:float = inverse_lerp(0.0, max_lifetime, lifetime)
	
	modulate = start_color.lerp(end_color, weight)
	dir_radial += vel_rotary * delta
	if orbital:
		position += origin.direction_to(position) * vel_radial * delta
		position = (position - origin).rotated(deg_to_rad(vel_rotary) * delta) + origin
	else:
		position += Vector2.RIGHT.rotated(dir_radial) * vel_radial * delta
	
	lifetime += delta
