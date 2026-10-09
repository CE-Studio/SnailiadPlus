# Copyright 2026 CE-Studio: AGPL-3.0-only
class_name RushManager
extends Node


var bullets:Array[int] = []
var damage:int = 0
var shields:int = 0
var parries:int = 0
var health_lost:int = 0
var health_gained:int = 0
var boss_time:Array[float] = []

static var instance:RushManager


func _ready() -> void:
	instance = self


func reset() -> void:
	bullets.clear()
	damage = 0
	shields = 0
	parries = 0
	health_lost = 0
	health_gained = 0
	boss_time.clear()


func add_bullet(id:int) -> void:
	while bullets.size() <= id:
		bullets.append(0)
	bullets[id] += 1


func count_up_boss_time(id:int, delta:float) -> void:
	while boss_time.size() <= id:
		boss_time.append(0.0)
	boss_time[id] += delta
