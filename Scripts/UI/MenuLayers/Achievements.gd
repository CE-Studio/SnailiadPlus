extends VBoxContainer


#region Variables
const ICON_RADII:Vector2i = Vector2i(254, 80)
const THETA_EASE_RATE:float = 10.0

var tr_descs:Array = [
	[tr(&"First of Four"), tr(&"Seek the Spherical Slinger of the Returning Shot"), tr(&"Defeat Shellbreaker")],
	[tr(&"Stinky Toe"), tr(&"Follow the footsteps"), tr(&"Defat Stompy")],
	[tr(&"Gravity Battle"), tr(&"Deal a crushing blow to this Quadrilateral Quake-maker"), tr(&"Defeat Space Box")],
	[tr(&"Victory"), tr(&"Bring an end to your quest"), tr(&"Defeat Moon Snail and beat the game")],
	[tr(&"Glass Cannon"), tr(&"Defense is merely a suggestion"), tr(&"Defeat Moon Snail without collecting Full Metal Snail")],
	[tr(&"Explorer"), tr(&"So much to do, so much to see"), tr(&"Find 100% of the map")],
	[tr(&"Happy Ending"), tr(&"Like flicking a light switch"), tr(&"Return Sun Snail's light")],
	[tr(&"Treasure Hunter"), tr(&"It's like your shell doubles as a backpack!"), tr(&"Find 100% of all items")],
	[tr(&"Homeless"), tr(&"Conquer with the armorless one"), tr(&"Beat the game as Sluggy Slug")],
	[tr(&"Top Floor"), tr(&"Conquer with the inverted one"), tr(&"Beat the game as Upside Snail")],
	[tr(&"Mansion"), tr(&"Conquer with the appendaged one"), tr(&"Beat the game as Leggy Snail")],
	[tr(&"Just Renting"), tr(&"Conquer with the odd one out"), tr(&"Beat the game as Blobby Blob")],
	[tr(&"Attic Dweller"), tr(&"Conquer with the hungry one"), tr(&"Beat the game as Leechy Leech")],
	[tr(&"Speedrunner"), tr(&"Snails don't normally go that fast, do they?"), tr(&"Beat the game in under 30 minutes")],
	[tr(&"The Gauntlet"), tr(&"Looks like they want a rematch"), tr(&"Beat the Boss Rush")],
	[tr(&"Pilgrim"), tr(&"All truths revealed... a mere two screens to the left"), tr(&"Find the Shrine of Iris")],
	[tr(&"Snelk Hunter A"), tr(&"They're hiding... Somewhere pretty square"), tr(&"Find the first Secret Snelk room")],
	[tr(&"Snelk Hunter B"), tr(&"They're still hiding... Somewhere pretty hot"), tr(&"Find the second Secret Snelk room")],
	[tr(&"Super Secret"), tr(&"A speedrunner's best friend"), tr(&"Find the Super Secret Boomerang")],
	[tr(&"Counter-Snail"), tr(&"Where it all began"), tr(&"Find the Remake's original test rooms")],
	[tr(&"Biologist"), tr(&"They're fun, once you get to know them"), tr(&"Unlock every entry in the bestiary")],
	[tr(&"Where are we, Snaily?"), tr(&"Where it all REALLY began"), tr(&"Find the original test map")],
	[tr(&"Omega Snail"), tr(&"This... this is just ludicrous"), tr(&"Beat the game on Absurd difficulty")],
	[tr(&"How did you get up here?"), tr(&"Procedurally-generated replayability"), tr(&"Beat a randomized seed")],
	[tr(&"Lost and Found"), tr(&"Everything deserves a second chance, shocking as it may be"), tr(&"Discover the long-forgotten Gravity Shock item")],
]

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
	var earned_count:int = Statics.get_achievement_count()
	counter_text.set_snaily_text(counter_raw % [ earned_count, achievement_count ])
	var camel_array:Array = []
	for _name in achievement_str_names:
		if _name is String:
			camel_array.append(_name.to_camel_case())
	achievement_str_names = camel_array.duplicate()
	
	var new_option_array:Array[String] = []
	for i in range(achievement_count):
		var new_text:String = tr(&"Locked")
		if Statics.check_achievement(i):
			new_text = tr_descs[i][0]
			#new_text = "menu_option_achievements_%s_title" % achievement_str_names[i]
		new_option_array.append(new_text)
	main_scroller.remote_import_new_options(new_option_array)
	
	hint_text.set_alignment(HORIZONTAL_ALIGNMENT_CENTER, VERTICAL_ALIGNMENT_CENTER)
	
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
	
	_update_desc(2 if Statics.check_achievement(0) else 0)


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
	if desc_mode == 0:
		_update_desc(desc_mode + 1)
	main_scroller.sfx_select.play()


func _update_desc(mode:int) -> void:
	desc_mode = mode
	#var new_desc := "menu_option_achievements_%s_%s"
	#match desc_mode:
	#	0:
	#		new_desc = new_desc % [ "locked", "desc" ]
	#	1:
	#		new_desc = new_desc % [ achievement_str_names[selected_achievement], "hint" ]
	#	_:
	#		new_desc = new_desc % [ achievement_str_names[selected_achievement], "desc" ]
	#desc_text.set_snaily_text(Statics.get_text(new_desc))
	if mode == 0:
		desc_text.set_snaily_text_raw(&"You haven't unlocked this achievement yet")
	else:
		desc_text.set_snaily_text_raw(tr_descs[selected_achievement][mode])
