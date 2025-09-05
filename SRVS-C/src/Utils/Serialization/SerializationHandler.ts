import { JSONHandler } from "./JSONHandler";
import { ProtobufHandler } from "./ProtobufHandler";
import { MessagePackHandler } from "./MessagePackHandler";

export interface SerializationHandler {
  decode(data: Buffer): any | null;
  encode(obj: any): Buffer | string;
  getContentType(): string;
}

export function getSerializer(contentType: string): SerializationHandler {
    switch(contentType.toLowerCase()) {
        case "application/x-protobuf":
            return new ProtobufHandler();
        case "application/x-msgpack":
            return new MessagePackHandler();
        default:
            return new JSONHandler();
    }
}