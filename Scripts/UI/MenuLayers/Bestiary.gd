extends Sprite2D


#region Variables
const NAME_STR:String = "entity_%s_name"
const DESC_STR:String = "entity_%s_desc"
const ENEMY_PATH:String = "res://Scenes/Entities/Enemies/%s.tscn"
const BOSS_PATH:String = "res://Scenes/Entities/Enemies/Bosses/%s.tscn"
const FOCUS_COLOR_SPEED:float = 4.5
const SELECTOR_WIGGLE_SPEED:float = 3.8
const SELECTOR_EASE_RATE:float = 20.0
const SELECTOR_Y_MARGIN:int = 20
const ENTRIES:Array = [
	Enemy.EnemyTypes.SPIKEY_COMMON,
	Enemy.EnemyTypes.SPIKEY_TOUGH,
	#Enemy.EnemyTypes.SPIKEY_ABSURD,
	Enemy.EnemyTypes.BABYFISH,
	Enemy.EnemyTypes.FLOATSPIKE,
	Enemy.EnemyTypes.BLOB_COMMON,
	Enemy.EnemyTypes.BLOB_TOUGH,
	Enemy.EnemyTypes.BLOB_ANGEL,
	Enemy.EnemyTypes.BLOB_DEVIL,
	Enemy.EnemyTypes.CHIRPY,
	Enemy.EnemyTypes.BATTYBAT,
	Enemy.EnemyTypes.FIREBALL,
	Enemy.EnemyTypes.ICEBALL,
	Enemy.EnemyTypes.GHOSTBALL,
	Enemy.EnemyTypes.SNELK,
	Enemy.EnemyTypes.KITTY,
	Enemy.EnemyTypes.CANON,
	Enemy.EnemyTypes.NONCANON,
	#Enemy.EnemyTypes.FANON,
	Enemy.EnemyTypes.ANGRYBLOCK,
	Enemy.EnemyTypes.SNAKEY,
	Enemy.EnemyTypes.SKYVIPER,
	Enemy.EnemyTypes.SPIDER_COMMON,
	Enemy.EnemyTypes.SPIDER_TOUGH,
	Enemy.EnemyTypes.TURTLE_COMMON,
	Enemy.EnemyTypes.TURTLE_TOUGH,
	Enemy.EnemyTypes.JELLYFISH,
	Enemy.EnemyTypes.SEAHORSE,
	Enemy.EnemyTypes.TALLFISH_COMMON,
	Enemy.EnemyTypes.TALLFISH_TOUGH,
	Enemy.EnemyTypes.WALLEYE,
	Enemy.EnemyTypes.PINCER_FLOOR,
	#Enemy.EnemyTypes.PINCER_WALL,
	Enemy.EnemyTypes.PINCER_CEILING,
	Enemy.EnemyTypes.GEAR_COMMON,
	#Enemy.EnemyTypes.GEAR_TOUGH,
	Enemy.EnemyTypes.DRONE,
	Enemy.EnemyTypes.BALLOON,
	Enemy.EnemyTypes.SHELLBREAKER,
	Enemy.EnemyTypes.STOMPY,
	Enemy.EnemyTypes.SPACEBOX,
	Enemy.EnemyTypes.SPACEBOX_BABYBOX,
	Enemy.EnemyTypes.MOONSNAIL,
	Enemy.EnemyTypes.GIGASNAIL,
	#Enemy.EnemyTypes.COSMICSNAIL,
	#Enemy.EnemyTypes.SHELLBREAKER_RUSH,
	#Enemy.EnemyTypes.STOMPY_RUSH,
	#Enemy.EnemyTypes.SPACEBOX_RUSH,
	#Enemy.EnemyTypes.MOONSNAIL_RUSH,
	#Enemy.EnemyTypes.GIGASNAIL_RUSH,
]

var parent_layer:MenuLayer
var text_color:Color = Statics.get_color(Vector2i(0, 4))
var list_items:Array[SnailyText] = []
var entity_list:Array[String] = []
var entry_states:Array[bool] = []
var elapsed:float = 0.0
var focused_text:SnailyText = null
var selector_origin_x:float = 0.0
var selection:int = 0


@onready var text_scene:PackedScene = preload("res://Scenes/internals/SnailyText.tscn")
@onready var scroll_list:VBoxContainer = $"ScrollPanel/EntityList"
@onready var sfx_beep:AudioStreamPlayer = $"Beep"
@onready var selector:Node2D = $"Selector"
@onready var name_text:SnailyText = $"Name"
@onready var desc_text:SnailyText = $"Description"
@onready var display_enemies:Array[Enemy] = []
@onready var enemy_spawn:Node2D = $"EntityOrigin"
#endregion


func _ready() -> void:
	parent_layer = get_parent()

	var enemy_enums := Enemy.EnemyTypes.keys()
	for enemy in ENTRIES:
		entity_list.append(enemy_enums[enemy].to_camel_case())

	for i in range(entity_list.size()):
		var entity := entity_list[i]
		if not Statics.check_bestiary_entry(ENTRIES[i]):
			entity = "none"
			entry_states.append(false)
		else:
			entry_states.append(true)
		var new_text:SnailyText = text_scene.instantiate()
		new_text.text_scale = 1
		new_text.max_width = 112
		new_text.name = str(i)
		new_text.modulate = text_color
		new_text.set_snaily_text(_get_name(entity))
		scroll_list.add_child(new_text)
		list_items.append(new_text)
		#region Set focus
		new_text.focus_mode = Control.FOCUS_ALL
		var this_focus = "../" + str(i)
		var last_focus = "../" + str(i - 1)
		var next_focus = "../" + str(i + 1)
		if i == 0:
			last_focus = "../" + str(entity_list.size() - 1)
		if i == entity_list.size() - 1:
			next_focus = "../" + str(0)
		new_text.focus_neighbor_left = this_focus
		new_text.focus_neighbor_right = this_focus
		new_text.focus_neighbor_bottom = next_focus
		new_text.focus_next = next_focus
		new_text.focus_neighbor_top = last_focus
		new_text.focus_previous = last_focus
		#endregion
		new_text.focus_entered.connect(_on_text_focused)

	if list_items.size() > 0:
		list_items[0].grab_focus()

	selector_origin_x = selector.position.x

	name_text.modulate = text_color
	desc_text.modulate = text_color
	_update_entry_display()


func _process(delta: float) -> void:
	elapsed += delta
	for text in list_items:
		if text == focused_text:
			var elapsed_mod:float = abs(sin(elapsed * FOCUS_COLOR_SPEED))
			text.modulate = text_color.lerp(Color.WHITE, elapsed_mod)
		else:
			text.modulate = text_color

	selector.position.x = selector_origin_x + sin(elapsed * SELECTOR_WIGGLE_SPEED)
	selector.global_position.y = clampf(
		lerpf(
			selector.global_position.y,
			focused_text.global_position.y + 5,
			SELECTOR_EASE_RATE * delta
		),
		parent_layer.global_position.y + SELECTOR_Y_MARGIN,
		parent_layer.global_position.y + 240 - SELECTOR_Y_MARGIN
	)


func _on_text_focused() -> void:
	focused_text = get_viewport().gui_get_focus_owner()
	sfx_beep.play()
	selection = int(focused_text.name)
	_update_entry_display()


func _update_entry_display() -> void:
	if display_enemies.size() > 0:
		for enemy in display_enemies:
			enemy.queue_free()
		display_enemies.clear()
	var has_entry := entry_states[selection]
	var this_entity := entity_list[selection] if has_entry else "none"
	name_text.set_snaily_text(_get_name(this_entity))
	desc_text.set_snaily_text(_get_desc(this_entity))
	if has_entry:
		spawn_display_entity(this_entity)


func spawn_display_entity(entity:String) -> void:
	match entity:
		"babyfish":
			var fish1 = spawn_entity("babyfish1")
			enemy_spawn.add_child(fish1)
			fish1.position = Vector2.LEFT * 12
			var fish2 = spawn_entity("babyfish2")
			enemy_spawn.add_child(fish2)
			fish2.position = Vector2.RIGHT * 12
		"floatspike":
			var floatspike1 = spawn_entity("floatspike_common")
			enemy_spawn.add_child(floatspike1)
			floatspike1.position = Vector2.LEFT * 16
			var floatspike2 = spawn_entity("floatspike_tough")
			enemy_spawn.add_child(floatspike2)
			floatspike2.position = Vector2.RIGHT * 16
		"chirpy":
			var chirpy1 = spawn_entity("chirpy_common")
			enemy_spawn.add_child(chirpy1)
			chirpy1.position = Vector2.LEFT * 16
			var chirpy2 = spawn_entity("chirpy_tough")
			enemy_spawn.add_child(chirpy2)
			chirpy2.position = Vector2.RIGHT * 16
		"snakey":
			var snakey1 = spawn_entity("snakey_common")
			enemy_spawn.add_child(snakey1)
			snakey1.position = Vector2.LEFT * 16
			var snakey2 = spawn_entity("snakey_tough")
			enemy_spawn.add_child(snakey2)
			snakey2.position = Vector2.RIGHT * 16
		"kitty":
			var kitty2 = spawn_entity("kitty_tough")
			enemy_spawn.add_child(kitty2)
		"canon":
			var canon = spawn_entity("canon")
			enemy_spawn.add_child(canon)
			canon.position = Vector2(8, 8)
		"noncanon":
			var noncanon = spawn_entity("noncanon")
			enemy_spawn.add_child(noncanon)
			noncanon.position = Vector2(-8, 8)
		"pincerFloor":
			var pincer = spawn_entity("pincer")
			pincer.direction = Statics.DirsSurface.FLOOR
			enemy_spawn.add_child(pincer)
		"pincerWall":
			var pincer_l = spawn_entity("pincer")
			pincer_l.direction = Statics.DirsSurface.LWALL
			pincer_l.position.x -= 20.0
			enemy_spawn.add_child(pincer_l)
			var pincer_r = spawn_entity("pincer")
			pincer_r.direction = Statics.DirsSurface.RWALL
			pincer_r.position.x += 20.0
			enemy_spawn.add_child(pincer_r)
		"pincerCeiling":
			var pincer = spawn_entity("pincer")
			pincer.direction = Statics.DirsSurface.CEILING
			enemy_spawn.add_child(pincer)
		"stompy":
			var stompy = spawn_entity("stompy")
			enemy_spawn.add_child(stompy)
			stompy.position = Vector2(48, 0)
			stompy.foot_r.visible = false
			stompy.eye_r.visible = false
		_:
			var general_enemy = spawn_entity(entity)
			enemy_spawn.add_child(general_enemy)
			general_enemy.position = Vector2.ZERO


func spawn_entity(entity:String) -> Enemy:
	entity = entity.to_pascal_case()
	var new_enemy:Enemy = null
	if ResourceLoader.exists(BOSS_PATH % entity):
		new_enemy = load(BOSS_PATH % entity).instantiate()
	else:
		new_enemy = load(ENEMY_PATH % entity).instantiate()
	new_enemy.display_mode = true
	display_enemies.append(new_enemy)
	new_enemy.z_index = -5
	return new_enemy


func _get_name(key:String) -> StringName:
	match key:
		"spikeyCommon": return tr(&"Spikey (blue)")
		"spikeyTough": return tr(&"Spikey (orange)")
		"spikeyAbsurd": return tr(&"Spikey (pink)")
		"babyfish": return tr(&"Babyfish")
		"floatspike": return tr(&"Floatspike")
		"blobCommon": return tr(&"Blob")
		"blobTough": return tr(&"Blub")
		"blobAngel": return tr(&"Angelblob")
		"blobDevil": return tr(&"Devilblob")
		"chirpy": return tr(&"Chirpy")
		"battybat": return tr(&"Batty Bat")
		"fireball": return tr(&"Fireball")
		"iceball": return tr(&"Iceball")
		"ghostball": return tr(&"Ghost Dandelion")
		"snelk": return tr(&"Secret Snelk")
		"kitty": return tr(&"Kitty!!")
		"canon": return tr(&"Canon")
		"noncanon": return tr(&"Non-canon")
		"snakey": return tr(&"Snakey")
		"skyviper": return tr(&"Sky Viper")
		"spiderCommon": return tr(&"Spider")
		"spiderTough": return tr(&"Spider Mama")
		"turtleCommon": return tr(&"Gravity Turtle")
		"turtleTough": return tr(&"Gravity Turtle (red)")
		"jellyfish": return tr(&"Jellyfish")
		"seahorse": return tr(&"Syngnathida")
		"tallfishCommon": return tr(&"Tallfish")
		"tallfishTough": return tr(&"Angry Tallfish")
		"walleye": return tr(&"Walleye")
		"pincerFloor": return tr(&"Pincer")
		"pincerWall": return tr(&"Pouncer")
		"pincerCeiling": return tr(&"Sky Pincer")
		"gearCommon": return tr(&"Spinnygear")
		"gearTough": return tr(&"Speedygear")
		"drone": return tr(&"Federation Drone")
		"balloon": return tr(&"Balloon Buster")
		"shellbreaker": return tr(&"Shellbreaker")
		"stompy": return tr(&"Stompy")
		"spacebox": return tr(&"Space Box")
		"spaceboxBabybox": return tr(&"Babybox")
		"moonsnail": return tr(&"Moon Snail")
		"gigasnail": return tr(&"Giga Snail")
		"none": return tr(&"-- ??? --")
	return tr(&"Unrecognized")


func _get_desc(key:String) -> StringName:
	match key:
		"spikeyCommon": return tr(&"Habitat: diverse (M.Carelia)\nDemeanour: reserved\nTexture: prickly\n\nThese small creatures wander the surfaces of their homes, looking for plants to eat.  They don't like to be bothered much, so they've developed spiky shells to discourage hugs.")
		"spikeyTough": return tr(&"Habitat: diverse\nShell: shiny\nAccuracy: scarily precise\n\nWhatever mutation causes these spikeys' shells to become orange also makes their stomachs super sensitive, causing them to not agree with the pea plants they eat.  They don't mean to attack you; you're just in the way.")
		"spikeyAbsurd": return tr(&"")
		"babyfish": return tr(&"Habitat: M.Carelia\nAge: baby\nLikes: swimming, you\n\nTiny passive sealife that swim through the water without a care in the world.  Babyfish are fairly social creatures, and will take an interest in any passersby.  They usually migrate elsewhere as they grow.")
		"floatspike": return tr(&"Habitat: diverse\nHead: empty\nLife: easy\n\nThese simple urchins tend to gravitate toward areas either full of water or otherwise verdant and damp, finding a spot to sit idly for a long time.  The way they gather has led to theories of unknown floatspike societies, with little evidence.")
		"blobCommon": return tr(&"Habitat: diverse\nTexture: slimy, pliable\nTaste: would not recommend\n\nThe blob is a humble creature.  Being made purely of slime, its malleable surface can be bent and reshaped to its whim.  Their appearance around where snails tend to reside makes one wonder if they rose from residual snail trails.")
		"blobTough": return tr(&"Habitat: diverse\nSenses: heightened\nDiet: unknown\n\nBlobs have an impressive ability to adapt to their environments as they age.  Generally, this tends to involve a strengthening of surface tension and a change in pigmentation.  Blub also loves listening to music.")
		"blobAngel": return tr(&"Habitat: L.Lirata\nPalette: shining\nEnergy: overflowing\n\nThanks to high altitudes, sun exposure, proximity to holy figures, or any combination of the three, some blobs can become embued with the attributes of light and sky.  They're not entirely sure what to do with these powers.")
		"blobDevil": return tr(&"Habitat: S.Silere\nTemperature: toasty\nTemper: short\n\nBe it social isolation or simply proximity to fire, some blobs can develop into formidable beasts that are warm and tough to the touch and quick to aggravate.  If you can calm one down, however, they're surprisingly empathetic listeners.")
		"chirpy": return tr(&"Habitat: diverse (M.Carelia)\nPlumage: well-groomed\nEnergy: endless\n\nChirpies never stay in the same place for long, always flying from one area to another and never quite settling in.  Some say they're adventurous by nature, always looking for new and interesting places to fly...")
		"battybat": return tr(&"Habitat: diverse\nDemeanour: skittish\nEnergy: low\n\nBatty Bat doesn't want to cause harm.  It's just that when you sneak up on them while they're sleeping, and they startle as easily as they do, panic sets in fast.  Batty Bat's favorite cereal flavor is chocolate.")
		"fireball": return tr(&"Habitat: diverse (A.Abyssus)\nFeeling: fired up\nTemperature: toasty\n\nA wandering nature spirit attuned to the element of heat.  Despite being a living ball of flame, they don't produce any smoke, and hardly ever ignite things around them.  They'd make perfect snuggle buddies, if only they weren't so darn hot!")
		"iceball": return tr(&"Habitat: diverse (S.Silere)\nLife: chill\nTexture: misty\n\nA wandering nature spirit attuned to the element of cold.  A cloud of icy air surrounds them closely, making it difficult to see where they're going, but they don't seem to mind.  Believe it or not, not a fan of ice puns; they've heard 'em all before.")
		"ghostball": return tr(&"Habitat: diverse (S.Silere)\nFeeling: unbothered, happy\nWeight: lighter than air\n\nSome spirits are naturally lazier than others, and choose to embody calmer elements.  This one has become one with the wind, letting the breeze carry it wherever it desires.  It's hard to tell, but ghost dandelion might be asleep.")
		"snelk": return tr(&"Habitat: unknown\nVibe: mysterious\nFur: soft, snuggly\n\nSecret Snelks are a wondrous enigma.  They tend to hide away in and around spots most people don't know about, which makes even seeing one a rarity.  Some say they'd make great companions, if only you could get them to stop jumping.")
		"kitty": return tr(&"Habitat: S.Silere\nAge: 8 years old\nPersonal space: huge\n\nKitty is territorial by nature, laying claim to all that it sees.  Strange, then, that all it seems to do in its zone when unbothered is sleep.  Some kitties like to gather in small groups and share the space.")
		"canon": return tr(&"Habitat: diverse (L.Lirata)\nFinish: shiny\nHinges: freshly oiled\n\nThis heavy-duty machine is driven by a rudimentary machine-learning algorithm that receives positive feedback whenever it hits its mark. Unfortunately, the machine isn't quite precise enough to keep up with the algorithm's enthusiasm.")
		"noncanon": return tr(&"Habitat: L.Lirata\nBarrel: wider\nIntelligence: slightly higher\n\nAn alternate model of the humble Canon, fit with a version 2 algorithm and refined barrel build for faster warm-up and firing.  Unfortunately, these modifications have made its shell thinner, and thus much more susceptible to damage.")
		"snakey": return tr(&"Habitat: diverse (A.Abyssus)\nSkin: fresh\nFlexibility: incredible\n\nA born explorer, Snakey likes to wind its way into places it really shouldn't be.  It collects little trinkets and oddities to hoard or resell later.  It loves to break out in dance when it hears music, too--an activity it finds slightly embarrassing.")
		"skyviper": return tr(&"Habitat: L.Lirata\nWings: leathery\nBody: aerodynamic\n\nIt's been long debated what Sky Viper is, biologically.  Some say it's a Snakey variant that evolved wings to better traverse the fortress, but others argue it's a hybrid proving the existence of dragons.  ...or it's a bat hybrid, I dunno.")
		"spiderCommon": return tr(&"Habitat: A.Abyssus\nLimbs: plentiful\nSocial life: active\n\nSpiders like to keep close with one another, and are incredibly coordinated.  They do almost everything as a group--even things they don't need to be a team for.  One of their favorite group activities is throwing raves.")
		"spiderTough": return tr(&"Habitat: A.Abyssus\nSkills: slightly better\nAuthority: respected\n\nEvery good team needs a leader!  In spite of their ability to operate fine without one, groups of spiders often have a leader to guide the masses.  Their rule tends to be very lax, though, and they rarely ever need to issue big commands.")
		"turtleCommon": return tr(&"Habitat: A.Abyssus\nShell: tough\nTop speed: 32 px/sec\n\nTurtles, oddly, are born with an innate control over their own gravity.  They claim it's a boon from the turtle god, who walks laps around the sky to keep it moving.  Fitting, then, that they often use this power as their own mode of transport.")
		"turtleTough": return tr(&"Habitat: A.Abyssus\nFinish: shiny\nTop speed: 33 px/sec\n\nIt's not uncommon for turtles to choose a rather colorful method of self-expression, the most common method being eye-catching full-body coloration.  The upkeep is time-consuming, of course, but you have to admit the results are immaculate!")
		"jellyfish": return tr(&"Habitat: A.Abyssus\nThoughts: none\nTexture: slimy, squishy\n\nIn spite of the difference in home terrain, Jellyfish tout a few similarities to Blobs.  Their bodies are similar in composition, and their brain capacity is fairly equal, though devoted to different things (notably: swimming, and... more swimming).")
		"seahorse": return tr(&"Habitat: A.Abyssus\nForm: graceful\nAnd: knows it\n\nSyngnathida faces a conflict in life.  It's a beauty to watch as it glides through the water, and it loves the attention, but it's also instinctively territorial.  Trying to find a careful balance has proven to be a difficult but worthwhile life goal.")
		"tallfishCommon": return tr(&"Habitat: A.Abyssus\nAge: a lot older\nFeeling: a little irked\n\nA fully-grown sea creature that has migrated to an environment more suited to fit it.  Tallfish enjoy open seas, where they can swim freely with little risk of encountering other species (as it turns out, tallfish don't socialize well).")
		"tallfishTough": return tr(&"Habitat: A.Abyssus\nAnger: unmanaged\nEnergy: surprisingly high\n\nAs if their poor social skills weren't enough, something's got this tallfish upset.  It's not clear what--maybe some other fish bothered it a bit too much?  Either way, this one needs some time and space to cool off.  Best not to get too close.")
		"walleye": return tr(&"Habitat: diverse\nEyesight: really good\nReaction time: incredible\n\nWhat is a walleye?  Is it a volatile crystalline mass?  A creature in the shape of an eye as a sort of camouflage?  An actual eye embedded in the wall?  Nobody can get close enough long enough to investigate.")
		"pincerFloor": return tr(&"Habitat: L.Lirata\nEnergy: boundless\nSense of direction: none\n\nIt's unclear whether Pincer is a heavily-plated beetle or a small drone on legs.  Any attempts to approach and figure this conundrum out have been met with multiple bites.  At least we know they like to dance, but they don't slow down a bit doing so.")
		"pincerWall": return tr(&"")
		"pincerCeiling": return tr(&"Habitat: L.Lirata\nDifferences: hardly any\nGravity: what's that?\n\nAn odd thing about the Pincer is its strange knack for defying gravity, in spite of any clear lack of turtle lineage.  They won't tell us how they do it.")
		"gearCommon": return tr(&"Habitat: L.Lirata\nTeeth: 12\nTop speed: 240 RPM\n\nOnce a part of something greater, the spinnygear now aimlessly patrols the fortress it calls home.  It's unclear how intelligent they are, but they seem to take great joy in pouncing from the shadows. Not much else to do when you're a gear.")
		"gearTough": return tr(&"")
		"drone": return tr(&"Habitat: L.Lirata\nOrigin: unknown\n# in reserve: who knows\n\nA mass-produced protector of the fortress, it continues to tick on, opening fire on those not welcome.  When its shift is over, it returns home to recharge and scroll niche online forums while other drones take its place.")
		"balloon": return tr(&"Habitat: L.Lirata\nSkin: surprisingly durable\nDestination: uncertain\n\nSome species of insect have the odd ability to inflate their abdomens to a comical degree and float away on the wind.  They don't seem to have anywhere specific in mind, however, choosing instead to go where the breeze takes them.")
		"shellbreaker": return tr(&"Habitat: M.Carelia\nFeeling: a little grumpy\nAmmo supply: infinite\n\nAs a blob ages, funny things happen to its body.  Shellbreaker's once-fragile surface has hardened into a tough carapace, and they've gained a high level of control over their projectiles.  Their favorite music genre is drum & bass.")
		"stompy": return tr(&"Habitat: S.Silere\nSmell: not great\nBody: unknown\n\nA menace to snails and biologists alike, this odd creature (pair of creatures?) guards its turf with an iron toe, making a ton of noise in the process.  It's a wonder the floors haven't cracked!")
		"spacebox": return tr(&"Habitat: A.Abyssus\nShell: hard metal\nForce: unstoppable\n\nA construct of unknown origin, Space Box finds great joy in ramming itself into surfaces until they're perfectly flat.  As a result, it seems to have all but forgotten its role as a fortress guard.  It still guards, but only for its own amusement.")
		"spaceboxBabybox": return tr(&"Habitat: A.Abyssus\nAge: 6 seconds\nIntentions: unknowable\n\nA small satellite drone emitted by Space Box to aid its guard duty.  It has limited intelligence if any, and seems to aimlessly drift from wall to wall without purpose.  Even still, Space Box harbors an emotional attachment to it.")
		"moonsnail": return tr(&"Habitat: L.Lirata\nStrength: formidable\nMotive: unknown\n\nOnce the absolute peak of mortal snaildom, something happened in Moon Snail's psyche to twist his once-holy powers into a force of malice.  The only way to reverse such a change would be to subdue him, though few can stand to pose a challenge.")
		"gigasnail": return tr(&"Habitat: L.Lirata\nPower: heightened, unstable\nRage: building rapidly")
		"none": return tr(&"\n\nThis entry has not been discovered yet")
	return tr(&"This slot is missing an entry.  Either something was misspelled or it simply does not exist.")
