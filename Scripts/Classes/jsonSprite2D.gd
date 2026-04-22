@icon("res://Editor/ico/JsonSprite2D.svg")
class_name JsonSprite2D
extends Sprite2D


const _DATA_MATCH = {
	"tiles": [0, 0],
	"layerize": false,
	"meta": {},
	"animations": {
		"an_action": {
			"colors": [0],
			"fps": 30,
			"loop": true,
			"inherit": "name", #optional
			"loop_point": 0, #optional
			"autoplay_next": "an_action", #optional
			"randomize_start": false, #optional
			"frames": [
				#X, Y, hflip, vflip
				[0, 0, false, false],
			]
		},
	}
}


@export var normal_npc := false
@export_file("*.json") var texture_path:String
@export var load_autoplay:Array[String]
@export var fade_color:Color = Color.WHITE:
	set(value):
		fade_color = value
		_fade_color()
@export_range(0, 1, 0.01) var fade_lerp:float = 0:
	set(value):
		fade_lerp = value
		_fade_color()
@export var fps_mult := 1.0
var data:Dictionary
var is_ready := false
var action:String:
	set(value):
		action = value
		_index = 0
		_timer = -2
		_check_action()
var meta:Dictionary = {}


var _timer := 0.0
var _has_action := false
var _recheck := false
var _index:int = 0
var _children:Array[Sprite2D] = []
var _layers:Array[Texture2D] = []


static var _imgcache := {}
static var _datcache := {}


func _check_action():
	if is_ready:
		_has_action = data["animations"].has(action)
		if _has_action:
			if data["animations"][action].has("randomize_start"):
				if data["animations"][action]["randomize_start"]:
					_index = randi_range(0, data["animations"][action]["frames"].size() - 1)
		_recheck = false
		return
	_recheck = true


func _ready() -> void:
	var pathtrimmed := texture_path.get_basename()
	if not texture_path.get_extension().to_lower() == "json":
		assert(false, "Path is not a json file!")
		return


	if normal_npc:
		texture = preload("uid://cp668vus8s3n0")
	elif _imgcache.has(pathtrimmed):
		texture = _imgcache[pathtrimmed]
	else:
		var found := false
		for i:String in [".png", ".svg", ".tga", ".hdr", ".exr", ".ktx", ".dds", ".bmp", ".jpg", ".jpeg", ".webp"]:
			var path := pathtrimmed + i
			if ResourceLoader.exists(path):
				var tex = load(path)
				if tex is Texture:
					_imgcache[pathtrimmed] = tex
					texture = tex
					found = true
					break
		if not found:
			assert(false, "Texture doesn't exist, or it hasn't been imported as a texture!")
			return


	if _datcache.has(pathtrimmed):
		data = _datcache[pathtrimmed]
	else:
		if not FileAccess.file_exists(texture_path):
			assert(false, "Path to json is invalid!")
			return
		var j := JSON.new()
		var f := FileAccess.open(texture_path, FileAccess.READ)
		if j.parse(f.get_as_text()) != OK:
			f.close()
			assert(
				false,
				"JSON parsing failed! Line " + str(j.get_error_line()) +
				", Message: \"" + j.get_error_message() +
				"\", In file " + texture_path
			)
			return
		f.close()
		var tempdata = j.data
		if not tempdata is Dictionary:
			assert(false, "Json data has invalid base type!")
			return
		tempdata = tempdata as Dictionary
		for i in _DATA_MATCH.keys():
			if tempdata.has(i):
				if not typeof(tempdata[i]) == typeof(_DATA_MATCH[i]):
					assert(false, "Key type mismatch: " + i)
					return
			else:
				assert(false, "Missing key: " + i)
				return
		for i in tempdata["animations"].keys():
			var t = tempdata["animations"][i]
			if not (t is Dictionary):
				assert(false, "Animation is incorrect type: " + i)
				return
			t = t as Dictionary
			if t.has("inherit"):
				if not (t["inherit"] is String):
					assert(false, "Incorrect inheritance key type")
					return
				if not (tempdata["animations"].has(t["inherit"])):
					assert(false, "Inherited animation does not exist")
					return
				var ind = tempdata["animations"][t["inherit"]]
				if not (ind is Dictionary):
					assert(false, "Inherited animation is corrupt")
					return
				ind = ind as Dictionary
				t.merge(ind)
			if not t.has_all(["fps", "frames"]):
				assert(false, "Animation is missing important keys")
				return
			for h in t["frames"]:
				if h is Array:
					if h.size() == 4:
						if (Statics.is_number(h[0]) and
							Statics.is_number(h[1]) and
							h[2] is bool and
							h[3] is bool):
								continue
				assert(false, "Invalid frame data!")
				return
			for h in t.keys():
				if _DATA_MATCH["animations"]["an_action"].has(h):
					if typeof(t[h]) == typeof(_DATA_MATCH["animations"]["an_action"][h]):
						continue
					else:
						if (Statics.is_number(t[h]) and
							Statics.is_number(_DATA_MATCH["animations"]["an_action"][h])):
							continue
					assert(false, "Animation data type mismatch: " + h)
					return
				assert(false, "Extra animation data: " + h)
				return
		if tempdata["tiles"].size() != 2:
			assert(false, "tiles is not vector")
			return
		if not (Statics.is_number(tempdata["tiles"][0]) and
			Statics.is_number(tempdata["tiles"][1])):
			assert(false, "tiles is not vector")
			return
		_datcache[pathtrimmed] = tempdata
		data = tempdata

	hframes = data["tiles"][0]
	vframes = data["tiles"][1]
	meta = data["meta"]

	if data["layerize"]:
		var layers:Dictionary[int, Image]
		var img := texture.get_image()
		var sz = img.get_size()
		for x in sz.x:
			for y in sz.y:
				var col := img.get_pixel(x, y)
				if col.a8 == 255:
					var c32 := col.to_rgba32()
					if not (c32 in layers):
						layers[c32] = Image.create_empty(sz.x, sz.y, false, Image.FORMAT_RGBA8)
						layers[c32].fill(Color.TRANSPARENT)
					var l := layers[c32]
					l.set_pixel(x, y, Color.WHITE)
		var j := layers.keys()
		j.sort()
		for i in j:
			_layers.append(ImageTexture.create_from_image(layers[i]))
		for i in _layers.size() - 1:
			var sp := Sprite2D.new()
			sp.texture = _layers[i]
			sp.hframes = hframes
			sp.vframes = vframes
			sp.use_parent_material = true
			add_child(sp)
			_children.append(sp)
		texture = _layers[-1]
	is_ready = true

	if load_autoplay.size() > 0:
		action = load_autoplay.pick_random()


func _fade_color() -> void:
	if _has_action:
		var _action:Dictionary = data["animations"][action]
		if _action.has("colors"):
			for i in _action["colors"].size():
				var col:Color = Color.hex(_action["colors"][i])
				col = col.lerp(fade_color, fade_lerp)
				if i >= _children.size():
					self_modulate = col
				else:
					_children[i].self_modulate = col


func _process(delta: float) -> void:
	if _recheck:
		_check_action()
	_timer += delta * fps_mult
	_fade_color()
	if _has_action:
		var _action:Dictionary = data["animations"][action]
		var _fps:float = _action["fps"]
		var _frames:Array = _action["frames"]
		var _frametime = 1 / _fps
		if (_frames.size() == 0):
			if _action.has("autoplay_next"):
				action = _action["autoplay_next"]
			else:
				action = "__NONE__"
			return
		if (_timer >= _frametime) or (_timer < -1):
			if _timer < -1:
				_timer = 0
			else:
				_timer -= _frametime
			if _index > _frames.size() - 1:
				if _action["loop"]:
					if _action.has("loop_point"):
						_index = int(_action["loop_point"])
					else:
						_index = 0
				elif _action.has("autoplay_next"):
					action = _action["autoplay_next"]
					return
				else:
					action = "__NONE__"
					return
			var _frame = _frames[_index]
			if min(_frame[0], _frame[1]) < 0:
				hide()
			else:
				show()
				frame_coords = Vector2i(_frame[0], _frame[1])
			flip_h = _frame[2]
			flip_v = _frame[3]
			_index += 1
	for i in _children:
		i.frame = frame
		i.flip_h = flip_h
		i.flip_v = flip_v


func create_afterimage(_start_a:float, _end_a:float, _fade_time:float, _z_index:int, _frame_coords:Vector2i = frame_coords) -> void:
	var afterimage:AfterimageSprite = AfterimageSprite.new()
	afterimage.setup(self, _frame_coords, _start_a, _end_a, _fade_time, _z_index)
	Statics.active_room.layer_ground.add_child(afterimage)


func create_afterimage_with_offset(_start_a:float, _end_a:float, _fade_time:float, _z_index:int, _coord_offset:Vector2i) -> void:
	var coords:Vector2i = frame_coords + _coord_offset
	while coords.x < 0:
		coords.x += hframes
	coords.x = coords.x % hframes
	while coords.y < 0:
		coords.y += vframes
	coords.y = coords.y % vframes
	create_afterimage(_start_a, _end_a, _fade_time, _z_index, coords)
