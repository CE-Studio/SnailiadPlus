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


var BTYPES = {
	"Actors": [
		Color.DODGER_BLUE,
		[
			["Actor", ACTOR, [], false, false, actorsel],
			["Actor...", ACTOR, [STRING], false, false, null],
			["Glide to", BOOL, [VECTOR2], false, false, null],
			["Impulse", BOOL, [VECTOR2], false, false, null],
			["Say", BOOL, [TEXTURE2D, STRING], false, false, null],
			["Get Icon", TEXTURE2D, [ACTOR], false, false, null],
			["Fake Input", BOOL, [STRING], false, false, null],
			["Look at Position", BOOL, [VECTOR2], false, false, null],
			["Look at Local Position", BOOL, [VECTOR2], false, false, null],
			["Look at Node", BOOL, [NODE2D], false, false, null],
			["Lock inputs", BOOL, [BOOL], false, false, null],
			["Can Perform Action", BOOL, [ACTOR], false, false, null],
			["Perform Action", BOOL, [ACTOR], false, false, null],
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
			["If", BOOL, [BOOL], true, false, null],
			["If", BOOL, [BOOL], true, true, null],
		],
	],
	"Data": [
		Color.DARK_ORANGE,
		[
			
		],
	],
}
const BLOCK:PackedScene = preload("uid://bhvl14npfluwo")
const CATE:PackedScene = preload("uid://j3ldjrsghpyr")


var blocks:Array[ProgramBlock] = []
var ep:EditorPlugin


@onready var files:ItemList = $HSplitContainer/ItemList
@onready var program:VBoxContainer = $HSplitContainer/HSplitContainer/HBoxContainer/VBoxContainer/program
@onready var holdpoint:Control = $held
@onready var pallete:VBoxContainer = $HSplitContainer/HSplitContainer/PanelContainer/ScrollContainer/VBoxContainer


func actorsel(block:ProgramBlock, arr:Array) -> void:
	block.opt.show()
	block.opt.clear()
	block.opt.add_item("player")
	for i:CutsceneControllable in arr:
		block.opt.add_item(i.identifier)


func scene_changed(root:Node) -> void:
	for i in blocks:
		i.reset()
	if root is Room:
		var ents:Array[CutsceneControllable] = root.get_actors()
		for i in blocks:
			i.scr_setup(ents)


func _ready() -> void:
	ep.scene_changed.connect(scene_changed)
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
			blocks.append(blk)
