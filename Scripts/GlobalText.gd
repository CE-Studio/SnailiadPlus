extends Node


const COL_CTRL:String = "[color=#ffd48c]"

var version_types:Array[StringName] = [
	tr(&"Dev"),
	tr(&"Demo"),
	tr(&"Release")
]

var characters:Array = [
	[tr(&"Snaily"), tr(&"Snaily Snail"),],
	[tr(&"Sluggy"), tr(&"Sluggy Slug"),],
	[tr(&"Upside"), tr(&"Upside Snail"),],
	[tr(&"Leggy"), tr(&"Leggy Snail"),],
	[tr(&"Blobby"), tr(&"Blobby Blob"),],
	[tr(&"Leechy"), tr(&"Leechy Leech"),],
]

var species:Array = [
	[tr(&"snail"), tr(&"snails"),],
	[tr(&"slug"), tr(&"slugs"),],
	[tr(&"snail"), tr(&"snails"),],
	[tr(&"snail"), tr(&"snails"),],
	[tr(&"blob"), tr(&"blobs"),],
	[tr(&"leech"), tr(&"leeches"),],
]

var areas:Dictionary = {
	"SnailTown": tr(&"Snail Town"),
	"MareCarelia": tr(&"Mare Carelia"),
	"SpiralisSilere": tr(&"Spiralis Silere"),
	"AmastridaAbyssus": tr(&"Amastrida Abyssus"),
	"LuxLirata": tr(&"Lux Lirata"),
	"Iris": tr(&"Shrine of Iris"),
	"BossRush": tr(&"Boss Rush")
}

var room_names:Dictionary = {
	"SnailTown/TownMain": tr(&"Snail Town Plaza"),
	"SnailTown/EntranceSave": tr(&"Town Save Tunnel"),
	"SnailTown/RunnerCave": tr(&"Runner's Path"),
	"SnailTown/CaveRight1": tr(&"Tree Tunnel"),
	"SnailTown/CaveRight2": tr(&"Suspicious Tree"),
	"SnailTown/Paths": tr(&"Parallel Paths"),
	"SnailTown/CaveLeft1": tr(&"Brick Wall"),
	"SnailTown/CaveLeft2": tr(&"Into the Fire"),
	"SnailTown/CaveLeft3": tr(&"Digging Grounds"),
	"SnailTown/CaveLeft4": tr(&"Into the Flames"),
	"SnailTown/CaveDown1": tr(&"Cave Snail's Cave"),
	"SnailTown/CaveDown2": tr(&"Root Cave"),
	"SnailTown/CaveDown3": tr(&"Fragment Cave"),
	"SnailTown/LoveAlcove": tr(&"Love Snail's Alcove"),
	"SnailTown/GreenSecret1": tr(&"Anger Management Room"),
	"SnailTown/GreenSecret2": tr(&"Percentage Snail's Hidey Hole"),
	"SnailTown/SuperSecret": tr(&"Super Secret Alcove"),
	"SnailTown/CaveUp1": tr(&"Leggy Snail's Tunnel"),
	"SnailTown/CaveUp2": tr(&"Town Overtunnel"),
	"SnailTown/CaveUp3": tr(&"Runner's Cutacross"),
	"SnailTown/CaveUp4": tr(&"Upper Town Entrance"),
	"SnailTown/Angle": tr(&"Angle Path, Town-Side"),
	"SnailTown/TestRoom1": tr(&"Original Testing Room"),
	"SnailTown/TestRoom2": tr(&"Testing Plaza"),
	"SnailTown/TestRoom3": tr(&"Testing Theater"),
	"MareCarelia/Entrance": tr(&"Quest's Start Lake"),
	"MareCarelia/EntranceSecret": tr(&"Confusion Corner"),
	"MareCarelia/Tower1": tr(&"Brineshore"),
	"MareCarelia/Hub1": tr(&"Seabed Caves"),
	"MareCarelia/Peashooter": tr(&"Fine Dining"),
	"MareCarelia/CornerTransition": tr(&"Roadblock ahead"),
	"MareCarelia/MazeRoom": tr(&"The Maze Room"),
	"MareCarelia/BlockSnail": tr(&"Monument of Greatness"),
	"MareCarelia/BigLake": tr(&"Spring Lake"),
	"MareCarelia/Tower2": tr(&"Frost Tower"),
	"MareCarelia/Cave1": tr(&"Heart of the Sea"),
	"MareCarelia/Cave2": tr(&"Daily helping of calcium"),
	"MareCarelia/Hub2": tr(&"Rocky Peaks"),
	"MareCarelia/Cave3": tr(&"Dig, Snaily, Dig"),
	"MareCarelia/SnowSave": tr(&"Chilly Checkpoint"),
	"MareCarelia/IceHall": tr(&"Frozen River"),
	"MareCarelia/Tower3": tr(&"Upper Brineshore"),
	"MareCarelia/Cave4": tr(&"Skywatcher's Cave"),
	"MareCarelia/Cave5": tr(&"Skywatcher's Tunnel"),
	"MareCarelia/Cave6": tr(&"Skywatcher's Loot"),
	"MareCarelia/Hub3": tr(&"Dirt Diode"),
	"MareCarelia/Angle": tr(&"Angle Path, Carelia-Side"),
	"MareCarelia/BossHub": tr(&"The Summit"),
	"MareCarelia/Shellbreaker": tr(&"Hi I'm Boss #1"),
	"MareCarelia/Boomerang": tr(&"Signature Croissants"),
	"MareCarelia/TowerHub": tr(&"Transition Tower"),
	"MareCarelia/TransitionSave": tr(&"Transition Checkpoint"),
	"SpiralisSilere/Entrance": tr(&"Tiled Entryway"),
	"SpiralisSilere/Hub1": tr(&"Pitfall Plaza"),
	"SpiralisSilere/Hub1_ALT": tr(&"Unassuming Plaza"),
	"SpiralisSilere/TowerHub": tr(&"Perpendicular Path"),
	"SpiralisSilere/ConnectionVert": tr(&"Could Use An Elevator"),
	"SpiralisSilere/BlockTower": tr(&"Tile Tower"),
	"SpiralisSilere/Snelk": tr(&"Squared Snelks"),
	"SpiralisSilere/Kitty": tr(&"The Catcave"),
	"SpiralisSilere/Shortcut": tr(&"The Vents"),
	"SpiralisSilere/IcePath": tr(&"Chilly Descent"),
	"SpiralisSilere/IceSnailEntry": tr(&"Bit Drafty In Here"),
	"SpiralisSilere/IceSnail": tr(&"Frost Shrine"),
	"SpiralisSilere/IceSpikeItem": tr(&"Sweater Required"),
	"SpiralisSilere/TilePath": tr(&"Swampy Steps"),
	"SpiralisSilere/GhostHub": tr(&"Dance of the Dandelions"),
	"SpiralisSilere/SnowTunnel": tr(&"A Secret to Snowbody"),
	"SpiralisSilere/Devilblob": tr(&"Devil's Alcove"),
	"SpiralisSilere/IceTowerSave": tr(&"Safety Save"),
	"SpiralisSilere/IceTower": tr(&" Ice Climb"),
	"SpiralisSilere/Labyrinth": tr(&"The Labyrinth"),
	"SpiralisSilere/RainbowHelix": tr(&"Sneaky, Sneaky"),
	"SpiralisSilere/AngryBlock": tr(&"In the Hall of the Pouting Thing"),
	"SpiralisSilere/BossSave": tr(&"Leave your Shoes in the Hall"),
	"SpiralisSilere/Stompy": tr(&"Tippy-Tap Tunnel"),
	"SpiralisSilere/RainbowWave": tr(&"Prismatic Prize"),
	"AmastridaAbyssus/Entrance": tr(&"Heated Gateway"),
	"AmastridaAbyssus/FireHall": tr(&"Hall of Fire"),
	"AmastridaAbyssus/PlatTower1": tr(&"Elevator Shaft"),
	"AmastridaAbyssus/Snelk": tr(&"Scorching Snelks"),
	"AmastridaAbyssus/Grassy1": tr(&"Grassy Shortcut"),
	"AmastridaAbyssus/WaterCave": tr(&"Tunnels of Troubles"),
	"AmastridaAbyssus/PlatTower2": tr(&"Watertower"),
	"AmastridaAbyssus/WaterFireTransition": tr(&"Boiler Room"),
	"AmastridaAbyssus/Walleye": tr(&"Hidden Hideout"),
	"AmastridaAbyssus/Grassy2": tr(&"Green Cache"),
	"AmastridaAbyssus/PinkIntersection": tr(&"Crawlspace"),
	"AmastridaAbyssus/TopSave": tr(&"Break Room"),
	"AmastridaAbyssus/TopHub": tr(&" Snarkour"),
	"AmastridaAbyssus/FireballItem": tr(&"Furnace"),
	"AmastridaAbyssus/PinkTower1": tr(&"Firetower"),
	"AmastridaAbyssus/PinkTower2": tr(&"Firetower"),
	"AmastridaAbyssus/PinkTower3": tr(&"Firetower"),
	"AmastridaAbyssus/SnakeHeart": tr(&"Slitherine Grove"),
	"AmastridaAbyssus/FloatspikeMaze": tr(&"Floaty Fortress"),
	"AmastridaAbyssus/SkyTower": tr(&" High Dive"),
	"AmastridaAbyssus/Spider": tr(&"Whoa mama"),
	"AmastridaAbyssus/Basement": tr(&"Seafloor of Ire"),
	"AmastridaAbyssus/JellyCave": tr(&"Shocked Shell"),
	"AmastridaAbyssus/GravitySnailEntry": tr(&"Up and Across"),
	"AmastridaAbyssus/GravitySnail": tr(&"Gravity Shrine"),
	"AmastridaAbyssus/GravitySnail_ALT": tr(&"  Odd End"),
	"AmastridaAbyssus/GravTunnel": tr(&"Funnel Tunnel"),
	"AmastridaAbyssus/RapidFire": tr(&"Fast Food"),
	"AmastridaAbyssus/PathToSpaceBox": tr(&"Troubled Waters"),
	"AmastridaAbyssus/SpaceBox": tr(&"The Barycenter"),
	"AmastridaAbyssus/LateCrossroad": tr(&"The Bridge"),
	"LuxLirata/Entrance": tr(&"Sky Plaza"),
	"LuxLirata/AnglePassageItem": tr(&"Transit 90"),
	"LuxLirata/AnglePassageTop": tr(&"Transit 90"),
	"LuxLirata/SkyViper": tr(&"Viper's Pass"),
	"LuxLirata/ThreeChamber": tr(&"Triple Trouble"),
	"LuxLirata/PurpleSkyPath": tr(&"Sunset Vista"),
	"LuxLirata/FullMetalSnail": tr(&"Steel Shrine"),
	"LuxLirata/SpaceSecret": tr(&"Space Balcony"),
	"LuxLirata/GearTower": tr(&"Clocktower"),
	"LuxLirata/Devastator": tr(&"The Vault"),
	"LuxLirata/PincerPath": tr(&"Skyway"),
	"LuxLirata/Labyrinth": tr(&"The Other Labyrinth"),
	"LuxLirata/GearHall": tr(&"Final Stretch"),
	"LuxLirata/AngelItem": tr(&"Holy Hideaway"),
	"LuxLirata/IceballItem": tr(&"Arctic Alcove"),
	"LuxLirata/GravityShock": tr(&"Lost Loot"),
	"LuxLirata/LastHall": tr(&"End of the Line"),
	"LuxLirata/LastHeart": tr(&"Reinforcements"),
	"LuxLirata/MoonSnail": tr(&"Moon Arena"),
	"LuxLirata/AfterMoonSnail": tr(&"Victory Road"),
	"Iris/Entrance": tr(&"Wait what"),
	"Iris/Crossroads": tr(&"Where are we, Snaily?"),
	"Iris/Helix": tr(&"Glitched Goodies"),
	"Iris/Shrine": tr(&"Shrine of Iris"),
}


func get_player_name(id:Player.Players, full:bool = false) -> StringName:
	match id:
		Player.Players.SNAILY:
			return tr(&"Snaily Snail") if full else tr(&"Snaily")
		Player.Players.SLUGGY:
			return tr(&"Sluggy Slug") if full else tr(&"Sluggy")
		Player.Players.UPSIDE:
			return tr(&"Upside Snail") if full else tr(&"Upside")
		Player.Players.LEGGY:
			return tr(&"Leggy Snail") if full else tr(&"Leggy")
		Player.Players.BLOBBY:
			return tr(&"Blobby Blob") if full else tr(&"Blobby")
		Player.Players.LEECHY:
			return tr(&"Leechy Leech") if full else tr(&"Leechy")
	return "?"


func get_item_name(id:Item.ItemTypes, specify_shell:bool = false) -> StringName:
	var character:int = 0
	if GameCore.instance:
		int(Statics.current_profile["character"])
	match id:
		Item.ItemTypes.PEASHOOTER:
			return tr(&"Peashooter")
		Item.ItemTypes.BOOMERANG:
			return tr(&"Boomerang")
		Item.ItemTypes.RAINBOW_WAVE:
			return tr(&"Rainbow Wave")
		Item.ItemTypes.DEVASTATOR:
			return tr(&"Devastator")
		Item.ItemTypes.HIGH_JUMP:
			if character == Player.Players.BLOBBY:
				return tr(&"Wall Grab")
			return tr(&"High Jump")
		Item.ItemTypes.SHELL_SHIELD:
			if character == Player.Players.BLOBBY:
				return tr(&"Shelmet")
			return tr(&"Shell Shield")
		Item.ItemTypes.RAPID_FIRE:
			if character == Player.Players.LEECHY:
				return tr(&"Backfire")
			return tr(&"Rapid Fire")
		Item.ItemTypes.ICE_SHELL:
			match character:
				Player.Players.SLUGGY: return tr(&"Ice Slug")
				Player.Players.BLOBBY: return tr(&"Ice Blob")
				Player.Players.LEECHY: return tr(&"Ice Leech")
				_: return tr(&"Ice Shell") if specify_shell else tr(&"Ice Snail")
		Item.ItemTypes.GRAVITY_SHELL:
			match character:
				Player.Players.SNAILY: return tr(&"Gravity Shell") if specify_shell else tr(&"Gravity Snail")
				Player.Players.SNAILY: return tr(&"Gravity Slug")
				Player.Players.UPSIDE: return tr(&"Magnetic Foot")
				Player.Players.LEGGY: return tr(&"Corkscrew Jump")
				Player.Players.BLOBBY: return tr(&"Angel Jump")
				Player.Players.SNAILY: return tr(&"Gravity Leech")
		Item.ItemTypes.METAL_SHELL:
			match character:
				Player.Players.SNAILY: return tr(&"Full Power Slug")
				Player.Players.BLOBBY: return tr(&"Non-Newtonian Blob")
				Player.Players.SNAILY: return tr(&"Full Power Leech")
				_: return tr(&"Full Metal Shell") if specify_shell else tr(&"Full Metal Snail")
		Item.ItemTypes.GRAVITY_SHOCK:
			return tr(&"Gravity Shock")
		Item.ItemTypes.SECRET_BOOMERANG:
			return tr(&"Super Secret Boomerang")
		Item.ItemTypes.DEBUG_WAVE:
			return tr(&"Debug Rainbow Wave")
		Item.ItemTypes.HEART_CONTAINER:
			return tr(&"Heart Container")
		Item.ItemTypes.HELIX_FRAGMENT:
			return tr(&"Helix Fragment")
		#Item.ItemTypes.RADAR_SHELL:
		Item.ItemTypes.WEAPON_LOCK_TRAP:
			return tr(&"Weapon Lock")
		Item.ItemTypes.GRAVITY_LOCK_TRAP:
			return tr(&"Gravity Lock")
		Item.ItemTypes.LULLABY_TRAP:
			return tr(&"Lullaby Trap")
		Item.ItemTypes.SPIDER_TRAP:
			return tr(&"Spider Ambush")
		Item.ItemTypes.WARP_TRAP:
			return tr(&"Warp Trap")
	if specify_shell:
		match character:
			Player.Players.SNAILY: return tr(&"Normal Slug")
			Player.Players.BLOBBY: return tr(&"Normal Blob")
			Player.Players.SNAILY: return tr(&"Normal Leech")
			_: return tr(&"Normal Shell")
	return tr(&"Nothing")
