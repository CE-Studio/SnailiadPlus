class_name AngryBlock
extends Enemy


@onready var face_spr:JsonSprite2D = $"Face"


func _ready() -> void:
	my_type = EnemyTypes.ANGRYBLOCK
	hitbox = $"Area2D"
	sprite = $"Body"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active and not display_mode:
		return
	
