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
		Color.DODGER_BLUE,
		[
			["Actor...", ACTOR, [STRING], false, false],
			["Glide to", BOOL, [VECTOR2], false, false],
			["Impulse", BOOL, [VECTOR2], false, false],
			["Say", BOOL, [TEXTURE2D, STRING], false, false],
			["Get Icon", TEXTURE2D, [ACTOR], false, false],
			["Fake Input", BOOL, [STRING], false, false],
			["Look at Position", BOOL, [VECTOR2], false, false],
			["Look at Local Position", BOOL, [VECTOR2], false, false],
			["Look at Node", BOOL, [NODE2D], false, false],
			["Lock inputs", BOOL, [BOOL], false, false],
			["Can Perform Action", BOOL, [ACTOR], false, false],
			["Perform Action", BOOL, [ACTOR], false, false],
		],
	],
	"Tiles": [
		Color.BLUE_VIOLET,
		[
			
		],
	],
	"Flow": [
		Color.GOLDENROD,
		[
			
		],
	],
	"Variables": [
		Color.DARK_ORANGE,
		[
			
		],
	],
}
const BLOCK:PackedScene = preload("uid://bhvl14npfluwo")
const CATE:PackedScene = preload("uid://j3ldjrsghpyr")


var blocktree := {}
var ep:EditorPlugin


@onready var files:ItemList = $HSplitContainer/ItemList
@onready var program:VBoxContainer = $HSplitContainer/HSplitContainer/HBoxContainer/VBoxContainer/program
@onready var holdpoint:Control = $held
@onready var pallete:VBoxContainer = $HSplitContainer/HSplitContainer/PanelContainer/ScrollContainer/VBoxContainer


func _ready() -> void:
	for i in BTYPES:
		var item = BTYPES[i]
		var cate:Cate = CATE.instantiate()
		cate.setname(i, item[0])
		pallete.add_child(cate)
		for j in item[1]:
			var blk:ProgramBlock = BLOCK.instantiate()
			blk.simpsetup(j)
			blk.modulate = item[0]
			cate.add_item(blk)
