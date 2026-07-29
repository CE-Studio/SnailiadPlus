extends Node2D



@export var new_colors: Array[Color];
# which row gets replaced ie player is always row 2
@export var id: int = 2;
# the new one we make
@export var pallette_tex: CompressedTexture2D;
@export var mat : ShaderMaterial;
var master_tex: ImageTexture;


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var img = pallette_tex.get_image()
	master_tex = ImageTexture.create_from_image(img)
	if mat:
		mat.set_shader_parameter("palettes", master_tex)
	
	
	replace_colors(new_colors)


func replace_colors(custom_colors: Array[Color]) -> void:
	var img = master_tex.get_image()
	for i in range(img.get_width()):
		img.set_pixel(i, id, custom_colors[i])
	master_tex.update(img)
	
