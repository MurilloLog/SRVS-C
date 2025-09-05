import { SerializationHandler } from "./SerializationHandler";
export class MessagePackHandler implements SerializationHandler {
    decode(data: Buffer): any {
        // Pending logic to deserialize MessagePack data (optional)
    }
    encode(obj: any): Buffer {
        // Pending logic to serialize object to MessagePack (optional)
        return Buffer.from([]);
    }
    getContentType(): string {
        return "application/x-msgpack";
    }
}