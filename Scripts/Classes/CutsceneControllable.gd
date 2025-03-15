@icon("res://Editor/ico/CutsceneControllable.svg")
class_name CutsceneControllable
extends Node2D


## Used to identify objects in a cutscene. Must be unique for every object currently active.
@export var identifier:StringName


static var actors:Array[CutsceneControllable] = []


func _ready():
	if OS.get_cmdline_args().has("debug"):
		impulse(Vector2.ZERO)
		glide_to(position, 0)
		get_dialogue_icon()
		fake_input(InputEventAction.new(), 0)
		look_at_position(global_position)
		look_at_local(Vector2.ZERO)
		look_at_node(self)
		lock_inputs(false)
		has_item(Statics.Items.NONE)
		can_perform_action("")
		perform_action("", false)
	while actors.has(null):
		actors.erase(null)
	for i in actors:
		assert(not i.identifier == identifier, str(get_path()) + ": Identifier conflict! (" + identifier + ")")
	actors.append(self)


func _process(_delta):
	pass


## Apply a force to the object. Returns true if force was applied.
func impulse(_direction:Vector2) -> bool:
	assert(false, str(get_path()) + ": Required func 'impulse(vector2) -> bool' not defined!")
	return false


## Make the object go to a position. Returns true if if the object will go there.
func glide_to(_position:Vector2, _duration:float) -> bool:
	assert(false, str(get_path()) + ": Required func 'glide_to(Vector2, float) -> bool' not defined!")
	return false


## Returns the image used to represent this object in dailouge boxes.
func get_dialogue_icon() -> Texture:
	assert(false, str(get_path()) + ": Required func 'get_dialogue_icon() -> Texture' not defined!")
	return null


## Simulates an input, as if the player was controlling this object. Returns true if the object will accept the input and try to act on it.[br]
## A negative hold value means the fake input will not automatically be released. The object sould wait for another input to know when to stop.
func fake_input(_event:InputEventAction, _hold_for:float) -> bool:
	assert(false, str(get_path()) + ": Required func 'fake_input(InputEventAction, float) -> bool' not defined!")
	return false


## Makes the object look at a global position. Returns true if the object will look there.
func look_at_position(_pos:Vector2) -> bool:
	assert(false, str(get_path()) + ": Required func 'look_at_position(Vector2) -> bool' not defined!")
	return false


## Makes the object look at a local position. Returns true if the object will look there.
func look_at_local(_pos:Vector2) -> bool:
	assert(false, str(get_path()) + ": Required func 'look_at_local(Vector2) -> bool' not defined!")
	return false


## Makes the object look at a Node2D. Returns true if the object will look at it.
func look_at_node(_node:Node2D) -> bool:
	assert(false, str(get_path()) + ": Required func 'look_at_node(Node2D) -> bool' not defined!")
	return false


## Stops the object from accepting player inputs. Fake inputs should still be accepted. Returns true if locking or unlocking succeeded.
func lock_inputs(_locked:bool) -> bool:
	assert(false, str(get_path()) + ": Required func 'lock_inputs(bool) -> bool' not defined!")
	return false


## Returns true of the object has the specified item
func has_item(_id:Statics.Items) -> bool:
	assert(false, str(get_path()) + ": Required func 'has_item(Statics.Items) -> bool' not defined!")
	return false


## Returns true if the object can currently perform the diesired action, such as "fire_boomerang", or "gravity_up"
func can_perform_action(_action:String) -> bool:
	assert(false, str(get_path()) + ": Required func 'can_perform_action(String, bool) -> bool' not defined!")
	return false


## Performs an action, such as "fire_boomerang", or "gravity_up". If force is true, it should happen regardless of if the object should be able to.
func perform_action(_action:String, _force:bool) -> bool:
	assert(false, str(get_path()) + ": Required func 'perform_action(String, bool) -> bool' not defined!")
	return false
