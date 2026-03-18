extends Node2D


#region Variables
const CELL_SIZE:Vector2i = Vector2i(16, 16)
const SELECTOR_OFFSET:Vector2i = Vector2i(8, 8)

var selected_cell:int = 0
var selection_depth:int = 0
var buffer_frames:int = 4

@export var grid_size:Vector2i = Vector2i(10, 5)
@export var sprites:Array[Sprite2D] = []
@export var selector:JsonSprite2D
#endregion


func _ready() -> void:
	position = Statics.VECTOR_CENTER - (Vector2(CELL_SIZE * grid_size) * 0.5)


func _process(_delta: float) -> void:
	if ((SInput.check_input(SInput.Inputs.PAUSE, true) or SInput.check_input(SInput.Inputs.DEBUG, true))
	and selection_depth == 0 and buffer_frames <= 0):
		UICore.instance.pause_layer.unpause_fade_out()
		queue_free()
	
	var intended_dir:Vector2 = Vector2(
		int(SInput.check_input(SInput.Inputs.RIGHT, true)) - int(SInput.check_input(SInput.Inputs.LEFT, true)),
		int(SInput.check_input(SInput.Inputs.DOWN, true)) - int(SInput.check_input(SInput.Inputs.UP, true))
	)
	if intended_dir != Vector2.ZERO:
		var current_row:int = floori(selected_cell / grid_size.x)
		selected_cell += roundi(intended_dir.x + (intended_dir.y * grid_size.x))
		var new_row:int = floori(selected_cell / grid_size.x)
		if selected_cell < 0:
			new_row = -1
		if intended_dir.y == 0:
			if new_row < current_row:
				selected_cell += grid_size.x
			elif new_row > current_row:
				selected_cell -= grid_size.x
		while selected_cell < 0:
			selected_cell += sprites.size()
		while selected_cell >= sprites.size():
			selected_cell -= sprites.size()
		selector.position = Vector2i(
			selected_cell % grid_size.x,
			floori(selected_cell / grid_size.x)
		) * CELL_SIZE + SELECTOR_OFFSET
	
	
	if buffer_frames > 0:
		buffer_frames -= 1
