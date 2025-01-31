extends Node2D

const SPEED = 300.0  # Movement speed in pixels per second
@onready var cam = $MainCam
var min_x = -500  # Set your minimum x position
var max_x = 500   # Set your maximum x position
var swipe_sensitivity = 1.5  # Adjust sensitivity of swipe

var touch_start_pos = Vector2()
var touch_active = false
var attack = false
var team: int = 0
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
@export var magicOrb: PackedScene
var magicOrbPrice: int = 500
@export var magicOrbText: Label

func _enter_tree() -> void:
	set_multiplayer_authority(name.to_int())

func _ready():
	if is_multiplayer_authority():
		cam.make_current()
		coinText.text = str(coins)

func _process(delta: float) -> void:
	# Only allow movement if this peer has authority
	if not is_multiplayer_authority():
		return
	if MeleeTimer > 0:
		MeleeTimer -= delta
		meleeText.text = str(MeleeTimer)
	else:
		meleeText.text = meleeUpgradePrice
	if WitchTimer > 0:
		WitchTimer -= delta
		witchText.text = str(WitchTimer)
	else:
		witchText.text = witchUpgradePrice
	
		
func _input(event):
	if event is InputEventScreenTouch:
		if event.pressed:
			touch_start_pos = event.position
			touch_active = true
		else:
			touch_active = false
	
	elif event is InputEventScreenDrag and touch_active:
		var delta_x = event.relative.x * swipe_sensitivity
		position.x = clamp(position.x - delta_x, min_x, max_x)

func ToggleAttack():
	attack = !attack

func UpgradeCoins():
	if coins >= coinsUpgradePrice:
		coins -= coinsUpgradePrice
		coinsUpdatePrice += coinsUpdateAmount + 5
		coinText.text = str(coins)

func BuyMelee():
	if coins >= meleeUpgradePrice and MeleeTimer <= 0:
		coins -= meleeUpgradePrice
		coinText.text = str(coins)
		var meleeclone = melee.instance()
		meleeclone.team = team
		meleeclone.player = self
		meleeclone.opponent_tower_position = get_node("/root/Player/Player2/Tower")
		meleeclone.base_position = get_node("/root/Player/Player2/Base")
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
	if coins >= magicOrbPrice:
		coins -= magicOrbPrice
		coinText.text = str(coins)
		magicOrb.level += 1
		magicOrbText.text = "Magic Orb Level: " + str(magicOrb.level)


		
