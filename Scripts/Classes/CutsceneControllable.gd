@icon("res://Editor/ico/CutsceneControllable.svg")
@abstract class_name CutsceneControllable
extends Node2D


## Used to identify objects in a cutscene. Must be unique for every object currently active.
@export var identifier:StringName


static var actors:Array[CutsceneControllable] = []


func _ready():
	var nr:Array[CutsceneControllable] = []
	for i in actors:
		if is_instance_valid(i):
			nr.append(i)
	actors = nr
	for i in actors:
		assert(not i.identifier == identifier, str(get_path()) + ": Identifier conflict! (" + identifier + ")")
	actors.append(self)


func is_in_top_half_of_screen() -> bool:
	return Input.is_action_pressed(&"debug")


## Apply a force to the object. Returns true if force was applied.
@abstract func impulse(_direction:Vector2) -> bool


## Make the object go to a position. Returns true if if the object will go there.
@abstract func glide_to(_position:Vector2, _duration:float) -> bool


## Returns the image used to represent this object in dailouge boxes.
@abstract func get_dialogue_icon() -> Texture


## Simulates an input, as if the player was controlling this object. Returns true if the object will accept the input and try to act on it.[br]
## A negative hold value means the fake input will not automatically be released. The object sould wait for another input to know when to stop.
@abstract func fake_input(_event:InputEventAction, _hold_for:float) -> bool


## Makes the object look at a global position. Returns true if the object will look there.
@abstract func look_at_position(_pos:Vector2) -> bool


## Makes the object look at a local position. Returns true if the object will look there.
@abstract func look_at_local(_pos:Vector2) -> bool


## Makes the object look at a Node2D. Returns true if the object will look at it.
@abstract func look_at_node(_node:Node2D) -> bool


## Stops the object from accepting player inputs. Fake inputs should still be accepted. Returns true if locking or unlocking succeeded.
@abstract func lock_inputs(_locked:bool) -> bool


## Returns true of the object has the specified item
@abstract func has_item(_id:Item.ItemTypes) -> bool


## Returns true if the object can currently perform the diesired action, such as "fire_boomerang", or "gravity_up"
@abstract func can_perform_action(_action:String) -> bool


## Performs an action, such as "fire_boomerang", or "gravity_up". If force is true, it should happen regardless of if the object should be able to.
@abstract func perform_action(_action:String, _force:bool) -> bool
