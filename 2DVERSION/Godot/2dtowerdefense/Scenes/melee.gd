extends CharacterBody2D

var player: Node2D = null
@export var speed: float = 100.0
@export var attack_range: float = 50.0  # Adjust this value as needed
@export var detection_range: float = 100.0  # Adjust this value as needed

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
@onready var character_sprite: AnimatedSprite2D = $CharacterSprite
@onready var animatable_body_2d: AnimationPlayer = $CharacterSprite/AnimatableBody2D


@export var Area2DattackRange: Area2D

func _ready():
	# Initialize the player variable
	var players = get_tree().get_nodes_in_group("players")
	if players.size() > 0:
		player = players[0]
	else:
		print("No players found in the 'players' group.")

	nav_agent.path_desired_distance = 4.0
	nav_agent.target_desired_distance = 2.0

func _process(delta: float):
	var all_players: Array = get_tree().get_nodes_in_group("players")
	if player:
		attacking_opponent_tower = player.attack
		returning_to_base = !player.attack

	# Debug prints
	print("Attack Tower:", attacking_opponent_tower)
	print("Return to Base:", returning_to_base)
	print("Target Found:", target_found)
	print("Target:", target)

	if returning_to_base and base_position != null:
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
	elif attacking_opponent_tower and target_found == false and opponent_tower_position != null:
		move_to(opponent_tower_position.global_position)
		print("Distance to Tower:", global_position.distance_to(opponent_tower_position.global_position))
		if global_position.distance_to(opponent_tower_position.global_position) <= attack_range:
			speed = 0
			if canAttack:
				attackTower()
	else:
		speed = 100  # Reset speed if no conditions are met

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
		print("Target detected:", target)

func _on_detection_area_body_exited(body: Node2D):
	if body == target:
		target = null
		target_found = false
		speed = 100  # Reset speed when target is lost
		print("Target lost.")

func attackTarget():
	if target and target.has_method("take_damage"):
		canAttack = false
		animatable_body_2d.play("Attack")
		character_sprite.play("Attack")
		target.take_damage(10)
		speed = 0
		await get_tree().create_timer(1.0).timeout
		canAttack = true
		speed = 100  # Reset speed after attack
		print("Attacked target.")

func attackTower():
	if (team == 1 or team == 2) and opponent_tower_position and opponent_tower_position.has_method("take_damage"):
		canAttack = false
		opponent_tower_position.take_damage(10)
		speed = 0
		await get_tree().create_timer(1.0).timeout
		canAttack = true
		speed = 100  # Reset speed after attack
		print("Attacked tower.")

func take_damage(damage: int):
	health -= damage
	if health <= 0:
		queue_free()
	print("Took damage. Health:", health)

func SetAttack(attack: bool):
	attacking_opponent_tower = attack
	returning_to_base = !attack
	target_found = false
	target = null
	if opponent_tower_position:
		move_to(opponent_tower_position.global_position)
	print("Set attack state:", attack)
