class_name SpaceboxShield
extends Enemy


func _ready() -> void:
	my_type = EnemyTypes.NONE
	col = $"BodyBox"
	sprite = $"Body"
	hitbox = $"Area2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	sprite.action = "idle"
