@tool
extends Control


enum {
	BOOL,
	STRING,
	ACTOR,
	INT,
	FLOAT,
	TEXTURE2D,
	VECTOR2,
	NODE2D,
}


const BTYPES = {
	"Actors": [
		["Actor...", ACTOR],
		["Glide to", BOOL],
		["Impulse", BOOL],
		["Say", BOOL],
		["Get Icon", TEXTURE2D],
		["Fake Input", BOOL],
		["Look at Position", BOOL],
		["Look at Local Position", BOOL],
		["Look at Node", BOOL],
		["Lock inputs", BOOL],
		["Can Perform Action", BOOL],
		["Perform Action", BOOL],
	],
	"Tiles": [],
	"Flow": [],
	"Variables": [],
}
const BLOCK:PackedScene = preload("uid://bhvl14npfluwo")


var blocktree := {}
var ep:EditorPlugin


@onready var files:ItemList = $HSplitContainer/ItemList
@onready var blocks:Tree = $HSplitContainer/HSplitContainer/Tree
@onready var program:HBoxContainer = $HSplitContainer/HSplitContainer/HBoxContainer/VBoxContainer/program


func _ready() -> void:
	blocktree["__ROOT__"] = blocks.create_item()
	for i in BTYPES.keys():
		var t := blocks.create_item(blocktree["__ROOT__"])
		blocktree[i] = t
		t.set_text(0, i)
		for j in BTYPES[i]:
			var b := blocks.create_item(t)
			b.set_text(0, j[0])
