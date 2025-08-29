extends VBoxContainer


#region Variables
const ICON_RADII:Vector2i = Vector2i(300, 80)
const THETA_EASE_RATE:float = 10.0

var achievement_str_names:Array = AchievementCore.Achievements.keys()
var achievement_count:int = achievement_str_names.size()
var theta_add_value:float = 0.0
var icons:Array = []

var theta:float = 0.0
var target_theta:float = 0.0
var selected_achievement:int = 0
var desc_mode:int = 0

@onready var main_scroller:HeaderlessScrollingSnailyButton = $"MainScroller"
@onready var counter_text:SnailyText = $"Counter/Text"
@onready var hint_text:SnailyText = $"HintGuide/Text"
@onready var desc_text:SnailyText = $"Description/Text"
@onready var icon_group:Node2D = $"../IconGroup"
@onready var icon_scene:PackedScene = preload("res://Scenes/UI/AchievementIcon.tscn")
#endregion


func _ready() -> void:
	counter_text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	var counter_raw := counter_text.text
	counter_text.set_snaily_text(counter_raw % [ 0, achievement_count ])
	var camel_array:Array = []
	for _name in achievement_str_names:
		if _name is String:
			camel_array.append(_name.to_camel_case())
	achievement_str_names = camel_array.duplicate()
	
	var new_option_array:Array[String] = []
	for i in range(achievement_count):
		var new_text := "menu_option_achievements_locked_title"
		if Statics.check_achievement(i):
			new_text = "menu_option_achievements_%s_title" % achievement_str_names[i]
		new_option_array.append(new_text)
	main_scroller.remote_import_new_options(new_option_array)
	
	hint_text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	
	desc_text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_TOP)
	
	theta_add_value = TAU / float(achievement_count)
	for i in range(achievement_count):
		var new_icon:AchievementIcon = icon_scene.instantiate()
		icon_group.add_child(new_icon)
		icons.append(new_icon)
		new_icon.action = "selected" if i == 0 else "idle"
		if Statics.check_achievement(i):
			new_icon.icon.action = achievement_str_names[i]
		else:
			new_icon.icon.action = "locked"


func _process(delta: float) -> void:
	theta = lerpf(theta, target_theta, THETA_EASE_RATE * delta)
	for i in range(icons.size()):
		var icon:AchievementIcon = icons[i]
		var this_theta := theta - (theta_add_value * i)
		icon.position = Vector2(
			-sin(this_theta) * ICON_RADII.x,
			cos(this_theta) * ICON_RADII.y
		)
		icon.visible = icon.position.y > 0


func _on_cycle_left(value: Variant) -> void:
	target_theta -= theta_add_value
	icons[selected_achievement].action = "idle"
	selected_achievement = value
	icons[selected_achievement].action = "selected"
	_update_desc(2 if Statics.check_achievement(value) else 0)
	main_scroller.sfx_focus.play()


func _on_cycle_right(value: Variant) -> void:
	target_theta += theta_add_value
	icons[selected_achievement].action = "idle"
	selected_achievement = value
	icons[selected_achievement].action = "selected"
	_update_desc(2 if Statics.check_achievement(value) else 0)
	main_scroller.sfx_focus.play()


func _on_scroller_pressed(_value: Variant) -> void:
	_update_desc(desc_mode + 1)
	main_scroller.sfx_select.play()


func _update_desc(mode:int) -> void:
	desc_mode = mode
	var new_desc := "menu_option_achievements_%s_%s"
	match desc_mode:
		0:
			new_desc = new_desc % [ "locked", "desc" ]
		1:
			new_desc = new_desc % [ achievement_str_names[selected_achievement], "hint" ]
		_:
			new_desc = new_desc % [ achievement_str_names[selected_achievement], "desc" ]
	desc_text.set_snaily_text(Statics.get_text(new_desc))
