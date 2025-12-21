@icon("res://Editor/ico/Interactable.svg")
class_name Grass
extends Node2D


#region Variables
enum GrassTypes {
	NORMAL,
	POWER
}

var type:GrassTypes
var surface:Statics.DirsSurface
var anim = ""
var player_intersecting:bool = false
var heal_amount:int
var nom_cooldown:float
var bite_count:int
var can_regrow:bool
var running_bite_count:int
var current_nom_cooldown:float
@onready var sprite:JsonSprite2D = $"JsonSprite2D"
@onready var box:CollisionShape2D = $"Area2D/CollisionShape2D"
@onready var sfx_nom:AudioStreamPlayer = $"AudioGroup/Nom"
@onready var sfx_grow:AudioStreamPlayer = $"AudioGroup/Grow"
@onready var timer:Timer = $"RegrowTimer"
@onready var vis:VisibleOnScreenNotifier2D = $"VisibleOnScreenNotifier2D"
#endregion


func spawn(grass_type:GrassTypes, home_surface:Statics.DirsSurface):
	type = grass_type
	var difficulty_id = Statics.current_profile["difficulty"]
	match grass_type:
		GrassTypes.POWER:
			anim = "power_"
			heal_amount = [6, 3, 3][difficulty_id]
			nom_cooldown = 0.17
			bite_count = 12
			can_regrow = false
			sfx_nom.stream = load("res://Assets/Sounds/Sfx/EatPowerGrass.ogg")
		_:
			anim = "normal_"
			heal_amount = 1
			nom_cooldown = 0.22
			bite_count = [6, 3, 1][difficulty_id]
			can_regrow = true
			timer.wait_time = 16.0
			sfx_nom.stream = load("res://Assets/Sounds/Sfx/EatGrass.ogg")
	running_bite_count = bite_count
	surface = home_surface
	match home_surface:
		Statics.DirsSurface.CEILING: anim += "ceiling_"
		_: anim += "ground_"
	sprite.action = anim + "idle"


func _process(delta: float) -> void:
	if player_intersecting and running_bite_count > 0 and not CutsceneController.running:
		if current_nom_cooldown <= 0.0:
			GameCore.instance.player.adjust_health(heal_amount)
			current_nom_cooldown = nom_cooldown
			sfx_nom.play()
			Statics.spawn_particle("Nom", Room.Layers.GROUND, position + Vector2(0, -8))
			running_bite_count -= 1
			if running_bite_count == 0:
				sprite.action = anim + "eaten"
				if can_regrow:
					timer.start()
	current_nom_cooldown -= delta
	if current_nom_cooldown < 0.0:
		current_nom_cooldown = 0.0


func _on_player_entered(_body: Node2D) -> void:
	player_intersecting = true


func _on_player_exited(_body: Node2D) -> void:
	player_intersecting = false


func _on_regrow_timer_timeout() -> void:
	running_bite_count = bite_count
	sprite.action = anim + "regrow"
	if vis.is_on_screen():
		sfx_grow.play()
