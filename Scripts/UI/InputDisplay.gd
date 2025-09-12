class_name InputDisplay
extends Node


@onready var left:JsonSprite2D = $"Left"
@onready var down:JsonSprite2D = $"Down"
@onready var up:JsonSprite2D = $"Up"
@onready var right:JsonSprite2D = $"Right"
@onready var jump:JsonSprite2D = $"Jump"
@onready var shoot:JsonSprite2D = $"Shoot"
@onready var strafe:JsonSprite2D = $"Strafe"
@onready var gravity:JsonSprite2D = $"Gravity"


func _process(_delta: float) -> void:
	if not self.visible:
		return
	
	if SInput.check_input(SInput.Inputs.LEFT, false):
		if left.action != "left_press":
			left.action = "left_press"
	else:
		if left.action != "left_idle":
			left.action = "left_idle"
	if SInput.check_input(SInput.Inputs.DOWN, false):
		if down.action != "down_press":
			down.action = "down_press"
	else:
		if down.action != "down_idle":
			down.action = "down_idle"
	if SInput.check_input(SInput.Inputs.UP, false):
		if up.action != "up_press":
			up.action = "up_press"
	else:
		if up.action != "up_idle":
			up.action = "up_idle"
	if SInput.check_input(SInput.Inputs.RIGHT, false):
		if right.action != "right_press":
			right.action = "right_press"
	else:
		if right.action != "right_idle":
			right.action = "right_idle"
	if SInput.check_input(SInput.Inputs.JUMP, false):
		if jump.action != "jump_press":
			jump.action = "jump_press"
	else:
		if jump.action != "jump_idle":
			jump.action = "jump_idle"
	if SInput.check_input(SInput.Inputs.SHOOT, false):
		if shoot.action != "shoot_press":
			shoot.action = "shoot_press"
	else:
		if shoot.action != "shoot_idle":
			shoot.action = "shoot_idle"
	if SInput.check_input(SInput.Inputs.STRAFE, false):
		if strafe.action != "strafe_press":
			strafe.action = "strafe_press"
	else:
		if strafe.action != "strafe_idle":
			strafe.action = "strafe_idle"
	if SInput.check_input(SInput.Inputs.GRAVITY, false):
		if gravity.action != "gravity_press":
			gravity.action = "gravity_press"
	else:
		if gravity.action != "gravity_idle":
			gravity.action = "gravity_idle"
