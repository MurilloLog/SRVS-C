import {IPlayer} from "../Server";
import { globalHandler } from "../Server";
import * as net from "net";

export class Room 
{
    private roomId: string;
    private players: IPlayer[];
    private objects: any[] = []; // Saved objects in the room
    private drawings: any[] = []; // Saved drawings in the room
    private lastActivity: Date;

    constructor(roomId: string, players: IPlayer[] = []) { 
        this.roomId = roomId;
        this.players = players;
        this.lastActivity = new Date();
    }

    /*
     * Broadcast a message to all players in the room, optionally excluding one (the sender).
     * @param data Data to broadcast
     * @param excludePlayerId Optional player ID to exclude from broadcast
     */
    public broadcast(data: any, excludePlayerId?: string): void {
        this.lastActivity = new Date();
        try {
            const message = globalHandler.encode(data);
            this.players.forEach(player => {
                // Skip the excluded player
                if (player.id === excludePlayerId) {
                    return;
                }

                try {
                    if (player.conexion && !player.conexion.destroyed && player.isConnected && 
                    player.conexion.writable) {
                        //player.conexion.write(message + "|", "utf8");
                        player.conexion.write(message);
                    } else {
                        console.log(`Player ${player.id} connection is invalid, destroyed or disconnected.`);
                    }
                } catch (writeError) {
                    console.error(`Error sending data to player ${player.id}:`, writeError);
                }
            });
            this.lastActivity = new Date(); // Update last activity time
        } catch (serializationError) {
            console.error("Error serializing data for broadcast:", serializationError);
        }
    }

    /*
     * Checks if the room is inactive based on the last activity time
     * @param timeoutMs Timeout in milliseconds
     */
    isInactive(timeoutMs: number): boolean {
        return (new Date().getTime() - this.lastActivity.getTime()) > timeoutMs;
    }
    
    /*
     * Returns a player and removes them from the room
     * @param playerId ID of the player to return
     */
    public returnPlayer(playerId: string): void {
        this.removePlayer(playerId);
    }

    /*
     * Adds a player to the room
     * @param player Player object to add
     */
    public addPlayer(player: IPlayer): void {
        if (!this.players.find(p => p.id === player.id)) {
            player.isConnected = true;
            player.roomId = this.roomId;
            this.players.push(player);
            
            // Notify other players about new player
            this.broadcast({
                command: "PLAYER_JOINED",
                playerId: player.id,
                roomId: this.roomId
            }, player.id);
            
            // Send welcome message to new player
            this.sendRoomState(player);
            
            this.lastActivity = new Date();
        } else {
            console.warn(`Player ${player.id} is already in the room.`);
        }
    }

    /*
     * Removes a player from the room
     * @param playerId ID of the player to remove
     */
    public removePlayer(playerId: string): void {
        const initialLength = this.players.length;
        const playerIndex = this.players.findIndex(p => p.id === playerId);
        
        if (playerIndex !== -1) {
            // Notify other players before removal
            this.broadcast({
                command: "PLAYER_LEFT",
                playerId: playerId,
                roomId: this.roomId
            }, playerId);
            
            this.players.splice(playerIndex, 1);
            console.log(`Player ${playerId} removed from room ${this.roomId}.`);
            this.lastActivity = new Date();
        } else {
            console.warn(`Player ${playerId} not found in room ${this.roomId}.`);
        }
    }

    /*
     * Handles player reconnection
     * @param player Reconnected player
     */
    public playerReconnected(player: IPlayer): void {
        this.lastActivity = new Date();
        const index = this.players.findIndex(p => p.id === player.id);
        
        if (index !== -1) {
            // Update existing player connection
            this.players[index] = player;
            /*this.players[index] = {
                ...this.players[index], // Mantenemos propiedades existentes
                conexion: player.conexion,
                isConnected: true,
                lastSeen: new Date(),
                lastActivity: new Date(),
                missedHeartbeats: 0,
                isActive: true
            };*/

            // Notify other players about reconnection
            this.broadcast({
                command: "PLAYER_RECONNECTED",
                playerId: player.id,
                roomId: this.roomId
            }, player.id);
            //this.sendRoomState(this.players[index]);
        } else {
            // If player not found, add them as a new player
            this.addPlayer(player);
        }
            this.lastActivity = new Date(); // Update last activity time
    }

    /*
     * Sends complete room state to a specific player
     * @param player Player to send state to
     */
    private sendRoomState(player: IPlayer): void {
        if (!player.conexion || player.conexion.destroyed) return;

        const roomState = {
            command: "ROOM_STATE",
            roomId: this.roomId,/*
            players: this.players
                .filter(p => p.isConnected)
                .map(p => ({
                    id: p.id,
                    position: p.pos,
                    rotation: p.rot
                })),
            objects: this.objects,
            drawings: this.drawings,*/
            timestamp: Date.now()
        };

        try {
            player.conexion.write(globalHandler.encode(roomState));
        } catch (error) {
            console.error(`Error sending room state to player ${player.id}:`, error);
        }
    }
    
    /*
     * Handles player disconnection
     * @param playerId ID of disconnected player
     */
    public playerDisconnected(playerId: string): void {
        const player = this.players.find(p => p.id === playerId);
        if (player) {
            player.isConnected = false;
            player.lastSeen = new Date();
            
            this.broadcast({
                command: "PLAYER_DISCONNECTED",
                playerId: playerId,
                roomId: this.roomId
            }, playerId);
            
            this.lastActivity = new Date();
        }
    }

    /*
     * Checks if room is empty (all players disconnected)
     */
    public isEmpty(): boolean {
        return this.players.every(p => !p.isConnected);
    }

    /*
     * Checks if room is active (recent activity)
     * @param timeoutMs Timeout in milliseconds
     */
    public isActive(timeoutMs: number = 30000): boolean {
        return (new Date().getTime() - this.lastActivity.getTime()) < timeoutMs;
    }

    /*
     * Gets list of connected player IDs
     */
    public getConnectedPlayerIds(): string[] {
        return this.players.filter(p => p.isConnected).map(p => p.id);
    }

    /*
     * Gets the number of active players in the room
     */
    getActivePlayers(): number {
        return this.players.filter(p => p.isActive).length;
    }

    public updatePlayerReference(updatedPlayer: IPlayer): boolean {
        const index = this.players.findIndex(p => p.id === updatedPlayer.id);
        if (index !== -1) {
            this.players[index] = updatedPlayer;
            return true;
        }
        return false;
    }
}