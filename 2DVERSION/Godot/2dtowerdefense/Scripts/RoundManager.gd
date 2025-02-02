extends Node2D

@export var towerOne: Node2D
@export var towerTwo: Node2D

var playerOneScore: int = 0
var playerTwoScore: int = 0

var playerOneTowerHealth: int = 2500
var playerTwoTowerHealth: int = 2500
func _ready():
    pass

func _process(delta: float) -> void:
    pass

func DamageTowerOne(damage: int):
    playerOneTowerHealth -= damage
    if playerOneTowerHealth <= 0:
        PlayerTwoWins()

func DamageTowerTwo(damage: int):
    playerTwoTowerHealth -= damage
    if playerTwoTowerHealth <= 0:
        PlayerOneWins()

func PlayerOneWins():
    print("Player One Wins")

func PlayerTwoWins():
    print("Player Two Wins")

func StartGame():
    print("Game Started")

func ResetGame():
    playerOneTowerHealth = 2500
    playerTwoTowerHealth = 2500
    playerOneScore = 0
    playerTwoScore = 0
    #more reset code here for removing units and reseting prices as well

func EndGame():
    print("Game Ended")