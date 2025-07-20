class_name Babyfish1
extends Enemy


#region Variables
const MOVE_TIMEOUT:float = 1.3
const SPEED:float = 60.0
const DECEL:float = SPEED * 0.6

var elapsed:float = 0.0
var move_timeout:float = 0.0
var facing_left:bool = false
#endregion

func _ready() -> void:
	my_type = EnemyTypes.BABYFISH_1
	col = $"BodyBox"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	move_timeout = MOVE_TIMEOUT * 0.125
	facing_left = randf() < 0.5
	play_anim("idle")


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	elapsed += delta
	position.y = origin.y + 4 * sin(elapsed * 2)
	if vis.is_on_screen():
		move_timeout -= delta
		if move_timeout <= 0:
			facing_left = GameCore.instance.player.position.x < position.x
			if randf() > 0.8:
				facing_left = not facing_left
			velocity.x = SPEED * (-1 if facing_left else 1)
			move_timeout = MOVE_TIMEOUT
			play_anim("swim")
		move_and_slide()
		if is_on_wall():
			velocity *= -1
		#velocity.x = lerpf(velocity.x, 0, DECEL * delta)
		velocity.x -= DECEL * delta * (-1 if velocity.x < 0 else 1)


func play_anim(modifier:String) -> void:
	var anim_name = "1_" + modifier
	anim_name += "_left" if facing_left else "_right"
	sprite.action = anim_name
