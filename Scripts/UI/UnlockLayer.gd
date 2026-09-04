# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name UnlockLayer
extends Node2D


const GRAVITY:float = 40.0
const POP_VEL_X:Vector2 = Vector2(40.0, 80.0)
const POP_VEL_Y:Vector2 = Vector2(20.0, 80.0)
const FRAME_START_Y_OFFSET:float = 240.0
const TEXT_START_Y_OFFSET:float = -64.0
const LOCK_ARCH_OFFSET:Vector2 = Vector2(0.0, -32.0)
const FRAME_SHAKE_MULT:float = 3.0
const BACK_FADE_COLOR:Color = Color("0000007f")

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
var shake_strength:float = 0.0
var lock_body_vel:Vector2 = Vector2.ZERO
var lock_arch_vel:Vector2 = Vector2.ZERO
var lock_arch_extension:float = 0.0

@export var frame:SnailySprite2D
@export var lock_body:SnailySprite2D
@export var lock_arch:SnailySprite2D
@export var backing_fade:Sprite2D
@export var top_text_group:Node2D
@export var text_condition:SnailyText
@export var text_unlocked:SnailyText
@export var text_reward:SnailyText


func _ready() -> void:
	backing_fade.modulate = Color("0000")
	_set_state_init()


func _set_state_init() -> void:
	frame.position.y = FRAME_START_Y_OFFSET
	lock_body.position.y = FRAME_START_Y_OFFSET
	lock_arch.position.y = FRAME_START_Y_OFFSET - LOCK_ARCH_OFFSET.y
