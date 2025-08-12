class_name CutsceneController
extends Node


enum Status {
	OK,
	BUSY,
	FILE_ERROR,
	UNKOWN_ERROR,
}


static var instance:CutsceneController


func _process(delta: float) -> void:
	pass


func _ready() -> void:
	instance = self
	print(name)
	print("!!!!!! READY")


func _sim_step() -> void:
	pass
