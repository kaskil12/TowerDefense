extends Node


var peer = ENetMultiplayerPeer.new()
@export var player_scene : PackedScene
func _on_host_game_button_down() -> void:
	print("Become host pressed")
	peer.create_server(1027)
	multiplayer.multiplayer_peer = peer
	multiplayer.peer_connected.connect(add_player)
	add_player()
	$MultiplayerHUD.hide()


func _on_join_as_player_2_button_down() -> void:
	print("Join as player 2")
	peer.create_client("127.0.0.1", 1027)
	multiplayer.multiplayer_peer = peer
func add_player(id = 1):
	var player = player_scene.instantiate()
	player.name = str(id)
	call_deferred("add_child", player)
func exit_game(id):
	multiplayer.peer_disconnected.connect(del_player)
	del_player(id)
	$MultiplayerHUD.hide()
	
func del_player(id):
	rpc("_del_player", id)
	
@rpc("any_peer","call_local")
func _del_player(id):
	get_node(str(id)).queue_free()
