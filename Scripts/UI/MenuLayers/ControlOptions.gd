extends VBoxContainer


var panel_up:bool = false

@onready var layer:MenuLayer = get_parent()
@onready var panel:ContextPanel = $"../ContextPanel"
@onready var default_button:SnailyButton = $"Default"
@onready var sfx_default:AudioStreamPlayer = $"../AudioGroup/Defaults"


func _ready() -> void:
	panel.modulate.a = 0
	panel.call_deferred("set_text",
		"menu_option_controls_default_confirm", 1)
	panel.call_deferred("add_button",
		Statics.get_text("menu_add_generic_no"), _on_panel_no, false)
	panel.call_deferred("add_button",
		Statics.get_text("menu_add_generic_yes"), _on_panel_yes, false)


func _process(delta: float) -> void:
	if panel_up:
		panel.modulate.a = lerpf(panel.modulate.a, 1.0, MenuLayer.MOVE_RATE * delta)
		panel.position.y = lerp(panel.position.y, 80.0, MenuLayer.MOVE_RATE * delta)
		layer.menu.selector_y_offset = 80.0 - panel.position.y
	else:
		panel.modulate.a = lerpf(panel.modulate.a, 0.0, MenuLayer.MOVE_RATE * delta)
		panel.position.y = lerp(panel.position.y, 240.0, MenuLayer.MOVE_RATE * delta)
		layer.menu.selector_y_offset = 0


func on_reset_defaults(_value) -> void:
	_focus_panel()


func _on_panel_no(_value) -> void:
	_defocus_panel()


func _on_panel_yes(_value) -> void:
	ProjectSettings.set_setting("game/control/controls", SInput.DEFAULTS.duplicate())
	layer.menu.save_general()
	sfx_default.play()
	_defocus_panel()


func _focus_panel() -> void:
	layer.can_focus = false
	layer.menu.read_inputs = false
	panel_up = true
	panel.can_focus = true
	panel.focus_button(0)
	layer.menu.selector_y_offset = 80.0 - panel.position.y


func _defocus_panel() -> void:
	panel_up = false
	panel.can_focus = false
	layer.can_focus = true
	layer.menu.set_deferred("read_inputs", true)
	default_button.grab_focus()
