class_name Fire
extends Hazard


var base_dmg:int


func _ready() -> void:
	base_dmg = damage
	super()


func _physics_process(_delta: float) -> void:
	if Statics.has_shell(1):
		damage = ceili(float(base_dmg) * 0.25)
	else:
		damage = base_dmg
	super(_delta)
