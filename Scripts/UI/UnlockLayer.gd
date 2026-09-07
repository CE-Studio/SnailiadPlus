# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name UnlockLayer
extends Node2D


const GRAVITY:float = 1200.0
const POP_VEL_X:Vector2 = Vector2(180.0, 600.0)
const POP_VEL_Y:Vector2 = Vector2(-520.0, -60.0)
const FRAME_START_Y_OFFSET:float = 240.0
const TEXT_START_Y_OFFSET:float = -64.0
const LOCK_ARCH_OFFSET:Vector2 = Vector2(0.0, -32.0)
const FRAME_SHAKE_MULT:float = 1.5
const BACK_FADE_COLOR:Color = Color("0000007f")
const IN_LERP_RATE:float = 8.0
const BEAM_ROT_SPEED:Vector2 = Vector2(40.0, 160.0)
const BEAM_GROW_SPEED:float = 4.0

enum Stages {
	INIT,
	IN,
	SHAKE,
	POP,
	AWAIT,
	OUT,
	FREE
}

var stage:Stages = Stages.INIT
var lock_body_vel:Vector2 = Vector2.ZERO
var lock_arch_vel:Vector2 = Vector2.ZERO
var queue:Array[Statics.Unlocks] = []
var beam_rot_speeds:Array[float] = []
var visible_beams:int = 0
@export var shake_strength:float = 0.0
@export var lock_arch_extension:float = 0.0

@export var frame:SnailySprite2D
@export var lock_body:SnailySprite2D
@export var lock_arch:SnailySprite2D
@export var backing_fade:Sprite2D
@export var top_text_group:Node2D
@export var text_condition:SnailyText
@export var text_unlocked:SnailyText
@export var text_reward:SnailyText
@export var anim:AnimationPlayer
@export var beam_group:Node2D
@export var beams:Array[Polygon2D] = []
@export var sfx_beam1:AudioStreamPlayer
@export var sfx_beam2:AudioStreamPlayer
@export var sfx_beam3:AudioStreamPlayer
@export var sfx_pop:AudioStreamPlayer


func _ready() -> void:
	queue.append(randi_range(0, 6) as Statics.Unlocks) # temp
	backing_fade.modulate = Color(BACK_FADE_COLOR.r, BACK_FADE_COLOR.g, BACK_FADE_COLOR.b, 0.0)
	_set_stage_init()
	beam_group.visible = false
	text_reward.visible = false
	text_reward.set_default_flashy(2)
	text_reward.enable_rainbow_scroll()


func _process(delta: float) -> void:
	match stage:
		Stages.INIT:
			backing_fade.modulate.a = move_toward(backing_fade.modulate.a, BACK_FADE_COLOR.a, delta)
			if backing_fade.modulate.a == BACK_FADE_COLOR.a:
				_set_stage_in()
		Stages.IN:
			var weight:float = IN_LERP_RATE * delta
			frame.position = frame.position.lerp(Vector2.ZERO, weight)
			_set_lock_pos(frame.position)
			top_text_group.position = top_text_group.position.lerp(Vector2.ZERO, weight)
		Stages.SHAKE:
			var weight:float = randf() * shake_strength
			frame.position = Vector2(
				randf_range(-1.0, 1.0),
				randf_range(-1.0, 1.0)
			).normalized() * weight * FRAME_SHAKE_MULT
			_set_lock_pos(Vector2(
				randf_range(-1.0, 1.0),
				randf_range(-1.0, 1.0)
			).normalized() * weight)
			_process_beams(delta)
		Stages.POP:
			lock_body.position += lock_body_vel * delta
			lock_body_vel.y += GRAVITY * delta
			lock_arch.position += lock_arch_vel * delta
			lock_arch_vel.y += GRAVITY * delta


func _set_lock_pos(pos:Vector2) -> void:
	lock_body.position = pos
	lock_arch.position = lock_body.position + LOCK_ARCH_OFFSET
	lock_arch.position.y -= lock_arch_extension


func _set_text() -> void:
	if queue.is_empty():
		return
	match queue[0]:
		Statics.Unlocks.BOSS_RUSH:
			text_condition.set_snaily_text(tr(&"By beating the game,"))
			text_reward.set_snaily_text(tr(&"Boss Rush!!"))
		Statics.Unlocks.CHAR_SEL:
			text_condition.set_snaily_text(tr(&"By beating Boss Rush,"))
			text_reward.set_snaily_text(tr(&"Character Select!!"))
		Statics.Unlocks.ABSURD_DIFF:
			text_condition.set_snaily_text(tr(&"By beating the game in less than 30 minutes,"))
			text_reward.set_snaily_text(tr(&"Absurd Difficulty!!"))
		Statics.Unlocks.ITEM_RANDO:
			text_condition.set_snaily_text(tr(&"By collecting all items,"))
			text_reward.set_snaily_text(tr(&"Randomizer Mode!!"))
		Statics.Unlocks.OPEN_MAP:
			text_condition.set_snaily_text(tr(&"By filling out the map completely,"))
			text_reward.set_snaily_text(tr(&"Open Map!!"))
		Statics.Unlocks.SIX_HUNDO:
			text_condition.set_snaily_text(tr(&"By beating the game with a new character,"))
			text_reward.set_snaily_text(tr(&"600% Mode!!"))
		Statics.Unlocks.CHAOS_MODE:
			text_condition.set_snaily_text(tr(&"By beating the game on Absurd Difficulty,"))
			text_reward.set_snaily_text(tr(&"Chaos Mode!!"))


func _set_frame_reward_anim() -> void:
	if queue.is_empty():
		return
	match queue[0]:
		Statics.Unlocks.BOSS_RUSH: frame.play("boss_rush")
		Statics.Unlocks.CHAR_SEL: frame.play("char_sel")
		Statics.Unlocks.ABSURD_DIFF: frame.play("absurd_diff")
		Statics.Unlocks.ITEM_RANDO: frame.play("item_rando")
		Statics.Unlocks.OPEN_MAP: frame.play("open_map")
		Statics.Unlocks.SIX_HUNDO: frame.play("six_hundo")
		Statics.Unlocks.CHAOS_MODE: frame.play("chaos_mode")


func _init_beams() -> void:
	beams.shuffle()
	beam_rot_speeds.clear()
	for beam in beams:
		beam.rotation_degrees = randf() * 360.0
		beam_rot_speeds.append(
			randf_range(BEAM_ROT_SPEED.x, BEAM_ROT_SPEED.y) * (-1.0 if randf() < 0.5 else 1.0)
		)
		beam.scale = Vector2(0.1, 0.1)
	visible_beams = 0


func _show_beam(count:int, sound_id:int) -> void:
	count = clampi(count, 0, beams.size())
	visible_beams = clampi(visible_beams + count, 0, beams.size())
	match sound_id:
		0: sfx_beam1.play()
		1: sfx_beam2.play()
		2: sfx_beam3.play()


func _hide_beams() -> void:
	for beam in beams:
		beam.scale = Vector2(0.1, 0.1)


func _process_beams(delta:float) -> void:
	for i in range(beams.size()):
		if i >= visible_beams:
			break
		var beam:Polygon2D = beams[i]
		beam.rotation_degrees += beam_rot_speeds[i] * delta
		if beam.scale != Vector2.ONE:
			beam.scale = beam.scale.move_toward(Vector2.ONE, BEAM_GROW_SPEED * delta)


func _set_stage_init() -> void:
	stage = Stages.INIT
	frame.position.y = FRAME_START_Y_OFFSET
	lock_body.position.y = FRAME_START_Y_OFFSET
	lock_arch.position.y = FRAME_START_Y_OFFSET - LOCK_ARCH_OFFSET.y
	top_text_group.position.y = TEXT_START_Y_OFFSET
	frame.play("locked")
	lock_body.play("default")
	lock_arch.play("default")
	_init_beams()


func _set_stage_in() -> void:
	stage = Stages.IN
	_set_text()
	anim.play("sequence")


func _set_stage_shake() -> void:
	stage = Stages.SHAKE
	frame.position.y = 0.0
	_set_lock_pos(Vector2.ZERO)


func _set_stage_pop() -> void:
	stage = Stages.POP
	_set_frame_reward_anim()
	frame.position = Vector2.ZERO
	lock_body.play("fall")
	lock_body_vel = Vector2(
		randf_range(POP_VEL_X.x, POP_VEL_X.y) * (-1.0 if randf() < 0.5 else 1.0),
		randf_range(POP_VEL_Y.x, POP_VEL_Y.y)
	)
	lock_arch.play("fall")
	lock_arch_vel = Vector2(
		randf_range(POP_VEL_X.x, POP_VEL_X.y) * (-1.0 if randf() < 0.5 else 1.0),
		randf_range(POP_VEL_Y.x, POP_VEL_Y.y)
	)
	if sign(lock_body_vel.x) == sign(lock_arch_vel.x):
		lock_arch_vel.x *= -1
	if lock_arch_vel.y > lock_body_vel.y:
		var temp:float = lock_arch_vel.y
		lock_arch_vel.y = lock_body_vel.y
		lock_body_vel.y = temp
	sfx_pop.play()
	_hide_beams()


func _set_stage_await() -> void:
	stage = Stages.AWAIT


func _set_stage_out() -> void:
	stage = Stages.OUT


func _set_stage_free() -> void:
	stage = Stages.FREE
