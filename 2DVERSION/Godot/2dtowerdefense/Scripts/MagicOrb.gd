extends Node2D

var enabled: bool = false
var level: int = 0
var damage: int = 10
var speed: int = 10
var cooldownTimer: int = 0

func _ready():
    pass

func _process(delta: float) -> void:
    if cooldownTimer > 0:
        cooldownTimer -= delta
    elif enabled:
        Shoot()
    match level:
        0:
            enabled = false
            damage = 0
        1:
            enabled = true
            damage = 10
        2:
            enabled = true
            damage = 15
        3:
            enabled = true
            damage = 20
        4:
            enabled = true
            damage = 25

func Shoot():
    if cooldownTimer <= 0:
        print("Shooting")
        cooldownTimer = 1
        

func UpgradeOrb():
    level += 1