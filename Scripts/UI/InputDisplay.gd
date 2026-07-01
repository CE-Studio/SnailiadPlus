# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name InputDisplay
extends Node


@export var left:SnailySprite2D
@export var down:SnailySprite2D
@export var up:SnailySprite2D
@export var right:SnailySprite2D
@export var jump:SnailySprite2D
@export var shoot:SnailySprite2D
@export var strafe:SnailySprite2D
@export var gravity:SnailySprite2D


func _process(_delta: float) -> void:
	if not self.visible:
		return
	
	if SInput.check_input(SInput.Inputs.LEFT, false):
		if left.animation == "default": left.play("press")
	elif left.animation == "press": left.play("default")
	
	if SInput.check_input(SInput.Inputs.DOWN, false):
		if down.animation == "default": down.play("press")
	elif down.animation == "press": down.play("default")
	
	if SInput.check_input(SInput.Inputs.UP, false):
		if up.animation == "default": up.play("press")
	elif up.animation == "press": up.play("default")
	
	if SInput.check_input(SInput.Inputs.RIGHT, false):
		if right.animation == "default": right.play("press")
	elif right.animation == "press": right.play("default")
	
	if SInput.check_input(SInput.Inputs.JUMP, false):
		if jump.animation == "default": jump.play("press")
	elif jump.animation == "press": jump.play("default")
	
	if SInput.check_input(SInput.Inputs.SHOOT, false):
		if shoot.animation == "default": shoot.play("press")
	elif shoot.animation == "press": shoot.play("default")
	
	if SInput.check_input(SInput.Inputs.STRAFE, false):
		if strafe.animation == "default": strafe.play("press")
	elif strafe.animation == "press": strafe.play("default")
	
	if SInput.check_input(SInput.Inputs.GRAVITY, false):
		if gravity.animation == "default": gravity.play("press")
	elif gravity.animation == "press": gravity.play("default")
