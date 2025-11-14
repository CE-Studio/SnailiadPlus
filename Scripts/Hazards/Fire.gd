class_name Fire
extends Hazard


var base_dmg:int = damage


func _ready() -> void:
	super()


func _physics_process(_delta: float) -> void:
	if Statics.has_shell(1):
		damage = roundi(base_dmg * 0.25)
	else:
		damage = base_dmg
	super(_delta)
