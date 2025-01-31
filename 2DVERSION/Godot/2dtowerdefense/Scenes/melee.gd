extends CharacterBody2D

var player: Node2D = null
@export var speed: float = 100.0
@export var attack_range: float = 5.0
@export var detection_range: float = 10.0

# @export var areabody: Area2D

@export var opponent_tower_position: Node2D
@export var base_position: Node2D

var attacking_opponent_tower: bool = false
var returning_to_base: bool = false
var target_found: bool = false
var target = null
var team: int = 0
var canAttack: bool = true
var health: int = 100
@onready var nav_agent: NavigationAgent2D = $NavigationAgent2D

func _ready():
	nav_agent.path_desired_distance = 4.0
	nav_agent.target_desired_distance = 2.0

func _process(delta: float):
	if player.team == team:
		attacking_opponent_tower = player.attack
		returning_to_base = !player.attack
	else:
		#find other player
		
	
	# if Input.is_action_just_pressed("attack"):
	# 	attacking_opponent_tower = !attacking_opponent_tower
	# if Input.is_action_just_pressed("defend"):
	# 	returning_to_base = !returning_to_base
	print("attack",attacking_opponent_tower)
	print("return",returning_to_base)
	if returning_to_base:
		attacking_opponent_tower = false
		target_found = false
		move_to(base_position.global_position)
		if global_position.distance_to(base_position.global_position) <= attack_range:
			returning_to_base = false
	elif target_found and target:
		move_to(target.global_position)
		if global_position.distance_to(target.global_position) <= attack_range:
			speed = 0
			if canAttack:
				attackTarget()
	elif attacking_opponent_tower and target_found == false:
		move_to(opponent_tower_position.global_position)
		if global_position.distance_to(opponent_tower_position.global_position) <= attack_range:
			speed = 0
			if canAttack:
				attackTower()
	else:
		speed = 100

func _physics_process(delta: float):
	if nav_agent.is_navigation_finished():
		return
	
	var next_path_position = nav_agent.get_next_path_position()
	var direction = (next_path_position - global_position).normalized()
	velocity = direction * speed
	move_and_slide()

func move_to(target_position: Vector2):
	nav_agent.target_position = target_position

func _on_detection_area_body_entered(body: Node2D):
	if body.is_in_group("pawns") and not returning_to_base and body.team != team:
		target = body
		target_found = true

func _on_detection_area_body_exited(body: Node2D):
	if body == target:
		target = null
		target_found = false

func attackTarget():
	if target:
		canAttack = false
		target.method("take_damage", 10)
		speed = 0
		#delay
		yield(get_tree().create_timer(1.0), "timeout")
		canAttack = true

func attackTower():
	if team == 1:
		opponent_tower_position.method("take_damage", 10)
		speed = 0
		#delay
		yield(get_tree().create_timer(1.0), "timeout")
	elif team == 2:
		opponent_tower_position.method("take_damage", 10)
		speed = 0
		#delay
		yield(get_tree().create_timer(1.0), "timeout")
func take_damage(damage: int):
	health -= damage
	if health <= 0:
		queue_free()

func SetAttack(attack: bool):
	attacking_opponent_tower = attack
	returning_to_base = !attack
	target_found = false
	target = null
	move_to(opponent_tower_position.global_position)
