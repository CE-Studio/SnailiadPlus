@tool
class_name Cutscene
extends Resource

@export var name:String = "":
	set(value):
		name = value
		resource_name = value


@export_storage var program:Array
