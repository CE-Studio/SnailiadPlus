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
const CENTER_JOLT_STRENGTH:float = 6.0
const CENTER_JOLY_RETURN:float = 7.5

enum Stages {
	INIT,
	IN,
	SHAKE,
	POP,
	AWAIT,
	OUT,
	FREE
}

signal finished

var stage:Stages = Stages.INIT
var lock_body_vel:Vector2 = Vector2.ZERO
var lock_arch_vel:Vector2 = Vector2.ZERO
var queue:Array[Statics.Unlocks] = []
var beam_rot_speeds:Array[float] = []
var visible_beams:int = 0
var center_default:Vector2 = Vector2.ZERO
var center_offset:Vector2 = Vector2.ZERO
var break_progress:int = 0
@export var shake_strength:float = 0.0
@export var lock_arch_extension:float = 0.0
@export var read_inputs:bool = false

@export var frame:SnailySprite2D
@export var lock_body:SnailySprite2D
@export var lock_arch:SnailySprite2D
@export var backing_fade:Sprite2D
@export var top_text_group:Node2D
@export var bottom_text_group:Node2D
@export var text_condition:SnailyText
@export var text_unlocked:SnailyText
@export var text_reward:SnailyText
@export var text_elaboration:SnailyText
@export var anim:AnimationPlayer
@export var center_group:Node2D
@export var beam_group:Node2D
@export var beams:Array[Polygon2D] = []
@export var sfx_beams:Array[AudioStreamPlayer]
@export var sfx_pop:AudioStreamPlayer
@export var advance_prompt:SnailySprite2D


func _ready() -> void:
	queue.append(randi_range(0, 6) as Statics.Unlocks) # temp
	queue.append(randi_range(0, 6) as Statics.Unlocks) # temp
	backing_fade.modulate = Color(BACK_FADE_COLOR.r, BACK_FADE_COLOR.g, BACK_FADE_COLOR.b, 0.0)
	_set_stage_init()
	beam_group.visible = false
	text_reward.visible = false
	text_reward.set_default_flashy(2)
	text_reward.enable_rainbow_scroll()
	center_default = center_group.position


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
			center_group.position = center_default + center_offset
			center_offset = center_offset.lerp(Vector2.ZERO, CENTER_JOLY_RETURN * delta)
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
		Stages.FREE:
			backing_fade.modulate.a = move_toward(backing_fade.modulate.a, 0.0, delta)
			if backing_fade.modulate.a == 0.0:
				queue_free()


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
			text_elaboration.set_snaily_text(tr(&"Find it in the main menu!"))
		Statics.Unlocks.CHAR_SEL:
			text_condition.set_snaily_text(tr(&"By beating Boss Rush,"))
			text_reward.set_snaily_text(tr(&"Character Select!!"))
			text_elaboration.set_snaily_text(tr(&"Find it in the new game options!"))
		Statics.Unlocks.ABSURD_DIFF:
			text_condition.set_snaily_text(tr(&"By beating the game in less than 30 minutes,"))
			text_reward.set_snaily_text(tr(&"Absurd Difficulty!!"))
			text_elaboration.set_snaily_text(tr(&"Find it in the new game options!"))
		Statics.Unlocks.ITEM_RANDO:
			text_condition.set_snaily_text(tr(&"By collecting all items,"))
			text_reward.set_snaily_text(tr(&"Randomizer Mode!!"))
			text_elaboration.set_snaily_text(tr(&"Find it in the new game options!"))
		Statics.Unlocks.OPEN_MAP:
			text_condition.set_snaily_text(tr(&"By filling out the map completely,"))
			text_reward.set_snaily_text(tr(&"Open Map!!"))
			text_elaboration.set_snaily_text(tr(&"Find it in the new game options!"))
		Statics.Unlocks.SIX_HUNDO:
			text_condition.set_snaily_text(tr(&"By beating the game with a new character,"))
			text_reward.set_snaily_text(tr(&"600% Mode!!"))
			text_elaboration.set_snaily_text(tr(&"Find it in the new game options!"))
		Statics.Unlocks.CHAOS_MODE:
			text_condition.set_snaily_text(tr(&"By beating the game on Absurd Difficulty,"))
			text_reward.set_snaily_text(tr(&"Chaos Mode!!"))
			text_elaboration.set_snaily_text(tr(&"Find it in the new game options!"))


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


func _input(event: InputEvent) -> void:
	if not read_inputs:
		return
	if (event.is_action_pressed("uiAccept")
	or event.is_action_pressed("uiBack")
	or event.is_action_pressed("uiClick")):
		if stage == Stages.SHAKE:
			_progress_break()
		elif stage == Stages.AWAIT:
			_set_stage_out()


func _progress_break() -> void:
	center_offset = Vector2(CENTER_JOLT_STRENGTH, 0.0).rotated(randf() * TAU)
	center_offset *= 1.0 + (break_progress * 0.25)
	anim.play("break" + str(break_progress))
	sfx_beams[break_progress].play()
	break_progress += 1
	if break_progress == 5:
		read_inputs = false


func _check_next() -> void:
	queue.remove_at(0)
	if queue.is_empty():
		_set_stage_free()
	else:
		_set_stage_init()


#region Beam config
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


func _show_beam(count:int) -> void:
	count = clampi(count, 0, beams.size())
	visible_beams = clampi(visible_beams + count, 0, beams.size())


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
#endregion


#region Stage setters
func _set_stage_init() -> void:
	stage = Stages.INIT
	frame.position.y = FRAME_START_Y_OFFSET
	lock_body.position.y = FRAME_START_Y_OFFSET
	lock_arch.position.y = FRAME_START_Y_OFFSET - LOCK_ARCH_OFFSET.y
	top_text_group.position.y = TEXT_START_Y_OFFSET
	bottom_text_group.position.y = 0.0
	text_reward.visible = false
	text_elaboration.modulate.a = 0.0
	frame.play("locked")
	lock_body.play("default")
	lock_body.flip_h = false
	lock_arch.play("default")
	lock_body.flip_h = false
	_init_beams()
	break_progress = 0


func _set_stage_in() -> void:
	stage = Stages.IN
	_set_text()
	anim.play("init")


func _set_stage_shake() -> void:
	stage = Stages.SHAKE
	frame.position.y = 0.0
	_set_lock_pos(Vector2.ZERO)
	read_inputs = true


func _set_stage_pop() -> void:
	stage = Stages.POP
	_set_frame_reward_anim()
	center_group.position = center_default
	frame.position = Vector2.ZERO
	lock_body.play("fall")
	lock_body_vel = Vector2(
		randf_range(POP_VEL_X.x, POP_VEL_X.y) * (-1.0 if randf() < 0.5 else 1.0),
		randf_range(POP_VEL_Y.x, POP_VEL_Y.y)
	)
	lock_body.flip_h = lock_body_vel.x < 0
	lock_arch.play("fall")
	lock_arch_vel = Vector2(
		randf_range(POP_VEL_X.x, POP_VEL_X.y) * (-1.0 if randf() < 0.5 else 1.0),
		randf_range(POP_VEL_Y.x, POP_VEL_Y.y)
	)
	lock_arch.flip_h = lock_arch_vel.x < 0
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
	advance_prompt.visible = true
	read_inputs = true


func _set_stage_out() -> void:
	stage = Stages.OUT
	advance_prompt.visible = false
	read_inputs = false
	anim.play("out")


func _set_stage_free() -> void:
	stage = Stages.FREE
	finished.emit()
#endregion
