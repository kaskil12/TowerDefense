extends Node

const SERVER_PORT = 8080
const SERVER_IP = "127.0.0.1"
# Called when the node enters the scene tree for the first time.
func become_host():
	print("Become host pressed")
	var server_peer = ENetMultiplayerPeer.new()
	server_peer.create_server(SERVER_PORT)
	multiplayer.multiplayer_peer = server_peer
	multiplayer.peer_connected.connect(_add_player_to_game)
	multiplayer.peer_disconnected.connect(_del_player)
func join_as_player_2():
	print("Join as player 2")
	var server_peer = ENetMultiplayerPeer.new()
	server_peer.create_server(SERVER_PORT)
	
	multiplayer.multiplayer_peer = server_peer
	
	multiplayer.peer_connected.connect(_add_player_to_game)
	multiplayer.peer_disconnected.connect(_del_player)
	
func _add_player_to_game(id: int):
	print("_add_player_to_game" % id)
func _del_player(id: int):
	print("_del_player" % id)
	
	
