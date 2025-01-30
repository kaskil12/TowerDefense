extends CharacterBody2D

@export var move_speed: float = 100.0  # Speed of the enemy
@export var radius: float = 120  # Radius within which to set a random destination
@export var update_interval: float = 2.0  # Time interval to update the destination

@onready var navigation_agent: NavigationAgent2D = $NavigationAgent2D

var timer: float = 0.0

func _ready():
	# Only the server (or authority) should set the destination
	if is_multiplayer_authority():
		set_random_destination()

func _process(delta):
	# Only the server (or authority) should handle movement logic
	if not is_multiplayer_authority():
		return

	timer += delta

	# Update the destination every `update_interval` seconds
	if timer >= update_interval:
		timer = 0.0
		set_random_destination()

	# Move the enemy towards the destination
	if navigation_agent.is_navigation_finished():
		return

	var target_position = navigation_agent.get_next_path_position()
	var direction = (target_position - global_position).normalized()
	velocity = direction * move_speed

	move_and_slide()

	# Synchronize the position with all clients
	rpc("update_position", global_position)

func set_random_destination():
	# Generate a random position within the radius
	var random_offset = Vector2(randf_range(-radius, radius), randf_range(-radius, radius))
	var target_position = global_position + random_offset

	# Set the destination for the NavigationAgent2D
	navigation_agent.target_position = target_position

	# Synchronize the destination with all clients
	rpc("sync_destination", target_position)

@rpc("any_peer", "call_local")
func sync_destination(destination: Vector2):
	# Update the destination on all peers
	navigation_agent.target_position = destination

@rpc("any_peer", "call_local")
func update_position(new_position: Vector2):
	# Update the position on all peers
	global_position = new_position
