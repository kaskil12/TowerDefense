extends Node2D

const SPEED = 300.0  # Movement speed in pixels per second
@onready var cam = $MainCam

func _enter_tree() -> void:
	set_multiplayer_authority(name.to_int())

func _ready():
	if is_multiplayer_authority():
		cam.make_current()

func _process(delta: float) -> void:
	# Only allow movement if this peer has authority
	if not is_multiplayer_authority():
		return

	# Initialize movement vector
	var movement = Vector2.ZERO

	# Check input and update movement vector
	if Input.is_action_pressed("up"):
		movement.y -= 1
	if Input.is_action_pressed("down"):
		movement.y += 1
	if Input.is_action_pressed("left"):
		movement.x -= 1
	if Input.is_action_pressed("right"):
		movement.x += 1

	# Normalize the movement vector to prevent faster diagonal movement
	if movement.length() > 0:
		movement = movement.normalized()

	# Update the position based on movement and speed
	position += movement * SPEED * delta
