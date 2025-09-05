import mongoose from "mongoose";
import { mongo } from "./database";

let databaseConfig = new mongo();

async function conectBD() {
    try {
        console.log("Connecting to MongoDB...");
        const db = await mongoose.connect(`mongodb://${databaseConfig.host}/${databaseConfig.db}`);
        console.log("Successful connection.");
        console.log("DB is connected to", databaseConfig.db);
        console.log("Waiting for connections...");
        return db;
    } 
    catch (error) {
        console.error("Connection error:", error);
        throw error;
    }
}

// Exporting the functions to be used in other modules
export { conectBD };