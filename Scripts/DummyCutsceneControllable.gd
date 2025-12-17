@icon("res://Editor/ico/CutsceneControllable.svg")
class_name DummyCutsceneControllable
extends CutsceneControllable


func is_in_top_half_of_screen() -> bool:
	return Input.is_action_pressed(&"debug")


func impulse(_direction:Vector2) -> bool:
	return false


func glide_to(_position:Vector2, _duration:float) -> bool:
	return false


func get_dialogue_icon() -> Texture:
	return null


func fake_input(_event:InputEventAction, _hold_for:float) -> bool:
	return false


func look_at_position(_pos:Vector2) -> bool:
	return false


func look_at_local(_pos:Vector2) -> bool:
	return false


func look_at_node(_node:Node2D) -> bool:
	return false


func disable_ai(_locked:bool) -> bool:
	return false


func has_item(_id:Item.ItemTypes) -> bool:
	return false


func give_item(_id:Item.ItemTypes, _quantity:int) -> bool:
	return false


func take_item(_id:Item.ItemTypes, _quantity:int) -> bool:
	return false


func can_perform_action(_action:String) -> bool:
	return false


func perform_action(_action:String, _force:bool) -> bool:
	return false
