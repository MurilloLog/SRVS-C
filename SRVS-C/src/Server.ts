import * as net from "net"
import mongoose from "mongoose";
import crypto from "crypto";
import { mongo } from "./database";
import Player from "./Models/Player";
import Objects from "./Models/Objects";
import Drawings from "./Models/Draws";
import { Room } from "./MatchMaking/Room";
import { conectBD } from "./index";
import { getSerializer, SerializationHandler } from "./Utils/Serialization/SerializationHandler";
import { JSONHandler } from "./Utils/Serialization/JSONHandler";
import { serialize } from "v8";
import * as protobufRoot from "./Utils/Serialization/Protobuf/Messages";
import { ProtobufHandler } from "./Utils/Serialization/ProtobufHandler";

const USERS_PER_ROOM = 1; // Set number of players for each room
const HEARTBEAT_INTERVAL = 40000; // 40 seconds to check player connectivity
const INACTIVITY_THRESHOLD = 20000; // 20 seconds to consider a lost heartbeat
const DISCONNECTION_TIMEOUT = 300000; // 5 minutes to consider a player disconnected

let players: IPlayer[] = [];

let searchRoom: Map<String, IPlayer> = new Map<String, IPlayer>();
let playersPaired = 0;
let roomIdCounter = 0;
let rooms: Map<string, Room> = new Map<string, Room>();

export let globalHandler: SerializationHandler = new JSONHandler();
//export let globalHandler: SerializationHandler = new ProtobufHandler();
export function configureServerHandler(handler: SerializationHandler) {
  globalHandler = handler;
}

export interface IPlayer
{
    id: string;
    conexion: net.Socket;
    roomId?: string;
    lastSeen: Date;
    isConnected: boolean;
    lastActivity: Date;
    heartbeatInterval?: NodeJS.Timeout;
    isActive: boolean;
    missedHeartbeats: number;
    buffer: string;
    cleanup?: () => void;
    serializer: SerializationHandler;
}

// List of all available instructions
const actions = 
{
    ID: "ID",
    SEARCH_ROOM: "SEARCH_ROOM",
    UPDATE_PLAYER_POSE: "UPDATE_PLAYER_POSE",
    UPDATE_OBJECT_POSE: "UPDATE_OBJECT_POSE",
    DRAWING: "DRAWING",
    HEARTBEAT: "HEARTBEAT",
    RECONNECTED: "RECONNECTED",
    WAITING_PLAYER: "WAITING_PLAYER",
    ROOM_CREATED: "ROOM_CREATED"
};

/* Function to generate a unique ID for each player based on their socket's remote address.
 * This ID is used to identify players across reconnections.
 * The ID is a SHA-256 hash of the remote address, truncated to 8 characters.
 */
function generateId(socket: net.Socket): string {
    const remoteAddress = socket.remoteAddress || "unknown";
    const hash = crypto.createHash('sha256').update(remoteAddress).digest('hex');
    return hash.substring(0, 8); // Use only 8 characters
}

/* Function to handle player disconnection.
 * This function marks the player as disconnected, updates their last seen time,
 * clears their heartbeat interval, and removes them from their room if applicable.
 * If the room becomes empty, it is removed from the rooms map.
 */
function handleDisconnection(player: IPlayer) {
    const playerIndex = players.findIndex(p => p.id === player.id);
    if (playerIndex === -1) return;

    try {
        if (player.cleanup) {
            player.cleanup();
        }
        
        if (!player.conexion.destroyed) {
            player.conexion.destroy();
        }
    } catch (error) {
        console.error(`[DISCONNECT] Error destroying socket for ${player.id}:`, error);
    }

    if (player.heartbeatInterval) {
        clearInterval(player.heartbeatInterval);
        player.heartbeatInterval = undefined;
    }

    player.isConnected = false;
    player.lastSeen = new Date();

    if (player.roomId) {
        const room = rooms.get(player.roomId);
        if (room) {
            // Update player reference in case of reconnection
            if (!room.updatePlayerReference(player)) {
                room.playerDisconnected(player.id);
            }
            
            if (room.isEmpty()) {
                rooms.delete(player.roomId);
            }
        }
    }

    if (searchRoom.has(player.id)) {
        searchRoom.delete(player.id);
    }
}

/* Heartbeat setup function to check player connectivity
 * This function will send a heartbeat message to the player every HEARTBEAT_INTERVAL milliseconds
 * and check if the player is still connected. If the player does not respond within INACTIVITY_THRESHOLD,
 * they will be considered disconnected.
 */
function setupHeartbeat(player: IPlayer) {
    if (player.heartbeatInterval) {
        clearInterval(player.heartbeatInterval);
    }

    player.heartbeatInterval = setInterval(() => {
        if (!player.isConnected || !player.conexion.writable || player.conexion.destroyed) {
            console.log(`[HEARTBEAT] Skipping ping for ${player.id} - connection invalid`);
            return;
        }

        const inactiveTime = Date.now() - player.lastActivity.getTime();
        if (inactiveTime > INACTIVITY_THRESHOLD) {
            try {
                player.conexion.write(globalHandler.encode({ command: actions.HEARTBEAT }));
                console.log(`[HEARTBEAT] Sent ping to ${player.id}`);
            } catch (error) {
                console.error(`[HEARTBEAT] Error sending to ${player.id}:`, error);
                handleDisconnection(player);
            }
        }
    }, HEARTBEAT_INTERVAL);
}

/* Function to process incoming messages from players
 * This function parses the JSON data received from the player and calls the appropriate handler
 * based on the command in the message. It also updates the player's last seen time and resets their
 * missed heartbeats counter.
 */
function processMessage(Data: any, player: IPlayer) {
    player.lastSeen = new Date();
    player.missedHeartbeats = 0;

    switch (Data.command) {
        case actions.SEARCH_ROOM:
            handleSearchRoom(Data, player);
            break;
        case actions.HEARTBEAT:
            console.log(`[HEARTBEAT] Received from ${player.id} in room ${player.roomId}`);
            break;
        case actions.UPDATE_PLAYER_POSE:
            handlePlayerPoseUpdate(Data, player);
            break;
        case actions.UPDATE_OBJECT_POSE:
            handleObjectPoseUpdate(Data, player);
            break;
        case actions.DRAWING:
            handleDrawing(Data, player);
            break;
        default:
            console.log("[ERROR] Unrecognized command:", Data.command);
            break;
    }
}

/* Function to handle room search
 * This function is called when a player sends a SEARCH_ROOM command.
 * It adds the player to the searchRoom map and checks if enough players have been paired.
 * If enough players are found, it creates a new room and notifies all players in that room.
 */
async function handleSearchRoom(jsonData: any, player: IPlayer) {
    if (player.roomId) {
        console.log(`[SEARCH BLOCKED] Player ${player.id} is already in room ${player.roomId}`);
        return;
    }

    // Add player to search room if not already present
    if (!searchRoom.has(player.id)) {
        console.log(`[SEARCHING ROOM] Player ${player.id} joining search queue`);
        searchRoom.set(player.id, player);
        playersPaired = Array.from(searchRoom.keys()).length;
    }

    // Notify player they are waiting
    player.conexion.write(globalHandler.encode({ command: actions.WAITING_PLAYER }));

    // Check if enough players are available to create a room
    if (playersPaired >= USERS_PER_ROOM) {
        const roomPlayers: IPlayer[] = [];
        const usedIds = new Set<string>();
        
        // Select players for the room (up to USERS_PER_ROOM)
        searchRoom.forEach(p => {
            if (roomPlayers.length < USERS_PER_ROOM && !usedIds.has(p.id)) {
                roomPlayers.push(p);
                usedIds.add(p.id);
            }
        });

        if (roomPlayers.length === USERS_PER_ROOM) {
            const roomId = `R${++roomIdCounter}`;
            const newRoom = new Room(roomId, roomPlayers);
            rooms.set(roomId, newRoom);

            roomPlayers.forEach(p => {
                p.roomId = roomId;
                searchRoom.delete(p.id);
                p.conexion.write(globalHandler.encode({ command: actions.ROOM_CREATED, roomId: roomId }));
            });

            console.log(`[ROOM CREATED] With ID: "${roomId}" and players: ${roomPlayers.map(p => p.id).join(', ')}`);
            playersPaired = searchRoom.size;
        }
    }
}

/* Function to handle player pose updates
 * This function is called when a player sends an UPDATE_PLAYER_POSE command.
 * It updates the player's position and rotation in the database and notifies other players in the room.
 */
async function handlePlayerPoseUpdate(jsonData: any, player: IPlayer) {
    try {
        const users = await Player.find({_id: jsonData._id}, {_id: 1});
        if (users.length !== 0) {
            await Player.findByIdAndUpdate(jsonData._id, {
                position: jsonData.position,
                rotation: jsonData.rotation,
            });
            console.log(`Player ${jsonData._id} pose updated`);
        }

        if (player.roomId) {
            const room = rooms.get(player.roomId);
            room?.broadcast(jsonData, jsonData._id);
        }
    } catch (error) {
        console.error("[ERROR] Update Player Error:", error);
    }
}

/* Function to handle object pose updates
 * This function is called when a player sends an UPDATE_OBJECT_POSE command.
 * It updates the object's position, rotation, and scale in the database or creates a new object if it doesn't exist.
 * It also notifies other players in the room about the update.
 */
async function handleObjectPoseUpdate(jsonData: any, player: IPlayer) {
    try {
        const objects = await Objects.find({_id: jsonData._id}, {_id: 1});
        if (objects.length !== 0) {
            await Objects.findByIdAndUpdate(jsonData._id, {
                position: jsonData.position,
                rotation: jsonData.rotation,
                scale: jsonData.scale,
                playerEditor: jsonData.playerEditor,
                IsSelected: jsonData.IsSelected
            });
            console.log("[OBJECT POSE UPDATED]");
        } else {
            const newObject = new Objects({
                _id: jsonData._id,
                playerCreator: jsonData.playerCreator,
                playerEditor: jsonData.playerEditor,
                objectMesh: jsonData.objectMesh,
                IsSelected: jsonData.IsSelected,
                position: jsonData.position,
                rotation: jsonData.rotation,
                scale: jsonData.scale
            });
            await newObject.save();
            console.log("New object saved");
        }

        if (player.roomId) {
            const room = rooms.get(player.roomId);
            room?.broadcast(jsonData, jsonData.playerEditor);
        }
    } catch (error) {
        console.error("[ERROR] Update Object Error:", error);
    }
}

/* Function to handle drawing updates
 * This function is called when a player sends a DRAWING command.
 * It saves the drawing data to the database and notifies other players in the room about the drawing.
 * @param jsonData - The drawing data received from the player.
 * @param player - The player who sent the drawing data.
 */
async function handleDrawing(jsonData: any, player: IPlayer) {
    try {
        if (player.roomId) {
            const room = rooms.get(player.roomId);
            room?.broadcast(jsonData); // Broadcast to all players in the room
            //room?.broadcast(jsonData, jsonData._id); // Broadcast to all except sender
        }
    } catch (error) {
        console.error("[ERROR] Drawing Error:", error);
    }
}

/* Server setup
 * This function creates a TCP server that listens for incoming connections.
 * When a new player connects, it generates a unique ID for them and checks if they are reconnecting or a new player.
 * It sets up handlers for data, error, and close events on the socket.
 */
let server = net.createServer((socket) => {
    let id = generateId(socket);
    
    // Search for existing player with the same ID
    console.log(`[CONNECTION] Connection attempt from ${socket.remoteAddress}:${socket.remotePort}`);
    const existingPlayerIndex = players.findIndex(p => p.id === id);
    const isReconnecting = existingPlayerIndex !== -1;

    if (isReconnecting) {
        // Player already exists, handle reconnection
        console.log(`[RECONNECT] Reconnecting player ${id}`);
        const existingPlayer = players[existingPlayerIndex];

        if (existingPlayer.cleanup) {
            existingPlayer.cleanup();
        }

        const reconnectedPlayer: IPlayer = {
            ...existingPlayer,
            conexion: socket,
            isConnected: true,
            lastSeen: new Date(),
            lastActivity: new Date(),
            missedHeartbeats: 0,
            isActive: true,
            buffer: existingPlayer.buffer,
            cleanup: () => {
                socket.removeAllListeners();
            }
        };

        players[existingPlayerIndex] = reconnectedPlayer;

        // Set up socket handlers for the reconnected player
        socket.on('data', (data) => handleSocketData(data, reconnectedPlayer));
        socket.on('error', (error) => handleSocketError(error, reconnectedPlayer));
        socket.on('close', () => handleSocketClose(reconnectedPlayer));

        if (reconnectedPlayer.roomId) {
            const room = rooms.get(reconnectedPlayer.roomId);
            if (room) {
                room.playerReconnected(reconnectedPlayer); // Update player reference in the room
            }
        }

        // Send data saved in the database to the reconnected player
        const reconnectMsg = {
            command: actions.RECONNECTED,
            _id: reconnectedPlayer.id,
            roomId: reconnectedPlayer.roomId || ""
        };

        try {
            socket.write(globalHandler.encode(reconnectMsg));
            console.log(`[RECONNECT] Player with ${id} IP: ${socket.remoteAddress}) in room ${reconnectedPlayer.roomId || 'none'}`);
        } catch (error) {
            console.error(`[RECONNECT] Error sending reconnect message to ${id}:`, error);
        }

        // Reset heartbeat for the reconnected player
        setupHeartbeat(reconnectedPlayer);
        return;
    } else {
        console.log(`[NEW] New player ${id}`);
        const newPlayer: IPlayer = {
            id: id,
            conexion: socket,
            lastSeen: new Date(),
            isConnected: true,
            lastActivity: new Date(),
            isActive: true,
            missedHeartbeats: 0,
            serializer: getSerializer(""),
            buffer: "",
            roomId: undefined
        };
        players.push(newPlayer);
        // Set up socket handlers for the new player
        socket.on('data', (data) => handleSocketData(data, newPlayer));
        socket.on('error', (error) => handleSocketError(error, newPlayer));
        socket.on('close', () => handleSocketClose(newPlayer));
        
        // Send the player ID to the new player
        const idMsg = {
            command: actions.ID,
            _id: newPlayer.id,
        };

        try {
            socket.write(globalHandler.encode(idMsg));
        } catch (error) {
            console.error(`[ERROR] Error sending ID to ${id}:`, error);
        }
        // Set up heartbeat for the new player
        setupHeartbeat(newPlayer);
    }
});

/* Function to detect content type based on the first bytes of the data
 * This function checks if the data starts with '{' or '[' to determine if it's JSON.
 * If it is, it returns "application/json", otherwise it returns "application/x-protobuf".
 * @param data - The data buffer to check.
 * @returns The content type as a string.
 */
function detectContentType(data: Buffer): string {
    // Verify that data has at least one byte to read safely
    const firstBytes = data[0];
    
    if (firstBytes === '{'.charCodeAt(0)) {
        return "application/json";
    }
    if (firstBytes === '['.charCodeAt(0)) {
        return "application/x-protobuf";
    }
    if (firstBytes === 0x82 || firstBytes === 0x84) {
        return "application/x-msgpack";
    }
    // Default to JSON if unknown
    return "application/json";
}

/* Function to handle incoming data from the socket
 * This function processes the data received from the player, checking for complete messages
 * and parsing them as JSON. It then calls processMessage to handle the parsed data.
 * @param data - The data received from the socket.
 * @param player - The player who sent the data.
 */
function handleSocketData(data: Buffer, player: IPlayer) {
    if (!player.conexion.writable || player.conexion.destroyed) {
        console.log(`[WARNING] Connection invalid for ${player.id}, skipping data`);
        return;
    }

    try {
        player.lastActivity = new Date();

        if (!player.serializer) { // Only set serializer if not already set
            player.serializer = getSerializer(detectContentType(data));
        }

        if (player.serializer.getContentType() === "application/json") {
            player.buffer += data.toString();
            processJsonBuffer(player);
        } else {
            processBinaryBuffer(data, player);
        }
    } catch (error) {
        console.error(`[DATA HANDLER ERROR] For ${player.id}:`, error);
    }
}

/* Function to process JSON buffer
 * This function processes the JSON buffer of a player, extracting complete messages
 * separated by the '|' delimiter. It decodes each message and passes it to processMessage.
 * @param player 
 * @returns 
 */
function processJsonBuffer(player: IPlayer) {
    // Validate buffer existence
    if (!player.buffer) {
        player.buffer = "";
        return;
    }

    while (true) {
        const delimiterIndex = player.buffer.indexOf('|');
        if (delimiterIndex === -1) break;

        const message = player.buffer.substring(0, delimiterIndex);
        player.buffer = player.buffer.substring(delimiterIndex + 1);

        if (!message.trim()) continue;

        try {
            const decoded = player.serializer.decode(Buffer.from(message));
            decoded && processMessage(decoded, player);
        } catch (error) {
            console.error(`[JSON PARSE ERROR]`, error);
        }
    }
}

/* Function to process binary buffer
 * This function processes binary data received from the player.
 * It decodes the data using the player's serializer and passes it to processMessage.
 * @param data - The binary data received from the player.
 * @param player - The player who sent the data.
 */
function processBinaryBuffer(data: Buffer, player: IPlayer) {
    try {
        const decoded = player.serializer.decode(data);
        if (decoded) processMessage(decoded, player);
    } catch (error) {
        console.error(`[BINARY PARSE ERROR]`, error);
    }
}

/* Function to handle socket errors
 * This function logs the error and calls handleDisconnection to clean up the player state.
 * @param error - The error that occurred on the socket.
 * @param player - The player who encountered the error.
 */
function handleSocketError(error: Error, player: IPlayer) {
    console.error(`[SOCKET ERROR] ${player.id}: ${error.message}`);
    handleDisconnection(player);
}

/* Function to handle socket closure
 * This function logs the disconnection and calls handleDisconnection to clean up the player state.
 * @param player - The player whose socket was closed.
 */
function handleSocketClose(player: IPlayer) {
    console.log(`[DISCONNECT] Connection closed by ${player.id}`);
    handleDisconnection(player);
}

/* Function to start the server and connect to MongoDB (optional)
 * This function starts the TCP server and connects to MongoDB.
 * It also sets up a cleanup process to remove inactive players and empty rooms periodically.
 */
const PORT = 8080;
server.listen(PORT, () => {
    console.log("[STARTING] Server is starting...");
    mongoose.set('strictQuery', true);
    //conectBD(); // Uncomment this line if you want to connect to MongoDB
    console.log("Server is running on port: " + PORT);

    // Cleanup process to remove inactive players and empty rooms
    setInterval(() => {
        const now = new Date();
        
        // Clean up inactive players
        players = players.filter(player => {
            if (!player.isConnected && 
                (now.getTime() - player.lastSeen.getTime()) > DISCONNECTION_TIMEOUT) {
                    console.log(`[CLEANUP] Removing inactive player ${player.id}`);
                    if (player.roomId) {
                        rooms.get(player.roomId)?.playerDisconnected(player.id);
                    }
                    return false;
            }
            return true;
        });
        
        // Clean up empty rooms
        rooms.forEach((room, roomId) => {
            if (room.isEmpty()) {
                rooms.delete(roomId);
                console.log(`[ROOM CLEANUP] Removed empty room ${roomId}`);
            }
        });
    }, 60000); // 1 minute interval for cleanup
});