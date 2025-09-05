import { SerializationHandler } from "./SerializationHandler";

export class JSONHandler implements SerializationHandler {
  decode(data: Buffer): any | null {
    try {
      return JSON.parse(data.toString());
    } catch (error) {
      return null;
    }
  }

  encode(obj: any): string {
    return JSON.stringify(obj);
  }

  getContentType(): string {
    return "application/json";
  }
}
