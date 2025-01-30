extends Node

var peer = ENetMultiplayerPeer.new()
@export var player_scene: PackedScene

func _on_host_game_button_down() -> void:
	print("Become host pressed")
	peer.create_server(1027)
	multiplayer.multiplayer_peer = peer
	multiplayer.peer_connected.connect(add_player)
	add_player(multiplayer.get_unique_id())  # Add the host player
	$MultiplayerHUD.hide()

func _on_join_as_player_2_button_down() -> void:
	print("Join as player 2")
	peer.create_client("127.0.0.1", 1027)
	multiplayer.multiplayer_peer = peer

func add_player(id: int):
	var player = player_scene.instantiate()
	player.name = str(id)
	call_deferred("add_child", player)

func exit_game(id: int):
	multiplayer.peer_disconnected.connect(del_player)
	del_player(id)
	$MultiplayerHUD.hide()

func del_player(id: int):
	rpc("_del_player", id)

@rpc("any_peer", "call_local")
func _del_player(id: int):
	var player_node = get_node_or_null(str(id))
	if player_node:
		player_node.queue_free()
