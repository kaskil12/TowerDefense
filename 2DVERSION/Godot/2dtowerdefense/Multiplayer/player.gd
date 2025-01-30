extends Node2D

const SPEED = 300.0  # Movement speed in pixels per second
@onready var cam = $MainCam
var min_x = -500  # Set your minimum x position
var max_x = 500   # Set your maximum x position
var swipe_sensitivity = 1.5  # Adjust sensitivity of swipe

var touch_start_pos = Vector2()
var touch_active = false
func _enter_tree() -> void:
	set_multiplayer_authority(name.to_int())

func _ready():
	if is_multiplayer_authority():
		cam.make_current()

func _process(delta: float) -> void:
	# Only allow movement if this peer has authority
	if not is_multiplayer_authority():
		return
		
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

		
