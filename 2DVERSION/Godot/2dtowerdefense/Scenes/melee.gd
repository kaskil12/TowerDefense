extends CharacterBody2D

@export var speed: float = 100.0
@export var attack_range: float = 5.0
@export var detection_range: float = 10.0

@export var opponent_tower_position: Node2D
@export var base_position: Node2D

var attacking_opponent_tower: bool = false
var returning_to_base: bool = false
var target_found: bool = false
var target: Node2D = null

@onready var nav_agent: NavigationAgent2D = $NavigationAgent2D

func _ready():
	nav_agent.path_desired_distance = 4.0
	nav_agent.target_desired_distance = 2.0

func _process(delta: float):
	if Input.is_action_just_pressed("attack"):
		attacking_opponent_tower = !attacking_opponent_tower
	if Input.is_action_just_pressed("defend"):
		returning_to_base = !returning_to_base
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
	elif attacking_opponent_tower:
		move_to(opponent_tower_position.global_position)

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
	if body.is_in_group("opponents") and not returning_to_base:
		target = body
		target_found = true

func _on_detection_area_body_exited(body: Node2D):
	if body == target:
		target = null
		target_found = false
