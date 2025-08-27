class_name ShellbreakerHand
extends Enemy


func _ready() -> void:
	my_type = EnemyTypes.SHELLBREAKER
	col = $"BodyBox"
	sprite = $"Body"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	sprite.action = "idle"
