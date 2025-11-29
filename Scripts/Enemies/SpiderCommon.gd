class_name SpiderCommon
extends Enemy


#region Variables
const SPEED:float = 45.0
const MOVE_TIMEOUT:float = 4.0
const WAIT_TABLE:Array[float] = [
	0.0031435364, 0.5049282581, 0.4116654365, 0.5281877909, 0.5897768281, 0.1593536691, 0.4039032663, 0.4059745561, 0.0582864048, 0.7516076167,
	0.5521423097, 0.5566189571, 0.179637362,  0.3594656996, 0.4505799066, 0.3204984303, 0.6052265986, 0.6324895486, 0.4429925385, 0.8837227036,
	0.9752844916, 0.6930966018, 0.1752373743, 0.3443669975, 0.4504892571, 0.5988067483, 0.4101707911, 0.2408469776, 0.5234045926, 0.0236403878,
	0.6238985334, 0.6958397015, 0.270161708,  0.621824504,  0.9523979626, 0.234787262,  0.3882103268, 0.2584722478, 0.8966220748, 0.6939320403,
	0.2789706634, 0.0615652706, 0.9490876411, 0.3085988573, 0.4638467981, 0.6217814413, 0.7608899529, 0.4005289448, 0.1308170265, 0.4694031184,
	0.893697234,  0.0653774173, 0.7188370915, 0.6329837926, 0.1353358423, 0.2568268114, 0.8794799448, 0.0097632309, 0.6912924617, 0.7049703039,
	0.9255215652, 0.9475764837, 0.8637740622, 0.1508763211, 0.3508634319, 0.5790814897, 0.7811909111, 0.7848816088, 0.5664273614, 0.0356774301,
	0.3960516107, 0.6490766209, 0.947092078,  0.5093130463, 0.1259762285, 0.6807200132, 0.5178459596, 0.6560252702, 0.5672414089, 0.890198107,
	0.230094122,  0.642458186,  0.3116517752, 0.7473356059, 0.4938040994, 0.0920139665, 0.4272894969, 0.3406688463, 0.8804234661, 0.1334528937,
	0.9890385972, 0.9199415992, 0.0593612118, 0.5765849264, 0.7873382929, 0.827340683,  0.1771514581, 0.7038809678, 0.5615721056, 0.6086805593
]

var move_timeout:float = 0.0
var move_index:int = 0
static var global_move_index:int = 0
#endregion


func _ready() -> void:
	my_type = EnemyTypes.SPIDER_COMMON
	hitbox = $"Area2D"
	sprite = $"JsonSprite2D"
	vis = $"VisibleOnScreenNotifier2D"
	super.spawn()
	
	global_move_index = (global_move_index + 1) % WAIT_TABLE.size()
	move_index = global_move_index
	move_timeout = WAIT_TABLE[move_index] * MOVE_TIMEOUT


func _physics_process(delta: float) -> void:
	super(delta)
	if not ai_active:
		return
	
	if vis.is_on_screen():
		move_timeout -= delta
		if move_timeout <= 0.0:
			move_index = (move_index + 1) % WAIT_TABLE.size()
			move_timeout = WAIT_TABLE[move_index] * MOVE_TIMEOUT
			var angle:float = atan2(
				GameCore.instance.player.position.y - position.y,
				GameCore.instance.player.position.x - position.x
			)
			velocity = Vector2(cos(angle), sin(angle))
			_update_anim()
			velocity *= SPEED
		position += velocity * delta


func _update_anim() -> void:
	var anim_name = ""
	if velocity.y < -0.3827:
		anim_name += "U"
	elif velocity.y > 0.3827:
		anim_name += "D"
	if velocity.x < -0.3827:
		anim_name += "L"
	elif velocity.x > 0.3827:
		anim_name += "R"
	sprite.action = anim_name
