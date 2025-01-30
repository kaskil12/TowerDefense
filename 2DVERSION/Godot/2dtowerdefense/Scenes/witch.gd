extends Node2D

# Team Settings
@export var team: int = 1

# Navigation
@export var target_base: NodePath
@export var home_base_position: NodePath

# Health Settings
@export var health: int = 100
@export var max_health: int = 100
@export var health_bar: ProgressBar
@export var health_canvas: CanvasLayer

# Attack Settings
@export var damage: int = 10
@export var can_attack: bool = true
@export var orb_shoot: bool = true
@export var attack_opponent: bool = false

# Orb Settings
@export var orb_prefab: PackedScene
@export var orb_spawn_point: NodePath
@export var stop_distance: float = 5.0

# Other Settings
@export var witch_sprite: Sprite2D
@export var detection_range: float = 100.0
@export var is_invincible: bool = false
@export var animator: AnimationPlayer

var distance_to_home_base_local: float = 0.0
var home_base_position_local_offset: float = 20.0
var home_base_position_local: Vector2
var target_chosen: Node2D = null
var navigation_agent: NavigationAgent2D

func _ready():
	orb_shoot = true
	can_attack = true
	health_bar.max_value = max_health
	health_bar.value = health
	health_canvas.visible = false
	navigation_agent = $NavigationAgent2D
	navigation_agent.avoidance_enabled = true
	navigation_agent.velocity_computed.connect(_on_velocity_computed)
	set_team(team)

func _process(delta):
	find_and_attack()
	if health < max_health:
		if not health_canvas.visible:
			health_canvas.visible = true
		health_bar.value = health
	else:
		if health_canvas.visible:
			health_canvas.visible = false

	distance_to_home_base_local = global_position.distance_to(home_base_position_local)
	if navigation_agent.is_navigation_finished():
		var movement_direction = navigation_agent.get_next_path_position() - global_position
		if movement_direction.length() > 0.01:
			if movement_direction.x > 0:
				witch_sprite.flip_h = false
			else:
				witch_sprite.flip_h = true

func find_and_attack():
	var colliders = $Area2D.get_overlapping_bodies()
	var target_found = false
	var nearest_unit = null
	var nearest_distance = INF

	for body in colliders:
		if (body.is_in_group("PawnOne") and team == 2 and distance_to_home_base_local < 100) or \
		   (body.is_in_group("PawnTwo") and team == 1 and distance_to_home_base_local < 100) or \
		   (body.is_in_group("PawnOne") and team == 2 and attack_opponent) or \
		   (body.is_in_group("PawnTwo") and team == 1 and attack_opponent):
			target_found = true
			if target_chosen == null:
				target_chosen = body
			navigation_agent.set_target_position(target_chosen.global_position)
			if navigation_agent.is_navigation_finished() and navigation_agent.distance_to_target() > 10:
				navigation_agent.set_target_position(body.global_position)
			navigation_agent.target_desired_distance = 10
			if orb_shoot:
				orb_shoot = false
				orb_attack()
				spawn_orb(target_chosen)
		elif body.is_in_group("Melee") and body.team == team:
			var distance = global_position.distance_to(body.global_position)
			if distance < nearest_distance:
				nearest_distance = distance
				nearest_unit = body

	if not target_found and attack_opponent:
		if nearest_unit != null:
			var offset_position = nearest_unit.global_position - nearest_unit.transform.x * 15
			if global_position.distance_to(offset_position) > 5:
				navigation_agent.set_target_position(offset_position)
			navigation_agent.target_desired_distance = 0
		else:
			navigation_agent.set_target_position(get_node(target_base).global_position)
			if navigation_agent.distance_to_target() < 100 and orb_shoot:
				orb_shoot = false
				orb_attack()
				spawn_orb(get_node(target_base))
	elif not target_found and not attack_opponent:
		navigation_agent.set_target_position(home_base_position_local)
		if navigation_agent.distance_to_target() < 50 and navigation_agent.is_navigation_finished():
			navigation_agent.enabled = false

	if target_found or attack_opponent:
		if not navigation_agent.enabled:
			navigation_agent.enabled = true

func spawn_orb(target: Node2D):
	var orb_instance = orb_prefab.instantiate()
	get_parent().add_child(orb_instance)
	orb_instance.global_position = get_node(orb_spawn_point).global_position
	orb_instance.team = team
	orb_instance.damage = damage
	orb_instance.target = target

func set_team(new_team: int):
	team = new_team
	if team == 1:
		target_base = get_tree().get_nodes_in_group("PlayerTwoBase")[0].get_path()
		home_base_position = get_tree().get_nodes_in_group("HomeBaseOne")[0].get_path()
		home_base_position_local = get_node(home_base_position).global_position + Vector2(home_base_position_local_offset, 0)
		add_to_group("PawnOne")
	elif team == 2:
		target_base = get_tree().get_nodes_in_group("PlayerOneBase")[0].get_path()
		home_base_position = get_tree().get_nodes_in_group("HomeBaseTwo")[0].get_path()
		home_base_position_local = get_node(home_base_position).global_position - Vector2(home_base_position_local_offset, 0)
		add_to_group("PawnTwo")
	navigation_agent.set_target_position(get_node(target_base).global_position)

func toggle_attack(new_attack_opponent: bool):
	attack_opponent = new_attack_opponent

func take_damage(damage_amount: int):
	if not is_invincible:
		health -= damage_amount
		if health <= 0:
			queue_free()

func orb_attack():
	animator.play("Attack")
	await get_tree().create_timer(3.0).timeout
	orb_shoot = true

func _on_velocity_computed(safe_velocity: Vector2):
	position += safe_velocity * get_process_delta_time()
