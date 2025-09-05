import { SerializationHandler } from "./SerializationHandler";
import { Messages } from "./Protobuf/Messages";

export class ProtobufHandler implements SerializationHandler {
  decode(data: Buffer): any {
    try {
      // Deserialize the Protobuf data (assuming Messages is the root message type)
      return Messages.decode(data);
    } catch (error) {
      console.error("[Protobuf] Decode error:", error);
      return null;
    }
  }

  encode(obj: any): Buffer {
    try {
      // Serialize the object to Protobuf format (assuming Messages is the root message type)
      const message = Messages.create(obj);
      return Buffer.from(Messages.encode(message).finish());
    } catch (error) {
      console.error("[Protobuf] Encode error:", error);
      return Buffer.alloc(0);
    }
  }

  getContentType(): string {
    return "application/x-protobuf";
  }
}