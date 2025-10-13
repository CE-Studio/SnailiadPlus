extends JsonSprite2D


var player:Player


func _ready() -> void:
	if not GameCore.instance:
		queue_free()
		return
	assert(GameCore.instance.player, "No Player set in GameCore! A Player must exist for the PlayerSilhouette to work.")
	player = GameCore.instance.player
	texture_path = player.sprite.texture_path
	super()


func _process(delta: float) -> void:
	if not GameCore.instance:
		return
	var player_anim:String = player.sprite.action
	if action != player_anim:
		action = player_anim
	super(delta)
	global_position = player.global_position
