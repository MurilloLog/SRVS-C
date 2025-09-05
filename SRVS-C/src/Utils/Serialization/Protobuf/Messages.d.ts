import * as $protobuf from "protobufjs";
import Long = require("long");
/** Properties of a Messages. */
export interface IMessages {

    /** Messages command */
    command?: (string|null);

    /** Messages _id */
    _id?: (string|null);

    /** Messages roomId */
    roomId?: (string|null);

    /** Messages anchor */
    anchor?: (Messages.IAnchor|null);

    /** Messages line */
    line?: (Messages.ILine|null);
}

/** Represents a Messages. */
export class Messages implements IMessages {

    /**
     * Constructs a new Messages.
     * @param [properties] Properties to set
     */
    constructor(properties?: IMessages);

    /** Messages command. */
    public command: string;

    /** Messages _id. */
    public _id: string;

    /** Messages roomId. */
    public roomId: string;

    /** Messages anchor. */
    public anchor?: (Messages.IAnchor|null);

    /** Messages line. */
    public line?: (Messages.ILine|null);

    /**
     * Creates a new Messages instance using the specified properties.
     * @param [properties] Properties to set
     * @returns Messages instance
     */
    public static create(properties?: IMessages): Messages;

    /**
     * Encodes the specified Messages message. Does not implicitly {@link Messages.verify|verify} messages.
     * @param message Messages message or plain object to encode
     * @param [writer] Writer to encode to
     * @returns Writer
     */
    public static encode(message: IMessages, writer?: $protobuf.Writer): $protobuf.Writer;

    /**
     * Encodes the specified Messages message, length delimited. Does not implicitly {@link Messages.verify|verify} messages.
     * @param message Messages message or plain object to encode
     * @param [writer] Writer to encode to
     * @returns Writer
     */
    public static encodeDelimited(message: IMessages, writer?: $protobuf.Writer): $protobuf.Writer;

    /**
     * Decodes a Messages message from the specified reader or buffer.
     * @param reader Reader or buffer to decode from
     * @param [length] Message length if known beforehand
     * @returns Messages
     * @throws {Error} If the payload is not a reader or valid buffer
     * @throws {$protobuf.util.ProtocolError} If required fields are missing
     */
    public static decode(reader: ($protobuf.Reader|Uint8Array), length?: number): Messages;

    /**
     * Decodes a Messages message from the specified reader or buffer, length delimited.
     * @param reader Reader or buffer to decode from
     * @returns Messages
     * @throws {Error} If the payload is not a reader or valid buffer
     * @throws {$protobuf.util.ProtocolError} If required fields are missing
     */
    public static decodeDelimited(reader: ($protobuf.Reader|Uint8Array)): Messages;

    /**
     * Verifies a Messages message.
     * @param message Plain object to verify
     * @returns `null` if valid, otherwise the reason why it is not
     */
    public static verify(message: { [k: string]: any }): (string|null);

    /**
     * Creates a Messages message from a plain object. Also converts values to their respective internal types.
     * @param object Plain object
     * @returns Messages
     */
    public static fromObject(object: { [k: string]: any }): Messages;

    /**
     * Creates a plain object from a Messages message. Also converts values to other types if specified.
     * @param message Messages
     * @param [options] Conversion options
     * @returns Plain object
     */
    public static toObject(message: Messages, options?: $protobuf.IConversionOptions): { [k: string]: any };

    /**
     * Converts this Messages to JSON.
     * @returns JSON object
     */
    public toJSON(): { [k: string]: any };

    /**
     * Gets the default type url for Messages
     * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
     * @returns The default type url
     */
    public static getTypeUrl(typeUrlPrefix?: string): string;
}

export namespace Messages {

    /** Properties of an Anchor. */
    interface IAnchor {

        /** Anchor anchorID */
        anchorID?: (string|null);

        /** Anchor position */
        position?: (number[]|null);
    }

    /** Represents an Anchor. */
    class Anchor implements IAnchor {

        /**
         * Constructs a new Anchor.
         * @param [properties] Properties to set
         */
        constructor(properties?: Messages.IAnchor);

        /** Anchor anchorID. */
        public anchorID: string;

        /** Anchor position. */
        public position: number[];

        /**
         * Creates a new Anchor instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Anchor instance
         */
        public static create(properties?: Messages.IAnchor): Messages.Anchor;

        /**
         * Encodes the specified Anchor message. Does not implicitly {@link Messages.Anchor.verify|verify} messages.
         * @param message Anchor message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: Messages.IAnchor, writer?: $protobuf.Writer): $protobuf.Writer;

        /**
         * Encodes the specified Anchor message, length delimited. Does not implicitly {@link Messages.Anchor.verify|verify} messages.
         * @param message Anchor message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: Messages.IAnchor, writer?: $protobuf.Writer): $protobuf.Writer;

        /**
         * Decodes an Anchor message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Anchor
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: ($protobuf.Reader|Uint8Array), length?: number): Messages.Anchor;

        /**
         * Decodes an Anchor message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Anchor
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: ($protobuf.Reader|Uint8Array)): Messages.Anchor;

        /**
         * Verifies an Anchor message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: any }): (string|null);

        /**
         * Creates an Anchor message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Anchor
         */
        public static fromObject(object: { [k: string]: any }): Messages.Anchor;

        /**
         * Creates a plain object from an Anchor message. Also converts values to other types if specified.
         * @param message Anchor
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: Messages.Anchor, options?: $protobuf.IConversionOptions): { [k: string]: any };

        /**
         * Converts this Anchor to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: any };

        /**
         * Gets the default type url for Anchor
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string;
    }

    /** Properties of a Line. */
    interface ILine {

        /** Line points */
        points?: (number[]|null);

        /** Line color */
        color?: (number|null);

        /** Line size */
        size?: (number|null);
    }

    /** Represents a Line. */
    class Line implements ILine {

        /**
         * Constructs a new Line.
         * @param [properties] Properties to set
         */
        constructor(properties?: Messages.ILine);

        /** Line points. */
        public points: number[];

        /** Line color. */
        public color: number;

        /** Line size. */
        public size: number;

        /**
         * Creates a new Line instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Line instance
         */
        public static create(properties?: Messages.ILine): Messages.Line;

        /**
         * Encodes the specified Line message. Does not implicitly {@link Messages.Line.verify|verify} messages.
         * @param message Line message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: Messages.ILine, writer?: $protobuf.Writer): $protobuf.Writer;

        /**
         * Encodes the specified Line message, length delimited. Does not implicitly {@link Messages.Line.verify|verify} messages.
         * @param message Line message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: Messages.ILine, writer?: $protobuf.Writer): $protobuf.Writer;

        /**
         * Decodes a Line message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Line
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: ($protobuf.Reader|Uint8Array), length?: number): Messages.Line;

        /**
         * Decodes a Line message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Line
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: ($protobuf.Reader|Uint8Array)): Messages.Line;

        /**
         * Verifies a Line message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: any }): (string|null);

        /**
         * Creates a Line message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Line
         */
        public static fromObject(object: { [k: string]: any }): Messages.Line;

        /**
         * Creates a plain object from a Line message. Also converts values to other types if specified.
         * @param message Line
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: Messages.Line, options?: $protobuf.IConversionOptions): { [k: string]: any };

        /**
         * Converts this Line to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: any };

        /**
         * Gets the default type url for Line
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string;
    }
}
