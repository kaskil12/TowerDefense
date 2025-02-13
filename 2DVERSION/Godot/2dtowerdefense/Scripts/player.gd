extends Node2D

const SPEED = 300.0  # Movement speed in pixels per second
@onready var cam = $"."

var touch_start_pos = Vector2()
var touch_active = false
var attack = false
var team: int = 1
#coins and upgrade variables with text
var coins: int = 200
var coinsUpgradePrice: int = 100
var coinsUpdateAmount: int = 50
@export var coinText: Label

#Melee
@export var melee: PackedScene
var meleeUpgradePrice: int = 50
var meleeCooldown: float = 2.5
var MeleeTimer: float = 0
@export var meleeText: Label

#Witch
@export var witch: PackedScene
var witchUpgradePrice: int = 50
var witchCooldown: float = 5
var WitchTimer: float = 0
@export var witchText: Label

#Magic Orb
@export var magicOrb: Node2D
var magicOrbPrice: int = 500
@export var magicOrbText: Label

@onready var playerhud: Control = $PlayerHud

var opponent_tower_position: Node2D
var base_position: Node2D

func _enter_tree() -> void:
	set_multiplayer_authority(name.to_int())

func _ready():
	if is_multiplayer_authority():
		cam.make_current()
		playerhud.show()
	var DefaultPos = $"../DefaultSpawn"
	if DefaultPos:
		global_position = DefaultPos.global_position
	else:
		print("Node 'DefaultSpawn' not found")
	coinText.text = str(coins)
	if team == 1:
		opponent_tower_position = get_node("/root/Node2D/TowerTwo")
		base_position = get_node("/root/Node2D/TowerOne")
	else:
		opponent_tower_position = get_node("/root/Node2D/TowerOne")
		base_position = get_node("/root/Node2D/TowerOne")
		

func _process(delta: float) -> void:
	# Only allow movement if this peer has authority
	if not is_multiplayer_authority():
		return
	playerhud.show()
	if MeleeTimer > 0:
		MeleeTimer -= delta
		meleeText.text = str(MeleeTimer)
	else:
		meleeText.text = str(meleeUpgradePrice)
	if WitchTimer > 0:
		WitchTimer -= delta
		witchText.text = str(WitchTimer)
	else:
		witchText.text = str(witchUpgradePrice)
	
		


func ToggleAttack():
	attack = !attack

func UpgradeCoins():
	if coins >= coinsUpgradePrice:
		coins -= coinsUpgradePrice
		coinsUpgradePrice += coinsUpdateAmount + 5
		coinText.text = str(coins)

func BuyMelee():
	if coins >= meleeUpgradePrice and MeleeTimer <= 0:
		coins -= meleeUpgradePrice
		coinText.text = str(coins)
		var meleeclone = melee.instantiate()
		meleeclone.team = team
		meleeclone.player = self
		meleeclone.opponent_tower_position = opponent_tower_position
		meleeclone.base_position = base_position
		meleeclone.attacking_opponent_tower = attack
		#add child
		get_parent().add_child(meleeclone)
		meleeclone.global_position = global_position
		MeleeTimer = meleeCooldown



func BuyWitch():
	if coins >= witchUpgradePrice and WitchTimer <= 0:
		coins -= witchUpgradePrice
		coinText.text = str(coins)
		var witchclone = witch.instance()
		witchclone.team = team
		witch.player = self
		witchclone.opponent_tower_position = get_node("/root/Player/Player2/Tower")
		witchclone.base_position = get_node("/root/Player/Player2/Base")
		witchclone.attacking_opponent_tower = attack
		#add child
		get_parent().add_child(witchclone)
		witchclone.global_position = global_position
		WitchTimer = witchCooldown

func UpgradeMagicOrb():
	if team == 1:
		magicOrb = get_node("/root/Player/Player1/MagicOrb")
	else:
		magicOrb = get_node("/root/Player/Player2/MagicOrb")
	if coins >= magicOrbPrice and magicOrb.level < 3:
		coins -= magicOrbPrice
		coinText.text = str(coins)
		magicOrb.level += 1
		magicOrbText.text = "Magic Orb Level: " + str(magicOrb.level)


		
