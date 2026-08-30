# Copyright 2026 CE-Studio: AGPL-3.0-only
extends DummyCutsceneControllable


func can_perform_action(_action:String) -> bool:
	return _action == "start_fade"


func perform_action(_action:String, _force:bool) -> bool:
	if _action == "start_fade":
		var fade:EndingFade = load("uid://crgwv7a4ws03t").instantiate()
		fade.do_giga_fadein = false
		fade.quick_fade = true
		GameCore.instance.add_child(fade)
	return false
