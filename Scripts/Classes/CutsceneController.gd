class_name CutsceneController
extends Node


enum Status {
	OK,
	BUSY,
	FILE_ERROR,
	UNKOWN_ERROR,
}


static var instance:CutsceneController


func _process(_delta: float) -> void:
	pass


func _ready() -> void:
	instance = self


func _sim_step() -> void:
	pass
