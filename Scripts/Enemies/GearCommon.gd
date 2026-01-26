class_name GearCommon
extends Enemy


#region Variables
const REACT_DISTANCE:float = 90.0
const START_OFFSET:float = 80.0
const SPEED:Vector2 = Vector2(500.0, 410.0)
const ACCEL:Vector2 = Vector2(500.0, 410.0)
const FAST_IDLE_MIN:float = 1.5
const FAST_IDLE_MAX:float = 6.0

@export var direction:Statics.DirsCardinal = Statics.DirsCardinal.DOWN
var going:bool = false
var fast_idle_timer:float = randf_range(FAST_IDLE_MIN, FAST_IDLE_MAX)
var on_screen_once:bool = false
var closest_onscreen_point:Vector2 = origin
@onready var sfx_charge:AudioStreamPlayer = $"Charge"
#endregion


func _ready() -> void:
	my_type = EnemyTypes.GEAR_COMMON
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	match direction:
		Statics.DirsCardinal.UP: position.y += START_OFFSET
		Statics.DirsCardinal.LEFT: position.x += START_OFFSET
		Statics.DirsCardinal.DOWN: position.y -= START_OFFSET
		Statics.DirsCardinal.RIGHT: position.x -= START_OFFSET
	sprite.action = "idle"
	
	closest_onscreen_point = GameCore.instance.current_room.bounds.get_closest_point_to(origin)


func _process(delta: float) -> void:
	super(delta)
	if not going:
		fast_idle_timer -= delta
		if fast_idle_timer <= 0.0:
			fast_idle_timer = randf_range(FAST_IDLE_MIN, FAST_IDLE_MAX)
			sprite.action = "idle_fast"
	if not ai_active:
		return
	
	var player_pos:Vector2 = GameCore.instance.player.position
	match direction:
		Statics.DirsCardinal.UP:
			if not going:
				if abs(player_pos.x - position.x) < REACT_DISTANCE:
					going = true
					velocity.y = -SPEED.y
					sprite.action = "charge_up"
			else:
				move_and_slide()
				velocity.y += ACCEL.y * delta
				if position.y >= origin.y + START_OFFSET + 8:
					queue_free()
		Statics.DirsCardinal.LEFT:
			if not going:
				if abs(player_pos.y - position.y) < REACT_DISTANCE:
					going = true
					velocity.x = -SPEED.x
					sprite.action = "charge_left"
			else:
				move_and_slide()
				velocity.x += ACCEL.x * delta
				if position.x >= origin.x + START_OFFSET + 8:
					queue_free()
		Statics.DirsCardinal.DOWN:
			if not going:
				if abs(player_pos.x - position.x) < REACT_DISTANCE:
					going = true
					velocity.y = SPEED.y
					sprite.action = "charge_down"
			else:
				move_and_slide()
				velocity.y -= ACCEL.y * delta
				if position.y <= origin.y - START_OFFSET - 8:
					queue_free()
		Statics.DirsCardinal.RIGHT:
			if not going:
				if abs(player_pos.y - position.y) < REACT_DISTANCE:
					going = true
					velocity.x = SPEED.x
					sprite.action = "charge_right"
			else:
				move_and_slide()
				velocity.x -= ACCEL.x * delta
				if position.x <= origin.x - START_OFFSET - 8:
					queue_free()
	
	if vis.is_on_screen() and going and not on_screen_once:
		on_screen_once = true
		sfx_charge.play()
		sprite.position = Vector2.ZERO
		can_damage = true
	
	if not on_screen_once:
		var ratio:int = ProjectSettings.get_setting("display/window/size/aspect_ratio")
		var offset = Vector2i(200, 120) + Vector2i(Statics.ASPECT_RATIO_OFFSETS[ratio] * 0.5)
		match direction:
			Statics.DirsCardinal.UP:
				if abs(position.y - closest_onscreen_point.y) > offset.y:
					sprite.global_position = Vector2(global_position.x, closest_onscreen_point.y + offset.y)
			Statics.DirsCardinal.DOWN:
				if abs(position.y - closest_onscreen_point.y) > offset.y:
					sprite.global_position = Vector2(global_position.x, closest_onscreen_point.y - offset.y)
			Statics.DirsCardinal.LEFT:
				if abs(position.x - closest_onscreen_point.x) > offset.x:
					sprite.global_position = Vector2(closest_onscreen_point.x + offset.x, global_position.y)
			Statics.DirsCardinal.RIGHT:
				if abs(position.x - closest_onscreen_point.x) > offset.x:
					sprite.global_position = Vector2(closest_onscreen_point.x - offset.x, global_position.y)
