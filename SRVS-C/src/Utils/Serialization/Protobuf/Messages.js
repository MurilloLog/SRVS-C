/*eslint-disable block-scoped-var, id-length, no-control-regex, no-magic-numbers, no-prototype-builtins, no-redeclare, no-shadow, no-var, sort-vars*/
"use strict";

var $protobuf = require("protobufjs/minimal");

// Common aliases
var $Reader = $protobuf.Reader, $Writer = $protobuf.Writer, $util = $protobuf.util;

// Exported root namespace
var $root = $protobuf.roots["default"] || ($protobuf.roots["default"] = {});

$root.Messages = (function() {

    /**
     * Properties of a Messages.
     * @exports IMessages
     * @interface IMessages
     * @property {string|null} [command] Messages command
     * @property {string|null} [_id] Messages _id
     * @property {string|null} [roomId] Messages roomId
     * @property {Messages.IAnchor|null} [anchor] Messages anchor
     * @property {Messages.ILine|null} [line] Messages line
     */

    /**
     * Constructs a new Messages.
     * @exports Messages
     * @classdesc Represents a Messages.
     * @implements IMessages
     * @constructor
     * @param {IMessages=} [properties] Properties to set
     */
    function Messages(properties) {
        if (properties)
            for (var keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                if (properties[keys[i]] != null)
                    this[keys[i]] = properties[keys[i]];
    }

    /**
     * Messages command.
     * @member {string} command
     * @memberof Messages
     * @instance
     */
    Messages.prototype.command = "";

    /**
     * Messages _id.
     * @member {string} _id
     * @memberof Messages
     * @instance
     */
    Messages.prototype._id = "";

    /**
     * Messages roomId.
     * @member {string} roomId
     * @memberof Messages
     * @instance
     */
    Messages.prototype.roomId = "";

    /**
     * Messages anchor.
     * @member {Messages.IAnchor|null|undefined} anchor
     * @memberof Messages
     * @instance
     */
    Messages.prototype.anchor = null;

    /**
     * Messages line.
     * @member {Messages.ILine|null|undefined} line
     * @memberof Messages
     * @instance
     */
    Messages.prototype.line = null;

    /**
     * Creates a new Messages instance using the specified properties.
     * @function create
     * @memberof Messages
     * @static
     * @param {IMessages=} [properties] Properties to set
     * @returns {Messages} Messages instance
     */
    Messages.create = function create(properties) {
        return new Messages(properties);
    };

    /**
     * Encodes the specified Messages message. Does not implicitly {@link Messages.verify|verify} messages.
     * @function encode
     * @memberof Messages
     * @static
     * @param {IMessages} message Messages message or plain object to encode
     * @param {$protobuf.Writer} [writer] Writer to encode to
     * @returns {$protobuf.Writer} Writer
     */
    Messages.encode = function encode(message, writer) {
        if (!writer)
            writer = $Writer.create();
        if (message.command != null && Object.hasOwnProperty.call(message, "command"))
            writer.uint32(/* id 1, wireType 2 =*/10).string(message.command);
        if (message._id != null && Object.hasOwnProperty.call(message, "_id"))
            writer.uint32(/* id 2, wireType 2 =*/18).string(message._id);
        if (message.roomId != null && Object.hasOwnProperty.call(message, "roomId"))
            writer.uint32(/* id 3, wireType 2 =*/26).string(message.roomId);
        if (message.anchor != null && Object.hasOwnProperty.call(message, "anchor"))
            $root.Messages.Anchor.encode(message.anchor, writer.uint32(/* id 4, wireType 2 =*/34).fork()).ldelim();
        if (message.line != null && Object.hasOwnProperty.call(message, "line"))
            $root.Messages.Line.encode(message.line, writer.uint32(/* id 5, wireType 2 =*/42).fork()).ldelim();
        return writer;
    };

    /**
     * Encodes the specified Messages message, length delimited. Does not implicitly {@link Messages.verify|verify} messages.
     * @function encodeDelimited
     * @memberof Messages
     * @static
     * @param {IMessages} message Messages message or plain object to encode
     * @param {$protobuf.Writer} [writer] Writer to encode to
     * @returns {$protobuf.Writer} Writer
     */
    Messages.encodeDelimited = function encodeDelimited(message, writer) {
        return this.encode(message, writer).ldelim();
    };

    /**
     * Decodes a Messages message from the specified reader or buffer.
     * @function decode
     * @memberof Messages
     * @static
     * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
     * @param {number} [length] Message length if known beforehand
     * @returns {Messages} Messages
     * @throws {Error} If the payload is not a reader or valid buffer
     * @throws {$protobuf.util.ProtocolError} If required fields are missing
     */
    Messages.decode = function decode(reader, length, error) {
        if (!(reader instanceof $Reader))
            reader = $Reader.create(reader);
        var end = length === undefined ? reader.len : reader.pos + length, message = new $root.Messages();
        while (reader.pos < end) {
            var tag = reader.uint32();
            if (tag === error)
                break;
            switch (tag >>> 3) {
            case 1: {
                    message.command = reader.string();
                    break;
                }
            case 2: {
                    message._id = reader.string();
                    break;
                }
            case 3: {
                    message.roomId = reader.string();
                    break;
                }
            case 4: {
                    message.anchor = $root.Messages.Anchor.decode(reader, reader.uint32());
                    break;
                }
            case 5: {
                    message.line = $root.Messages.Line.decode(reader, reader.uint32());
                    break;
                }
            default:
                reader.skipType(tag & 7);
                break;
            }
        }
        return message;
    };

    /**
     * Decodes a Messages message from the specified reader or buffer, length delimited.
     * @function decodeDelimited
     * @memberof Messages
     * @static
     * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
     * @returns {Messages} Messages
     * @throws {Error} If the payload is not a reader or valid buffer
     * @throws {$protobuf.util.ProtocolError} If required fields are missing
     */
    Messages.decodeDelimited = function decodeDelimited(reader) {
        if (!(reader instanceof $Reader))
            reader = new $Reader(reader);
        return this.decode(reader, reader.uint32());
    };

    /**
     * Verifies a Messages message.
     * @function verify
     * @memberof Messages
     * @static
     * @param {Object.<string,*>} message Plain object to verify
     * @returns {string|null} `null` if valid, otherwise the reason why it is not
     */
    Messages.verify = function verify(message) {
        if (typeof message !== "object" || message === null)
            return "object expected";
        if (message.command != null && message.hasOwnProperty("command"))
            if (!$util.isString(message.command))
                return "command: string expected";
        if (message._id != null && message.hasOwnProperty("_id"))
            if (!$util.isString(message._id))
                return "_id: string expected";
        if (message.roomId != null && message.hasOwnProperty("roomId"))
            if (!$util.isString(message.roomId))
                return "roomId: string expected";
        if (message.anchor != null && message.hasOwnProperty("anchor")) {
            var error = $root.Messages.Anchor.verify(message.anchor);
            if (error)
                return "anchor." + error;
        }
        if (message.line != null && message.hasOwnProperty("line")) {
            var error = $root.Messages.Line.verify(message.line);
            if (error)
                return "line." + error;
        }
        return null;
    };

    /**
     * Creates a Messages message from a plain object. Also converts values to their respective internal types.
     * @function fromObject
     * @memberof Messages
     * @static
     * @param {Object.<string,*>} object Plain object
     * @returns {Messages} Messages
     */
    Messages.fromObject = function fromObject(object) {
        if (object instanceof $root.Messages)
            return object;
        var message = new $root.Messages();
        if (object.command != null)
            message.command = String(object.command);
        if (object._id != null)
            message._id = String(object._id);
        if (object.roomId != null)
            message.roomId = String(object.roomId);
        if (object.anchor != null) {
            if (typeof object.anchor !== "object")
                throw TypeError(".Messages.anchor: object expected");
            message.anchor = $root.Messages.Anchor.fromObject(object.anchor);
        }
        if (object.line != null) {
            if (typeof object.line !== "object")
                throw TypeError(".Messages.line: object expected");
            message.line = $root.Messages.Line.fromObject(object.line);
        }
        return message;
    };

    /**
     * Creates a plain object from a Messages message. Also converts values to other types if specified.
     * @function toObject
     * @memberof Messages
     * @static
     * @param {Messages} message Messages
     * @param {$protobuf.IConversionOptions} [options] Conversion options
     * @returns {Object.<string,*>} Plain object
     */
    Messages.toObject = function toObject(message, options) {
        if (!options)
            options = {};
        var object = {};
        if (options.defaults) {
            object.command = "";
            object._id = "";
            object.roomId = "";
            object.anchor = null;
            object.line = null;
        }
        if (message.command != null && message.hasOwnProperty("command"))
            object.command = message.command;
        if (message._id != null && message.hasOwnProperty("_id"))
            object._id = message._id;
        if (message.roomId != null && message.hasOwnProperty("roomId"))
            object.roomId = message.roomId;
        if (message.anchor != null && message.hasOwnProperty("anchor"))
            object.anchor = $root.Messages.Anchor.toObject(message.anchor, options);
        if (message.line != null && message.hasOwnProperty("line"))
            object.line = $root.Messages.Line.toObject(message.line, options);
        return object;
    };

    /**
     * Converts this Messages to JSON.
     * @function toJSON
     * @memberof Messages
     * @instance
     * @returns {Object.<string,*>} JSON object
     */
    Messages.prototype.toJSON = function toJSON() {
        return this.constructor.toObject(this, $protobuf.util.toJSONOptions);
    };

    /**
     * Gets the default type url for Messages
     * @function getTypeUrl
     * @memberof Messages
     * @static
     * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
     * @returns {string} The default type url
     */
    Messages.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
        if (typeUrlPrefix === undefined) {
            typeUrlPrefix = "type.googleapis.com";
        }
        return typeUrlPrefix + "/Messages";
    };

    Messages.Anchor = (function() {

        /**
         * Properties of an Anchor.
         * @memberof Messages
         * @interface IAnchor
         * @property {string|null} [anchorID] Anchor anchorID
         * @property {Array.<number>|null} [position] Anchor position
         */

        /**
         * Constructs a new Anchor.
         * @memberof Messages
         * @classdesc Represents an Anchor.
         * @implements IAnchor
         * @constructor
         * @param {Messages.IAnchor=} [properties] Properties to set
         */
        function Anchor(properties) {
            this.position = [];
            if (properties)
                for (var keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null)
                        this[keys[i]] = properties[keys[i]];
        }

        /**
         * Anchor anchorID.
         * @member {string} anchorID
         * @memberof Messages.Anchor
         * @instance
         */
        Anchor.prototype.anchorID = "";

        /**
         * Anchor position.
         * @member {Array.<number>} position
         * @memberof Messages.Anchor
         * @instance
         */
        Anchor.prototype.position = $util.emptyArray;

        /**
         * Creates a new Anchor instance using the specified properties.
         * @function create
         * @memberof Messages.Anchor
         * @static
         * @param {Messages.IAnchor=} [properties] Properties to set
         * @returns {Messages.Anchor} Anchor instance
         */
        Anchor.create = function create(properties) {
            return new Anchor(properties);
        };

        /**
         * Encodes the specified Anchor message. Does not implicitly {@link Messages.Anchor.verify|verify} messages.
         * @function encode
         * @memberof Messages.Anchor
         * @static
         * @param {Messages.IAnchor} message Anchor message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Anchor.encode = function encode(message, writer) {
            if (!writer)
                writer = $Writer.create();
            if (message.anchorID != null && Object.hasOwnProperty.call(message, "anchorID"))
                writer.uint32(/* id 1, wireType 2 =*/10).string(message.anchorID);
            if (message.position != null && message.position.length) {
                writer.uint32(/* id 2, wireType 2 =*/18).fork();
                for (var i = 0; i < message.position.length; ++i)
                    writer.float(message.position[i]);
                writer.ldelim();
            }
            return writer;
        };

        /**
         * Encodes the specified Anchor message, length delimited. Does not implicitly {@link Messages.Anchor.verify|verify} messages.
         * @function encodeDelimited
         * @memberof Messages.Anchor
         * @static
         * @param {Messages.IAnchor} message Anchor message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Anchor.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim();
        };

        /**
         * Decodes an Anchor message from the specified reader or buffer.
         * @function decode
         * @memberof Messages.Anchor
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {Messages.Anchor} Anchor
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Anchor.decode = function decode(reader, length, error) {
            if (!(reader instanceof $Reader))
                reader = $Reader.create(reader);
            var end = length === undefined ? reader.len : reader.pos + length, message = new $root.Messages.Anchor();
            while (reader.pos < end) {
                var tag = reader.uint32();
                if (tag === error)
                    break;
                switch (tag >>> 3) {
                case 1: {
                        message.anchorID = reader.string();
                        break;
                    }
                case 2: {
                        if (!(message.position && message.position.length))
                            message.position = [];
                        if ((tag & 7) === 2) {
                            var end2 = reader.uint32() + reader.pos;
                            while (reader.pos < end2)
                                message.position.push(reader.float());
                        } else
                            message.position.push(reader.float());
                        break;
                    }
                default:
                    reader.skipType(tag & 7);
                    break;
                }
            }
            return message;
        };

        /**
         * Decodes an Anchor message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof Messages.Anchor
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {Messages.Anchor} Anchor
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Anchor.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader))
                reader = new $Reader(reader);
            return this.decode(reader, reader.uint32());
        };

        /**
         * Verifies an Anchor message.
         * @function verify
         * @memberof Messages.Anchor
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Anchor.verify = function verify(message) {
            if (typeof message !== "object" || message === null)
                return "object expected";
            if (message.anchorID != null && message.hasOwnProperty("anchorID"))
                if (!$util.isString(message.anchorID))
                    return "anchorID: string expected";
            if (message.position != null && message.hasOwnProperty("position")) {
                if (!Array.isArray(message.position))
                    return "position: array expected";
                for (var i = 0; i < message.position.length; ++i)
                    if (typeof message.position[i] !== "number")
                        return "position: number[] expected";
            }
            return null;
        };

        /**
         * Creates an Anchor message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof Messages.Anchor
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {Messages.Anchor} Anchor
         */
        Anchor.fromObject = function fromObject(object) {
            if (object instanceof $root.Messages.Anchor)
                return object;
            var message = new $root.Messages.Anchor();
            if (object.anchorID != null)
                message.anchorID = String(object.anchorID);
            if (object.position) {
                if (!Array.isArray(object.position))
                    throw TypeError(".Messages.Anchor.position: array expected");
                message.position = [];
                for (var i = 0; i < object.position.length; ++i)
                    message.position[i] = Number(object.position[i]);
            }
            return message;
        };

        /**
         * Creates a plain object from an Anchor message. Also converts values to other types if specified.
         * @function toObject
         * @memberof Messages.Anchor
         * @static
         * @param {Messages.Anchor} message Anchor
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Anchor.toObject = function toObject(message, options) {
            if (!options)
                options = {};
            var object = {};
            if (options.arrays || options.defaults)
                object.position = [];
            if (options.defaults)
                object.anchorID = "";
            if (message.anchorID != null && message.hasOwnProperty("anchorID"))
                object.anchorID = message.anchorID;
            if (message.position && message.position.length) {
                object.position = [];
                for (var j = 0; j < message.position.length; ++j)
                    object.position[j] = options.json && !isFinite(message.position[j]) ? String(message.position[j]) : message.position[j];
            }
            return object;
        };

        /**
         * Converts this Anchor to JSON.
         * @function toJSON
         * @memberof Messages.Anchor
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Anchor.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions);
        };

        /**
         * Gets the default type url for Anchor
         * @function getTypeUrl
         * @memberof Messages.Anchor
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Anchor.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com";
            }
            return typeUrlPrefix + "/Messages.Anchor";
        };

        return Anchor;
    })();

    Messages.Line = (function() {

        /**
         * Properties of a Line.
         * @memberof Messages
         * @interface ILine
         * @property {Array.<number>|null} [points] Line points
         * @property {number|null} [color] Line color
         * @property {number|null} [size] Line size
         */

        /**
         * Constructs a new Line.
         * @memberof Messages
         * @classdesc Represents a Line.
         * @implements ILine
         * @constructor
         * @param {Messages.ILine=} [properties] Properties to set
         */
        function Line(properties) {
            this.points = [];
            if (properties)
                for (var keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null)
                        this[keys[i]] = properties[keys[i]];
        }

        /**
         * Line points.
         * @member {Array.<number>} points
         * @memberof Messages.Line
         * @instance
         */
        Line.prototype.points = $util.emptyArray;

        /**
         * Line color.
         * @member {number} color
         * @memberof Messages.Line
         * @instance
         */
        Line.prototype.color = 0;

        /**
         * Line size.
         * @member {number} size
         * @memberof Messages.Line
         * @instance
         */
        Line.prototype.size = 0;

        /**
         * Creates a new Line instance using the specified properties.
         * @function create
         * @memberof Messages.Line
         * @static
         * @param {Messages.ILine=} [properties] Properties to set
         * @returns {Messages.Line} Line instance
         */
        Line.create = function create(properties) {
            return new Line(properties);
        };

        /**
         * Encodes the specified Line message. Does not implicitly {@link Messages.Line.verify|verify} messages.
         * @function encode
         * @memberof Messages.Line
         * @static
         * @param {Messages.ILine} message Line message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Line.encode = function encode(message, writer) {
            if (!writer)
                writer = $Writer.create();
            if (message.points != null && message.points.length) {
                writer.uint32(/* id 1, wireType 2 =*/10).fork();
                for (var i = 0; i < message.points.length; ++i)
                    writer.float(message.points[i]);
                writer.ldelim();
            }
            if (message.color != null && Object.hasOwnProperty.call(message, "color"))
                writer.uint32(/* id 2, wireType 0 =*/16).int32(message.color);
            if (message.size != null && Object.hasOwnProperty.call(message, "size"))
                writer.uint32(/* id 3, wireType 0 =*/24).int32(message.size);
            return writer;
        };

        /**
         * Encodes the specified Line message, length delimited. Does not implicitly {@link Messages.Line.verify|verify} messages.
         * @function encodeDelimited
         * @memberof Messages.Line
         * @static
         * @param {Messages.ILine} message Line message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Line.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim();
        };

        /**
         * Decodes a Line message from the specified reader or buffer.
         * @function decode
         * @memberof Messages.Line
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {Messages.Line} Line
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Line.decode = function decode(reader, length, error) {
            if (!(reader instanceof $Reader))
                reader = $Reader.create(reader);
            var end = length === undefined ? reader.len : reader.pos + length, message = new $root.Messages.Line();
            while (reader.pos < end) {
                var tag = reader.uint32();
                if (tag === error)
                    break;
                switch (tag >>> 3) {
                case 1: {
                        if (!(message.points && message.points.length))
                            message.points = [];
                        if ((tag & 7) === 2) {
                            var end2 = reader.uint32() + reader.pos;
                            while (reader.pos < end2)
                                message.points.push(reader.float());
                        } else
                            message.points.push(reader.float());
                        break;
                    }
                case 2: {
                        message.color = reader.int32();
                        break;
                    }
                case 3: {
                        message.size = reader.int32();
                        break;
                    }
                default:
                    reader.skipType(tag & 7);
                    break;
                }
            }
            return message;
        };

        /**
         * Decodes a Line message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof Messages.Line
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {Messages.Line} Line
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Line.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader))
                reader = new $Reader(reader);
            return this.decode(reader, reader.uint32());
        };

        /**
         * Verifies a Line message.
         * @function verify
         * @memberof Messages.Line
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Line.verify = function verify(message) {
            if (typeof message !== "object" || message === null)
                return "object expected";
            if (message.points != null && message.hasOwnProperty("points")) {
                if (!Array.isArray(message.points))
                    return "points: array expected";
                for (var i = 0; i < message.points.length; ++i)
                    if (typeof message.points[i] !== "number")
                        return "points: number[] expected";
            }
            if (message.color != null && message.hasOwnProperty("color"))
                if (!$util.isInteger(message.color))
                    return "color: integer expected";
            if (message.size != null && message.hasOwnProperty("size"))
                if (!$util.isInteger(message.size))
                    return "size: integer expected";
            return null;
        };

        /**
         * Creates a Line message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof Messages.Line
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {Messages.Line} Line
         */
        Line.fromObject = function fromObject(object) {
            if (object instanceof $root.Messages.Line)
                return object;
            var message = new $root.Messages.Line();
            if (object.points) {
                if (!Array.isArray(object.points))
                    throw TypeError(".Messages.Line.points: array expected");
                message.points = [];
                for (var i = 0; i < object.points.length; ++i)
                    message.points[i] = Number(object.points[i]);
            }
            if (object.color != null)
                message.color = object.color | 0;
            if (object.size != null)
                message.size = object.size | 0;
            return message;
        };

        /**
         * Creates a plain object from a Line message. Also converts values to other types if specified.
         * @function toObject
         * @memberof Messages.Line
         * @static
         * @param {Messages.Line} message Line
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Line.toObject = function toObject(message, options) {
            if (!options)
                options = {};
            var object = {};
            if (options.arrays || options.defaults)
                object.points = [];
            if (options.defaults) {
                object.color = 0;
                object.size = 0;
            }
            if (message.points && message.points.length) {
                object.points = [];
                for (var j = 0; j < message.points.length; ++j)
                    object.points[j] = options.json && !isFinite(message.points[j]) ? String(message.points[j]) : message.points[j];
            }
            if (message.color != null && message.hasOwnProperty("color"))
                object.color = message.color;
            if (message.size != null && message.hasOwnProperty("size"))
                object.size = message.size;
            return object;
        };

        /**
         * Converts this Line to JSON.
         * @function toJSON
         * @memberof Messages.Line
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Line.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions);
        };

        /**
         * Gets the default type url for Line
         * @function getTypeUrl
         * @memberof Messages.Line
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Line.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com";
            }
            return typeUrlPrefix + "/Messages.Line";
        };

        return Line;
    })();

    return Messages;
})();

module.exports = $root;
