extends Node2D


const COL_CYCLE_TIME:float = 0.1
#const A_CYCLE_MOD:float = 2.25

#var cycle_a:float = 0.0
var cycle_col:float = 0.0
var ptr_col:int = 0


func _process(delta: float) -> void:
	cycle_col += delta
	while cycle_col >= COL_CYCLE_TIME:
		cycle_col -= COL_CYCLE_TIME
		ptr_col = (ptr_col + 1) % 4
		match ptr_col:
			0: modulate = Statics.get_color(Vector2i(0, 1))
			1: modulate = Statics.get_color(Vector2i(3, 3))
			2: modulate = Statics.get_color(Vector2i(3, 13))
			3: modulate = Statics.get_color(Vector2i(3, 7))
	
	#cycle_a += delta * A_CYCLE_MOD
	#while cycle_a >= 1.0:
	#	cycle_a -= 1.0
	#modulate.a = roundf(cycle_a)
