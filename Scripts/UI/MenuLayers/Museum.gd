extends VBoxContainer


func _ready() -> void:
	if not Statics.has_unlock(Statics.Unlocks.BOSS_RUSH):
		$"Times".quick_load_layer = ""
		$"Times".set_text("-- ??? --")
		$"Gallery".quick_load_layer = ""
		$"Gallery".set_text("-- ??? --")
		$"SoundTest".quick_load_layer = ""
		$"SoundTest".set_text("-- ??? --")
