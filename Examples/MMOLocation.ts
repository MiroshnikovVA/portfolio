/// <reference path="RTSession.ts" />
/// <reference path="Interfaces.ts" />

RTSession.getLogger().debug("Start server 1.5.000003");

function printLog(obj) {
    debug(obj);
    RTSession.getLogger().debug(JSON.stringify(obj));
}

function debug(message) {
    var mes = JSON.stringify(message);
    var rtData = RTSession.newData().setString(1, mes);
    RTSession.newPacket().setOpCode(5).setReliable(true).setData(rtData).send();
}

function createCloadAPI() {
    var cload = {
        logResponseData: function (response, debugCaption) {
            var message = cload.serverName + "." + debugCaption;
            if (response.error != null) {
                message += "<Request error>";
                for (var key in response.error) {
                    message += "   <" + key + " = " + response.error[key] + ">";
                }
            }
            if (response.scriptData != null) {
                message += "<Request Data>";
                for (var key in response.scriptData) {
                    message += "    <" + key + " = " + response.scriptData[key] + ">";
                }
            }
            if (response.error == null)
                RTSession.getLogger().info(message);
            else
                RTSession.getLogger().error(message);
        },
        serverName: "realTimeServer" + Math.random(),
        log: function (response:any, debugCaption:string) {
            if (response.error != null) {
                cload.logResponseData(response, debugCaption)
            }
        },
        sendPlayerStateToCloud: function (playerId, connected: boolean) {
            var objSend = {
                message: "playerState",
                matchId: RTSession.getSessionId(),
                connected: true,
                playerId: playerId
            };

            RTSession.newRequest().createLogEventRequest().setEventKey("REAL_TIME_API")
                .setPlayerId(playerId)
                .setScriptData(objSend)
                .send(function (response) {
                    cload.log(response, "onSendPlayerStateRequest");
                });
        },
        getServerInfo: function (playerId: string, callback: (serverInfo: I.ServerInfo) => void) {
            var objSend = {
                message: "getServerInfo",
                matchId: RTSession.getSessionId()
            };
            RTSession.newRequest().createLogEventRequest().setEventKey("REAL_TIME_API")
                .setPlayerId(playerId)
                .setScriptData(objSend)
                .send(function (response) {
                    cload.log(response, "onGetServerInfoEventRequest");
                    var serverInfo = response.scriptData as I.ServerInfo;
                    cload.serverName = serverInfo.ServerName;
                    callback(serverInfo);
                });
        },
        getPlayerInfo: function (playerId, peerId, callback: (playerInfo: I.PlayerInfo) => void) {

            var objSend = {
                message: "getPlayerInfo",
                matchId: RTSession.getSessionId(),
                playerId: playerId
            };
            RTSession.newRequest().createLogEventRequest().setEventKey("REAL_TIME_API")
                .setPlayerId(playerId)
                .setScriptData(objSend)
                .send(function (response) {
                    cload.log(response, "onGetPlayerInfoEventRequest");
                    var playerInfo = response.scriptData as I.PlayerInfo;
                    playerInfo.playerId = playerId;
                    playerInfo.peerId = peerId;
                    playerInfo.SaveCharacter = () => {
                        cload.saveCharacter(playerInfo.playerId, playerInfo.character)
                    };
                    callback(playerInfo);
                });
        },
        saveCharacter: function (playerId, character: I.CharacterInfo) {
            var objSend = {
                message: "saveCharacter",
                matchId: RTSession.getSessionId(),
                playerId: playerId,
                character: character
            };
            RTSession.newRequest().createLogEventRequest().setEventKey("REAL_TIME_API")
                .setPlayerId(playerId)
                .setScriptData(objSend)
                .send(function (response) {
                    cload.log(response, "onsaveCharacterRequest");
                });
        }
    }
    return cload;
}


{ //ObjectModel-------------------------------------------

    var PlayerState = function (playerInfo: I.PlayerInfo) {
        var state = playerInfo;
        var playerState = {} as I.PlayerState;
        playerState.getPlayerName = function () {
            return state.playerName;
        }
        playerState.getPeerId = function () {
            return state.peerId;
        }
        playerState.getPlayerId = function () { return state.playerId; }
        playerState.getCharacter = function () { return state.character; }
        playerState.SaveCharacter = function () { state.SaveCharacter(); }
        return playerState;
    }

    var ServerState = function () {
        var serverState = {} as I.ServerState;
        var serverInfo = null;
        var Players = {};
        var DisconnectedPeers = {};

        serverState.getServerInfo = function () { return serverInfo; }
        serverState.setServerInfo = function (value) { serverInfo = value; }

        serverState.AddPlayer = function (peerId, playerInfo) {
            if (DisconnectedPeers[peerId] == null) {
                Players[playerInfo.peerId] = playerInfo;
                return true;
            } else {
                return false;
            }
        }

        serverState.GetPlayer = function (peerId) {
            return PlayerState(Players[peerId]);
        }

        serverState.RemovePlayer = function (peerId) {
            DisconnectedPeers[peerId as number] = true;
            var id = peerId;
            var pl = Players[id as number];
            if (pl == null) return null;
            delete Players[id as number];
            return PlayerState(pl);
        }

        return serverState;
    }

} //ObjectModel--------------------------------------------

function getServerInitManager() {
    var internal = {
        serverState : ServerState(),
        onConnectPlayer: null,
        onDisconnectPlayer: null,
        onServerStart: null,
        onSubscribePackets: null
    };

    var initializedServerStarted = false;
    var initializedSever = false;
    var firstPlayerId = null;

    var initServer = function (playerId) {
        cloadAPI.getServerInfo(playerId, function (serverInfo) {
            internal.serverState.setServerInfo(serverInfo);
            internal.onServerStart(internal.serverState);
            internal.onSubscribePackets();
            initOldPlayers();
            initializedSever = true;
        })
    }

    var initPlayer = function (playerId, peerId) {
        cloadAPI.getPlayerInfo(playerId, peerId, function (playerInfo) {
            server.getServerState().AddPlayer(peerId, playerInfo);
            internal.onConnectPlayer(PlayerState(playerInfo));
            cloadAPI.sendPlayerStateToCloud(playerId, true);
        });
    }

    var initOldPlayers = function () {
        var players = RTSession.getPlayers();
        for (var playerIndex in players) {
            var player = players[playerIndex];
            initPlayer(player.getPlayerId(), player.getPeerId());
        }
    }


    var server = {

        getServerState : function () {
            return internal.serverState;
        },

        onServerStart: (callback: (serverState: I.ServerState) => void) => {
            internal.onServerStart = function (serverState: I.ServerState) { callback(serverState); };
        },

        onSubscribePackets: (callback : () => void) => {
            internal.onSubscribePackets = () => { callback(); };
        },

        onConnectPlayer: (callback: (playerState : I.PlayerState) => void) => {
            internal.onConnectPlayer = (playerState: I.PlayerState) => { callback(playerState); };
        },

        onDisconnectPlayer: (callback: (playerState: I.PlayerState) => void) => {
            internal.onDisconnectPlayer = (playerState: I.PlayerState) => { callback(playerState); };
        },

        start : () => {
            RTSession.onPlayerConnect(function (player) {

                //debug("players.length " + RTSession.getPlayers().length);
                //debug("Login player " + player.getPeerId() + " " + player.getPlayerId());

                if (initializedSever) {
                    initPlayer(player.getPlayerId(), player.getPeerId())
                    return;
                }
                if (initializedServerStarted) {
                    return;
                }
                initializedServerStarted = true;
                initServer(player.getPlayerId());
            });
            RTSession.onPlayerDisconnect(function (player) {
                var state = server.getServerState();
                var plState = state.RemovePlayer(player.getPeerId());
                if (plState != null) {
                    internal.onDisconnectPlayer(plState);
                    cloadAPI.sendPlayerStateToCloud(player.getPlayerId(), false);
                }
            })
        }
    };
    return server;
}

function ServerRTGame() {

    //enums------------------------------------------------
    var PacketCode = {
        PlayerConnectOK: 1,
        NewObject: 2,
        Move: 10,
        Request: 3,
        Response: 4,
        Log: 5,
        RemoveObject: 6,
        TargetChanged: 7,
        MobControll: 8,
        RequestInteraction: 9,
        DrawInteraction: 11,
        ChangeStateOfStaticObject: 12,
        ReleaseFoci: 13,
        AddInventoryItemFromServer : 14,
        Dress: 15,
        ChangeInventory: 16,
        MoveInventoryItemRequest : 17,
    }

    var RequestType = {
        RuntimeObjectInfo: 1,
        RuntimeObjectDescription: 2,
        Lock: 3,
        InventoryChanged: 4
    }

    var ObjectType = {
        Entity: 0x1,
        StaticObject: 0x2,
        DynamicObject: 0x3,
        Mob: 0x4,
        Player: 0x5,
        DatabaseObject: 0x10
    }
    //enums------------------------------------------------

    //Fields-----------------------------------------------
    var initManager = getServerInitManager();
    var map = createMap();
    var Players = createDictionary<FakeRTSession.IPeerId, I.Player>();
    var Mobs = createDictionary<I.ObjectIndex, I.Mob>();
    /** All units in game */
    var Units = createUnitsTypesDic<I.Unit>();
    var RulesofGame = createRulesofGame();
    var Network = createClientsAPI();

    var Const = (function Constants() {
        var Constants: I.Constants = {
            ServerOwner: -1,
            AliveStaticObjectState: 1,
            DeadStaticObjectState: 0,
            EmtyInventoryItemType : -1
        }
        return Constants;
    })();

    var StaticObjects = createDictionary<I.ObjectIndex, I.StaticObject>();
    var InventoryItemTypes = createDictionary<I.ObjectIndex, I.InventoryItemType>();
    var WeaponsSystemTypes = createDictionary<I.ObjectIndex, I.WeaponSystemType>();
    //Fields-----------------------------------------------

    //functions--------------------------------------------
    function getUnit(type: I.ObjectType, id: I.ObjectIndex) {  return Units.GetObject(type, id);  }
    function Identifier(type: I.ObjectType, id: I.ObjectIndex) {
        var Identifier: I.Identifier = {
            type: type,
            id: id
        };
        return Identifier;
    }

    function log(message) {
        var mes = cloadAPI.serverName + ": " + message;
        RTSession.getLogger().debug(mes);
        debug(mes);
    }

    //functions--------------------------------------------

    //Constructors-----------------------------------------
    function createClientsAPI() {

        var network = {

            sendPos : function (unit, reliable, clients) {
                var sendData = RTSession.newData()
                    .setNumber(1, unit.type)
                    .setNumber(2, unit.id)
                    .setFloatArray(3, unit.fPos);
                RTSession.newPacket().setOpCode(PacketCode.Move).setReliable(reliable).setData(sendData).setTargetPeers(clients).send();
            },

            drawInteraction: function (unit: I.Unit, objectType: I.ObjectType,
                objectId: I.ObjectIndex, clients: [FakeRTSession.IPeerId]) {
                var sendData = RTSession.newData()
                    .setNumber(1, unit.type)
                    .setNumber(2, unit.id)
                    .setNumber(3, objectType)
                    .setNumber(4, objectId);
                RTSession.newPacket().setOpCode(PacketCode.DrawInteraction).setReliable(true).setData(sendData).setTargetPeers(clients).send();
            },

            sendObjectControll: function (isControled: boolean, playerId: FakeRTSession.IPeerId,
                objectType: I.ObjectType, objectId: I.ObjectIndex) {
                var sendData = RTSession.newData()
                    .setNumber(1, objectType)
                    .setNumber(2, objectId)
                    .setNumber(3, isControled ? 1 : 0);
                RTSession.newPacket().setOpCode(PacketCode.MobControll).setReliable(true).setData(sendData).setTargetPeers([playerId]).send();
                //[TODO!]if true need send inventar and other info....
            },

            sendRemoveObject: function (objectType: I.ObjectType, objectId: I.ObjectIndex,
                otherPlayerId: FakeRTSession.IPeerId) {
                var rtData = RTSession.newData()
                    .setNumber(1, objectType)
                    .setNumber(2, objectId);
                RTSession.newPacket().setOpCode(PacketCode.RemoveObject)
                    .setReliable(true).setData(rtData).setTargetPeers([otherPlayerId]).send();
            },

            sendCreateObject : function (objectType: I.ObjectType, objectId: I.ObjectIndex, otherPlayerId: FakeRTSession.IPeerId) {
                var unit = Units.GetObject(objectType, objectId);
                var characterName = "";
                var characterClass = "";
                var isOwner = (unit.owner == otherPlayerId) ? 1 : 0;

                if (objectType == ObjectType.Player) {
                    var player = unit.player;
                    var char = player.PlayerState.getCharacter();
                    characterName = char.CharacterName;
                    characterClass = char.CharacterData.CharacterClass;
                }
                else if (objectType == ObjectType.Mob) {
                    characterName = unit.mob.info.MobType + "[" + unit.pos.getMapCell().cx + "," + unit.pos.getMapCell().cy + "]";
                    characterClass = unit.mob.info.MobType as string;
                }

                //debug("unit.fPos " + unit.fPos);

                var rtData = RTSession.newData()
                    .setNumber(1, objectType as number)
                    .setNumber(2, objectId as number)
                    .setNumber(3, isOwner)
                    .setString(4, characterName)
                    .setString(5, characterClass)
                    .setFloatArray(6, unit.fPos);
                RTSession.newPacket().setOpCode(PacketCode.NewObject)
                    .setReliable(true).setData(rtData).setTargetPeers([otherPlayerId]).send();
                if (unit.Inventory != null) {
                    if (isOwner) {
                        network.Inventory.sendChangeInventory(unit, unit.Inventory.GetFullRecord(), [otherPlayerId], true, unit.Inventory.BagSize);
                    } else {
                        network.Inventory.sendDress(unit, unit.Inventory.GetFullDress(), [otherPlayerId], true);
                    }
                }
            },

            sendTargetChanged: function (who: I.Identifier, target: I.Identifier, clients: [FakeRTSession.IPeerId]) {
                var rtData = RTSession.newData()
                    .setNumber(1, who.type)
                    .setNumber(2, who.id)
                    .setNumber(3, target.type)
                    .setNumber(4, target.id);
                RTSession.newPacket().setOpCode(PacketCode.TargetChanged)
                    .setReliable(true).setData(rtData).setTargetPeers(clients).send();
            },

            sendChangeStateOfStaticObject: function (objectId: I.ObjectIndex, state: I.StaticObjectState, clients: [FakeRTSession.IPeerId]) {
                var rtData = RTSession.newData()
                    .setNumber(1, objectId)
                    .setNumber(2, state);
                var packet = RTSession.newPacket().setOpCode(PacketCode.ChangeStateOfStaticObject).setReliable(true).setData(rtData)
                if (clients != null) {
                    packet = packet.setTargetPeers(clients);
                }
                packet.send();
            },

            Inventory: (function createInventorySender() {
                var InventorySender = {
                    sendChangeInventory: function (unitId: I.Identifier, inventory: [I.InventoryItemRecord], clients: [FakeRTSession.IPeerId], full: boolean, bagSize: number) {
                        var rtData = RTSession.newData()
                            .setNumber(1, unitId.type)
                            .setNumber(2, unitId.id)
                            .setNumber(3, inventory.length)
                            .setNumber(4, full ? 1 : 0);
                        var k = 4;
                        for (var i = 0, l = inventory.length; i<l; i++) {
                            var item = inventory[i];
                            rtData = rtData.setNumber(++k, item.TypeId);
                            rtData = rtData.setNumber(++k, item.Slot);
                            if (item.TypeId != Const.EmtyInventoryItemType) {
                                rtData = rtData.setNumber(++k, item.Data.Count);
                            }
                        }
                        if (full) {
                            rtData = rtData.setNumber(++k, bagSize);
                        }
                        RTSession.newPacket().setOpCode(PacketCode.ChangeInventory)
                            .setReliable(true).setData(rtData).setTargetPeers(clients).send();
                    },
                    sendDress: function (unitId: I.Identifier, dress: [I.InventoryItemRecord], clients: [FakeRTSession.IPeerId], full: boolean) {
                        var rtData = RTSession.newData()
                            .setNumber(1, unitId.type)
                            .setNumber(2, unitId.id)
                            .setNumber(3, dress.length)
                            .setNumber(4, full?1:0);
                        var k = 4;
                        for (var i = 0, l = dress.length; i < l; i++) {
                            var item = dress[i];
                            rtData = rtData.setNumber(++k, item.TypeId);
                            rtData = rtData.setNumber(++k, item.Slot);
                        }
                        RTSession.newPacket().setOpCode(PacketCode.Dress)
                            .setReliable(true).setData(rtData).setTargetPeers(clients).send();
                    }
                };
                return InventorySender;
            })(),

            sendReleaseFoci: function (objectType: I.ObjectType, objectId: I.ObjectIndex, clients: [FakeRTSession.IPeerId]) {
                var rtData = RTSession.newData()
                    .setNumber(1, objectType)
                    .setNumber(2, objectId);
                RTSession.newPacket().setOpCode(PacketCode.ReleaseFoci)
                    .setReliable(true).setData(rtData).setTargetPeers(clients).send();
            }

        };
        return network;
    }

    function createDictionary<TKey extends Number, TValue>() {
        var dic = {};
        var Dictionary: Collections.IDictionary<TKey, TValue> = {
            Set : (key: TKey, value: TValue) => {
                dic[key as any] = value;
            },
            TryGet: (key: TKey) =>  {
                return dic[key as any] as TValue;
            },
            Remove: (key: TKey) =>  {
                var value = dic[key as any] as TValue;
                if (value != null) {
                    delete dic[key as any];
                    return value;
                }
                return null;
            },
            Foreach: (callback) => {
                for (var key in dic) { callback(key as any, dic[key]); }
            },
            Where: (callback) => {
                var newDic = createDictionary<TKey, TValue>();
                Dictionary.Foreach((key, value) => {
                    if (callback(key, value)) newDic.Set(key, value)
                });
                return newDic;
            },
            ToArray: function <TNewValue>(callback: (key: TKey, value: TValue) => TNewValue) {
                var array = [] as [TNewValue];
                Dictionary.Foreach((key, value) => {
                    array.push(callback(key, value));
                });
                return array;
            },
            FirstOrDefault: (callback): Collections.IKeyValue<TKey, TValue> => {
                for (var key in dic) {
                    var value = dic[key];
                    if (callback(key as any, value)) {
                        return { Key: key as any as TKey, Value: value as TValue};
                    }
                }
                return null;
            },
            FirstNullInRange: (startIndex, maxIndex) => {
                var i = startIndex as any as number;
                while (!(dic[i] == null)) {
                    if (i == maxIndex as any as number)
                        return null;
                    i++;
                }
                return i as any as TKey;
            }
        }
        return Dictionary;
    }

    function createUnit(objectType: I.ObjectType, id: I.ObjectIndex, ownerId: FakeRTSession.IPeerId) {

        var unit = {} as I.Unit;
        unit.player = null;
        unit.mob = null;
        /** owner id */
        unit.owner = ownerId;
        /** unit id */
        unit.id = id;
        /** unit type from enum ObjectType */
        unit.type = objectType;
        /** knowed units in nearest cells */
        unit.visibleUnits = createUnitsTypesDic<boolean>();
        /** unit position in map.*/
        unit.pos = map.newMapObject();
        unit.pos.onChangeCell(function (oldCel, newCel) {
            unitOnChangeCell(unit, oldCel, newCel);
        });
        /** Vector3f poition */
        unit.fPos = [0, 0, 0];
        /** move in Vector3f pos */
        unit.move = function (newPos) {
            var x = newPos[0];
            var y = newPos[2];
            unit.pos.move(x, y);
            unit.fPos = newPos;
            sendUnitPos(unit);
        }

        var firsMoveed = false;

        unit.destroy = function deleteUnit() {
            unit.pos.destroy();
            var _units = Units.TryDeleteObject(unit.type, unit.id);
        }

        function sendUnitPos(unit: I.Unit) {
            var clients = unit.getAllVisiblePlayers();
            if (clients.length > 0) {
                sendPos(unit, false, clients);
            }
        }

        function sendPos(unit, reliable, targets) {
            Network.sendPos(unit, reliable, targets);
        }

        unit.sendPos = function (targets) {
            sendPos(unit, true, targets);
        }

        unit.regularSendPos = function () {
            sendUnitPos(unit);
        }

        function unitOnChangeCell(unit: I.Unit, oldCel: I.HostMapCell, newCel: I.HostMapCell) {
            //log("unitOnChangeCell");

            (function oldCellProcessor() {
                if (oldCel != null) {
                    unit.visibleUnits.ForAllKeys(function (key) {
                        unit.visibleUnits.SetObject(key, false);
                    });
                    oldCel.units.TryDeleteObjectFromIdentifier(unit);
                }
            })();

            //logVisibleUnits(unit.id + " logvisibleUnits after oldCellProcessor()");

            (function newCellProcessor() {
                if (newCel != null) {
                    newCel.units.SetObject(unit, unit);
                    newCel.forAllNearestUnits(function (pl) {
                        var otherUnitHas = unit.visibleUnits.GetObjectFromIdentifier(pl);
                        if (otherUnitHas == null) {
                            SendRemoveOrNewPlayer(unit.type, unit.id, pl.type, pl.id, PacketCode.NewObject);
                            if (unit.type == ObjectType.Player) {
                                if (pl.owner != unit.id)
                                    pl.sendPos([unit.id]);
                            }
                            if ((pl.type == ObjectType.Player) && (unit != pl)) {
                                if (unit.owner != pl.id)
                                    unit.sendPos([pl.id]);
                            }
                        }
                        unit.visibleUnits.SetObject(pl, true);
                        pl.visibleUnits.SetObject(unit, true);
                    });
                }
            })();

            //logVisibleUnits(unit.id + "logvisibleUnits after newCellProcessor()");

            var removedArray = [] as [I.Identifier];
            (function setRemovedArray() {
                unit.visibleUnits.ForAllKeys(function (identificator) {
                    var vOther = unit.visibleUnits.GetObjectFromIdentifier(identificator);
                    if (!vOther) {
                        removedArray.push(identificator);
                    }
                });
            })();

            (function removeFromArray() {
                for (var i = 0, l = removedArray.length; i < l; i++) {
                    var identificator = removedArray[i];
                    var otherUnit = Units.GetObjectFromIdentifier(identificator);
                    if (otherUnit != null) {
                        otherUnit.visibleUnits.TryDeleteObjectFromIdentifier(unit);
                    }
                    unit.visibleUnits.TryDeleteObjectFromIdentifier(identificator);
                    SendRemoveOrNewPlayer(unit.type, unit.id, identificator.type, identificator.id, PacketCode.RemoveObject);
                    if (unit.type == ObjectType.Player) {
                        var controledMob = unit.player.ControlMobs.TryDeleteObjectFromIdentifier(identificator);
                        if (controledMob != null) {
                            controledMob.owner = Const.ServerOwner;
                            Network.sendObjectControll(false, unit.id, identificator.type, identificator.id);
                            //Отсылаем что удаленным персом больше игрок не руководит, хотя он и сам поймет, это лишнее наверное
                        }
                    }
                }
            })();

            //logVisibleUnits(unit.id + "logvisibleUnits after removeFromArray()");

            function SendRemoveOrNewPlayer(curentObjectType, curentId, otherObjectType, otherId, code) {
                if (otherObjectType == ObjectType.Player) {
                    if (code == PacketCode.RemoveObject) {
                        Network.sendRemoveObject(curentObjectType, curentId, otherId);
                    } else {
                        Network.sendCreateObject(curentObjectType, curentId, otherId);
                    }
                }

                if (curentObjectType == ObjectType.Player) {
                    if (curentId != otherId || curentObjectType != otherObjectType) {
                        if (code == PacketCode.RemoveObject) {
                            Network.sendRemoveObject(otherObjectType, otherId, curentId);
                        } else {
                            Network.sendCreateObject(otherObjectType, otherId, curentId);
                        }
                    }
                }
            }

            function logVisibleUnits(caption) {
                var s = caption;
                for (var objectType in unit.visibleUnits) {
                    var units = unit.visibleUnits[objectType];
                    if (units != null) {
                        for (var otherId in units) {
                            s += "[" + otherId + "]=" + units[otherId] + "; ";
                        }
                    }
                }
                debug(s);
            }
        }

        /** for all visible players with out this owner */
        unit.getAllVisiblePlayers = function () {
            var clients = [] as [FakeRTSession.IPeerId];
            unit.visibleUnits.ForAllTypeKeys(ObjectType.Player, function (plId) {
                var playerUnit = Units.GetObjectFromIdentifier(plId);
                var peerId = (playerUnit != null && playerUnit.player != null)
                    ? playerUnit.player.PlayerState.getPeerId()
                    : Const.ServerOwner;
                if (peerId != unit.owner && peerId != Const.ServerOwner && clients.indexOf(peerId) == -1) {
                    clients.push(peerId);
                }
            });
            return clients;
        }

        unit.isVisible = function (objectIdentifier) {
            if (objectIdentifier == null) return false;
            if (objectIdentifier.type == ObjectType.StaticObject) {
                var static = StaticObjects.TryGet(objectIdentifier.id);
                if (static == null) { return false };
                return static.isAlive;
            } else {
                var otherUnit = unit.visibleUnits.GetObjectFromIdentifier(objectIdentifier);
                return !(otherUnit == null);
            }
        }

        unit.trySetLock = function (targetIdentifier) {
            var who = unit;
            if (!who.isVisible(targetIdentifier)) {
                return false;
            }
            who.target = targetIdentifier;
            var clients = who.getAllVisiblePlayers();
            Network.sendTargetChanged(who, targetIdentifier, clients);
            return true;
        }

        Units.SetObject(unit, unit);
        return unit;
    }

    function createStaticObject(staticObjectInfo: I.StaticObjectInfo) {
        var obj = {} as I.StaticObject;
        obj.info = staticObjectInfo;
        obj.id = staticObjectInfo.ID
        obj.type = ObjectType.StaticObject;
        obj.isAlive = true;
        obj.state = Const.AliveStaticObjectState;

        obj.setState = function (newState) {
            obj.state = newState;
            Network.sendChangeStateOfStaticObject(obj.id, obj.state, null);
            if (obj.state <= 0) {
                obj.isAlive = false;
                //Network.sendReleaseFoci(ObjectType.StaticObject, obj.Id, []);
            } else {
                obj.isAlive = true;
            }
        }

        var b = false;
        if (obj.info.Resource != null && obj.info.Resource.type == ObjectType.DatabaseObject) {
            (function init() {
                var ResourceType = InventoryItemTypes.TryGet(obj.info.Resource.id);
                if (ResourceType != null) {
                    obj.resources = 5;
                    b = true;
                    obj.useFrom = function (unit) {
                        if (!obj.isAlive) {
                            if (unit.Inventory != null)
                                Network.Inventory.sendChangeInventory(unit,
                                    unit.Inventory.GetFullRecord(), [unit.owner], true, unit.Inventory.BagSize);
                            debug("unit " + unit.id + " try take resource in dead " + ResourceType.name);
                            return;
                        }
                        debug("unit " + unit.id + " take resource " + ResourceType.name);
                        obj.resources = obj.resources - 1;
                        if (obj.resources <= 0) obj.setState(Const.DeadStaticObjectState);
                        if (unit.Inventory != null) {
                            unit.Inventory.AddItemFromCount(ResourceType, 1);
                        }

                    }
                } else {
                    debug("databaseobject " + obj.info.Resource.id + "  is null");
                }
            })();
        }
        if (!b) {
            debug("printLog(obj.info)");
            obj.useFrom = function (unit) {
                debug("Unknowed used " + obj.info.ObjectType + " from peerId " + unit.owner);
            }
        }
        StaticObjects.Set(obj.id, obj);
        return obj;
    }

    function createInventoryItemType(inventoryItemType: I.InventoryItemType) {
        var obj = inventoryItemType;
        InventoryItemTypes.Set(obj.id.id, obj);
        return obj;
    }

    function createWeaponSystemType(weaponsSystemType) {
        var obj = weaponsSystemType;
        WeaponsSystemTypes.Set(obj.Id, obj);
        return obj;
    }

    function createPlayer(playerState: I.PlayerState) {
        var player = {} as I.Player;
        /** server state of player.*/
        player.PlayerState = playerState;
        Players.Set(playerState.getPeerId(), player);
        var unit = createUnit(ObjectType.Player, player.PlayerState.getPeerId(), player.PlayerState.getPeerId());
        unit.player = player;
        unit.owner = player.PlayerState.getPeerId();
        player.unit = unit;
        player.ControlMobs = createUnitsTypesDic<I.Unit>();
        var bagSize = player.PlayerState.getCharacter().CharacterData["BagSize"];
        if (bagSize == null) {
            bagSize = 32;
            player.PlayerState.getCharacter().CharacterData["BagSize"] = bagSize;
        }
        unit.Inventory = createInventory(bagSize);
        unit.Inventory.OnDressChanged(rec => {
            Network.Inventory.sendDress(unit, [rec], unit.getAllVisiblePlayers(), false);
        });
        unit.Inventory.OnBadRequest(() => {
            Network.Inventory.sendChangeInventory(unit, unit.Inventory.GetFullRecord(), [unit.owner], true, unit.Inventory.BagSize);
        });
        var char = player.PlayerState.getCharacter();
        var savedInv = char.CharacterData["Inventory"];
        if (savedInv != null)
            unit.Inventory.Load(savedInv);
        function setNewOwner(mobUnit: I.Unit, player: I.Player) {
            var oldOwner = mobUnit.owner;
            mobUnit.owner = player.PlayerState.getPeerId();
            player.ControlMobs.SetObject(mobUnit, mobUnit);
            Network.sendObjectControll(true, player.PlayerState.getPeerId(), mobUnit.type, mobUnit.id);

            var oldPlayer = Players.TryGet(oldOwner);
            if (oldPlayer != null) {
                oldPlayer.ControlMobs.TryDeleteObjectFromIdentifier(mobUnit);
                Network.sendObjectControll(false, oldOwner, mobUnit.type, mobUnit.id);
            }
        }

        var firsMoved = false;

        unit.pos.onMoved(function () {
            if (!firsMoved) { firsMoved = true; return;}
            unit.visibleUnits.ForAllKeys(function (key) {
                var mob = Units.GetObjectFromIdentifier(key);
                if (mob.owner == Const.ServerOwner) {
                    setNewOwner(mob, player);
                }
                else if (mob.owner != player.PlayerState.getPeerId()) {
                    var otherOwner = Players.TryGet(mob.owner);
                    if (otherOwner == null) {
                        setNewOwner(mob, player);
                    }
                    else if (otherOwner.unit != mob) {
                        if (unit.pos.SqrDistance(mob.pos) < otherOwner.unit.pos.SqrDistance(mob.pos)) {
                            setNewOwner(mob, player);
                        }
                    }
                }
            });
        })

        return player;
    }

    var mobNextId = 1;

    function createMob(mobInfo: I.MobInfo) {
        /** This is mob! . */
        var mob = {} as I.Mob;
        mob.info = mobInfo;
        var unit = createUnit(ObjectType.Mob, mobNextId++, Const.ServerOwner);
        unit.mob = mob;
        mob.unit = unit;
        Mobs.Set(mob.unit.id, mob);
        unit.move(mob.info.SpawnPoint);   
        //debug("create mob " + unit.id +" in ["+unit.pos.getMapCell().cx + "," + unit.pos.getMapCell().cy + "]");
        
        return mob;
    }

    function createMap() {
        var Map = {} as I.Map;

        var MiniCellCize = 5; //The size of a mini cell in meters, transfer buffer
        var bitInCell = 3; //8 miniCell in Cell
        const CellCize = MiniCellCize * (1 << bitInCell); // 5 * 1<<3 = 40 meters

        const MaxViewRange4CellSystem = CellCize / 2 - (MiniCellCize * 2); //  10 meters
        const MaxAtackRange4CellSystem = MaxViewRange4CellSystem * 0.5; // 5 meters
        const MaxViewRange9CellSystem = CellCize - (MiniCellCize * 2); // 40 - 2 * 5 = 30
        const MaxAtackRange9CellSystem = MaxViewRange9CellSystem * 0.5; // 15 meters

        /** all cells in map*/
        var cells = {};

        /** return {number} MiniCell coordinat from {number} float unity coordinate.*/
        Map.calcMapCoordinate = function (number) {
            return Math.round(number / MiniCellCize) + 16000; //only positive numbers in coordinate
        }

        /** return {number} CellCoordinate from MiniCell coordinate.*/
        Map.calcCellCoordinate = function (mapCoordinate) {
            return (mapCoordinate as number) >> bitInCell;
        }

        var getKey = function (cx: I.HostCellCoordinate, cy: I.HostCellCoordinate) { return ((cx as number) << 15) + (cy as number); }; //return cx + " " + cy;}//

        /** return or create {cell} from cell coordinate.*/
        Map.getCell = function (cx, cy) {
            var key = getKey(cx, cy);
            var cell = cells[key] as I.HostMapCell;
            //debug("getCell [" + cx + "," + cy + "] key = " + key + " cell = " + cell);
            if (cell == null) {
                /** unit hosting cell in Map. */
                cell = {} as I.HostMapCell;
                /** cell coordinate.*/
                cell.cx = cx;
                /** cell coordinate.*/
                cell.cy = cy;

                /** player characters in cell.*/
                cell.units = createUnitsTypesDic<I.Unit>();

                /** 9 nearest cells and this cell.*/
                cell.nearestCell = [cell];
                (function initNearestCell(thisCell: I.HostMapCell) {
                    //debug("initNearestCell from [" + thisCell.cx + "," + thisCell.cy + "]");
                    for (var x = (thisCell.cx as number) - 1; x <= (thisCell.cx as number) + 1; x++)
                        for (var y = (thisCell.cy as number) - 1; y <= (thisCell.cy as number) + 1; y++) {
                            var key = getKey(x, y);
                            var nc = cells[key];
                            if (nc != null) {
                                if (thisCell.nearestCell.indexOf(nc, 0) == -1) {
                                    thisCell.nearestCell.push(nc);
                                    if (nc != thisCell) {
                                        nc.nearestCell.push(thisCell);
                                    }
                                    //debug("new nearestCells = [" + thisCell.cx + "," + thisCell.cy + "] and ["+ nc.cx + "," + nc.cy + "]");
                                }
                            }
                            else {
                                //debug("new nearestCells = [" +x + "," + y + "] is null");
                            }
                        }


                })(cell);

                cell.forAllNearestUnits = function (callback) {
                    for (var ic = 0, lc = cell.nearestCell.length; ic < lc; ic++) {
                        var c = cell.nearestCell[ic];
                        c.units.ForAllObject(callback);
                    }
                }
                cells[key] = cell;
            }
            return cell;
        }

        /** return {MapObject} * @
        */
        Map.newMapObject = function () {
            var MapObject = {} as I.MapObject

            /** return {number} ViewDirectio -1 or 1 from MiniCell coordinate. */
            function getViewDirection(mapCoordinate) {
                var cellStartCoordinate = (mapCoordinate >> bitInCell) << bitInCell;
                var indexInCell = mapCoordinate - cellStartCoordinate; //0..7
                var midleIndexCell = 1 << (bitInCell - 1);
                return indexInCell < midleIndexCell ? -1 : 1;
            }

            function trySetCoordinate(floatX, floatY) {
                var newX = Map.calcMapCoordinate(floatX);
                var newY = Map.calcMapCoordinate(floatY);

                //var dx = (newX - x); 
                //var dy = (newY - y); 
                //if (dx*dx + dy*dy > 2) return; //teleport kick!
                if ((internal.x == newX) && (internal.y == newY)) {
                    return false;
                }

                //RTSession.getLogger().debug(internal.x + " " + newX + " " + (newX==internal.x)) ;

                /** MiniCell coordinate @*/
                internal.x = newX;
                /** MiniCell coordinate @*/
                internal.y = newY;

                //debug("object moved in [" +  internal.x + "," + internal.y + "]");
                return true;
            }

            function tryChangeCell(newCX, newCY, x, y) {
                var startCellX = internal.cx as number << bitInCell; //for example CX = 2, return 16
                var startCellY = internal.cy as number << bitInCell;
                var cellSize = 1 << bitInCell; //this is 8

                if ( //Find in rect, border = 1
                    (x < (startCellX - 1) ||
                        (x > (startCellX + cellSize)) ||
                        (y < (startCellY - 1)) ||
                        (y > (startCellY + cellSize))
                    )
                ) {
                    changeCell(newCX, newCY);
                    return true;
                }
                return false;
            }

            function changeCell(newCX, newCY) {

                //debug("cellchanged1 [" + internal.cx + ", " + internal.cy + "][" + newCX + "," + newCY + "]");

                /** Host-cell coordinate @*/
                internal.cx = newCX;
                /** Host-cell coordinate @*/
                internal.cy = newCY;

                var oldCell = internal.cell;
                var newCell = Map.getCell(internal.cx, internal.cy);

                /** hosting MapCell */
                internal.cell = newCell;

                //if (oldCell!=null) debug("cellchanged2 [" + oldCell.cx + ", " + oldCell.cy + "][" + newCell.cx + "," + newCell.cy + "]");

                if (oldCell != newCell)
                    if (events.onChangeCell != null)
                        events.onChangeCell(oldCell, newCell);
            }

            function changeViewDirection(newVx: number, newVy: number, cx: I.HostCellCoordinate, cy: I.HostCellCoordinate) {
                /** View Direction for 4-cell system @*/
                internal.vx = newVx;
                /** View Direction for 4-cell system @*/
                internal.vy = newVy;

                var oldCells = internal.viewedCells;
                var newCells = [
                    Map.getCell((cx as number) + newVx, cy),
                    Map.getCell((cx as number) + newVx, cy),
                    Map.getCell(cx, (cy as number) + newVy),
                    Map.getCell((cx as number) + newVx, (cy as number) + newVy)
                ] as [I.HostMapCell];

                /** 4 cells for ViewDirection @*/
                internal.viewedCells = newCells;

                if (events.onChangeViewedCells != null)
                    events.onChangeViewedCells(oldCells, newCells);
            }

            var events : any = {};
            

            events.onChangeCell = null;;
            events.onChangeViewedCells = null;
            events.onMoved = null;
            var internal = {
                token : Math.random(),
                cell : null as I.HostMapCell,
                cx :null as I.HostCellCoordinate,
                cy : null as I.HostCellCoordinate,
                viewedCells: [] as [I.HostMapCell],
                vx : null as number,
                vy : null as number,
                x : null as I.MapCoordinate,
                y : null as I.MapCoordinate,
                oldX: 0 as I.MapCoordinate,
                oldY: 0 as I.MapCoordinate
            };


            //trySetCoordinate(floatX, floatY);

            //changeCell(
            //    Map.calcCellCoordinate(internal.x), 
            //    Map.calcCellCoordinate(internal.y));

            //changeViewDirection(
            //    getViewDirection(internal.x), 
            //    getViewDirection(internal.y),
            //    internal.cx, 
            //    internal.cy);

            /** Moving object 
            * @param {number} x float coordinate from Unity Client
            * @param {number} y float coordinate from Unity Client
            * @
            */
            MapObject.move = function (floatX, floatY) {
                if (!trySetCoordinate(floatX, floatY)) {
                    return;
                }

                var cellChanged = false;
                //debug("floatX " + floatX + " floatY " + floatY);
                var newCX = Map.calcCellCoordinate(internal.x);
                var newCY = Map.calcCellCoordinate(internal.y);
                if ((newCX != internal.cx) || (newCY != internal.cy)) {
                    cellChanged = tryChangeCell(newCX, newCY, internal.x, internal.y);
                }

                var newVx = getViewDirection(internal.x);
                var newVy = getViewDirection(internal.y);
                if ((newVx != internal.vx) || (newVy != internal.vy)) {
                    changeViewDirection(newVx, newVy, newCX, newCY);
                }

                if (cellChanged || (Math.abs(internal.x as number - (internal.oldX as number)) > 1) || (Math.abs(internal.y as number - (internal.oldY as number)) > 1)) {
                    internal.oldX = internal.x;
                    internal.oldY = internal.y;
                    if (events.onMoved != null)
                        events.onMoved();
                }
            }

            MapObject.destroy = function () {
                var oldCell = internal.cell;
                internal.cell = null;

                //for (var key in events) { debug(key + " = " + events[key]); }

                if (events.onChangeCell != null) {
                    events.onChangeCell(oldCell, null);
                }
            }

            /** Event callback(oldCell, newCell) */
            MapObject.onChangeCell = function (callback) {
                events.onChangeCell = function (oldCell, newCell) { callback(oldCell, newCell); }
            }

            /** Event callback(oldCells, newCells) */
            MapObject.onChangeViewedCells = function (callback) {
                events.onChangeViewedCells = function (oldCells, newCells) { callback(oldCells, newCells); }
            }

            MapObject.onMoved = function (callback) {
                events.onMoved = function () { callback(); }
            }

            MapObject.SqrDistance = function (otherMapObject: I.MapObject) {
                var dx = internal.x as number - (otherMapObject.getX() as number);
                var dy = internal.y as number - (otherMapObject.getY() as number);
                return dx * dx + dy * dy;
            }

            MapObject.getX = function () { return internal.x; }
            MapObject.getY = function () { return internal.y; }

            /** return hosting MapCell */
            MapObject.getMapCell = function () { return internal.cell; };

            /** return array for 4 nearest viewed cells @*/
            MapObject.getViewedCells = function () { return internal.viewedCells; };

            return MapObject;
        }

        return Map;
    }

    function createUnitsTypesDic<TObject>() {
        var objectTypeDic = {} as I.ObjectTypeDictionary<TObject>;
        var dictionary = {};
        //dictionary[ObjectType.Player] = {};
        //dictionary[ObjectType.Mob] = {};
        objectTypeDic.GetObjectsDictionary = function (type: I.ObjectType) {
            return dictionary[type as number];
        }
        objectTypeDic.GetObject = function (type: I.ObjectType, index: I.ObjectIndex) {
            var dic = objectTypeDic.GetObjectsDictionary(type);
            if (dic == null) return null;
            return dic[index as number];
        }
        objectTypeDic.GetObjectFromIdentifier = function(identifier) {
            return objectTypeDic.GetObject(identifier.type, identifier.id);
        }
        objectTypeDic.TryDeleteObjectFromIdentifier = function (identifier) {
            return objectTypeDic.TryDeleteObject(identifier.type, identifier.id);
        }
        objectTypeDic.TryDeleteObject = function (type: I.ObjectType, index: I.ObjectIndex) {
            var dic = objectTypeDic.GetObjectsDictionary(type);
            if (dic == null) return null;
            var unit = dic[index as number];
            if (unit != null) delete dic[index as number];
            return unit;
        }
        objectTypeDic.SetObject = function (key: I.Identifier, object: TObject ) {
            if (object == null) log("Error! ObjectTypeDic.SetObject null, key = " + key);
            var dic = dictionary[key.type as number];
            if (dic == null) dictionary[key.type as number] = {};
            dictionary[key.type as number][key.id as number] = object;
        }
        objectTypeDic.ForAllKeys = function (callback) {
            for (var k1 in dictionary) {
                var dic = dictionary[k1];
                for (var k2 in dic) {
                    var key: I.Identifier = {
                        type: k1 as any as I.ObjectType,
                        id: k2 as any as I.ObjectIndex
                    }
                    callback(key);
                }
            }
        };
        objectTypeDic.ForAllObject = function (callback) {
            objectTypeDic.ForAllKeys(function (key) {
                callback(dictionary[key.type as number][key.id as number] as TObject);
            });
        }
        objectTypeDic.ForAllTypeObject = function (type,callback) {
            var dic = dictionary[type as number];
            for (var k2 in dic) {
                callback(dic[k2]);
            }
        }
        objectTypeDic.ForAllTypeKeys = function (type, callback) {
            var dic = dictionary[type as number];
            for (var k2 in dic) {
                var key: I.Identifier = {
                    type: type as any as I.ObjectType,
                    id: k2 as any as I.ObjectIndex
                }
                callback(key);
            }
        }
        return objectTypeDic;
    }

    function createRulesofGame() {
        var InventorySlot = {
            None: 0x0,
            Inventory: 0x0,
            Head: 0x1,
            Body: 0x2,
            Pants: 0x3,
            Feet: 0x4,
            Primary: 0x5,
            Secondary: 0x6,
            Bag: 0x100,
            FastAccess: 0x4000,
            //CurrentInventory = 0xFFFF,
            NextInventory: 0x10000,
            Thrown: 0x10000000
        }


        var RulesofGame: I.RulesofGame = {
            InventoryRules: {
                IsDressSlot: (slot) => slot >= InventorySlot.Head && slot <= InventorySlot.Secondary,
                IsSavedSlot: (slot) => slot < InventorySlot.Thrown,
                IsThrownSlot: (slot) => slot >= InventorySlot.Thrown,
                IsBagSlot: (slot) => slot >= InventorySlot.Bag && slot < InventorySlot.FastAccess,
                StartBagSlot: InventorySlot.Bag,
                IsRelocatableSlot: (slot) => slot >= InventorySlot.Head && slot <= InventorySlot.Thrown
            }
        };
        return RulesofGame;
    }

    function createInventory(bagSize: number) {
        function createInventoryItemRecord(slot: I.InventorySlot, item: I.InventoryItem): I.InventoryItemRecord {
            var record;
            if (item != null)
                record = createInventoryItemRecord2(slot, item.Type.id.id, item.Data);
            else
                record = createInventoryItemRecord2(slot, Const.EmtyInventoryItemType, null);
            return record;
        }
        function createInventoryItemRecord2(slot: I.InventorySlot, typeId: I.ObjectIndex, data: I.InventoryItemData): I.InventoryItemRecord  {
            var InventoryItemRecord: I.InventoryItemRecord = {
                Slot: slot,
                TypeId: typeId,
                Data: data
            };
            return InventoryItemRecord;
        }
        function createInventoryItem(rec: I.InventoryItemRecord) : I.InventoryItem {
            if (rec.TypeId == Const.EmtyInventoryItemType)
                return null;
            var type = InventoryItemTypes.TryGet(rec.TypeId);
            if (type == null) {
                log("Not found InventoryItemType from id " + rec.TypeId);
                return null;
            }
            var InventoryItem: I.InventoryItem = {
                Type: type,
                Data: rec.Data
            };
            return InventoryItem;
        }
        function createInventoryItemFromLoot(loot: I.InventoryItem): I.InventoryItem {
            var newData = JSON.parse(JSON.stringify(loot.Data)) as I.InventoryItemData;
            var maxStack = loot.Type.maxStack;
            if (loot.Data.Count > maxStack) {
                loot.Data.Count -= maxStack;
                newData.Count = maxStack;
            }
            else {
                loot.Data.Count = 0;
            }
            var InventoryItem: I.InventoryItem = {
                Type: loot.Type,
                Data: newData
            };
            return InventoryItem;
        }
        function createInventoryItemFromTypeAndCount(type: I.InventoryItemType, count: number) {
            var item: I.InventoryItem = {
                Data : {
                    Count: count
                } as I.InventoryItemData,
                Type : type
            };
            return item;
        }
        function createInventoryItemFromTypeAndData(type: I.InventoryItemType, data: I.InventoryItemData) {
            var item: I.InventoryItem = {
                Data: data,
                Type: type
            };
            return item;
        }


        var OnDressChangedEventHandler = null;
        var OnBadRequestEventHandler = null;
        var OnDressChanged = function (slot: I.InventorySlot, item: I.InventoryItem) {
            if (OnDressChangedEventHandler!=null)
                OnDressChangedEventHandler(createInventoryItemRecord(slot, item));
        }
        var OnBadRequest = function () {
            if (OnBadRequestEventHandler!=null)
                OnBadRequestEventHandler();
        }
        var AddItemAndReturnLootItemInOldSlot = function (lootItem: I.InventoryItem): I.InventoryItem  {
            var maxStack = lootItem.Type.maxStack;
            if (maxStack > 1 && lootItem.Data.Count < maxStack) { //Если стыкуемый, то ищем неполный
                var firstThinSlot = Inventory.FullInventory.FirstOrDefault((k, v) =>
                    RulesofGame.InventoryRules.IsBagSlot(k)
                    && v.Type == lootItem.Type
                    && v.Data.Count < maxStack);
                if (firstThinSlot != null) {
                    var capacity = maxStack - firstThinSlot.Value.Data.Count;
                    if (capacity >= lootItem.Data.Count) {
                        firstThinSlot.Value.Data.Count += lootItem.Data.Count;
                        return null;
                    } else {
                        firstThinSlot.Value.Data.Count = maxStack;
                        lootItem.Data.Count -= capacity;
                        return AddItemAndReturnLootItemInOldSlot(lootItem);
                    }
                }
            }

            var emtySlot = Inventory.FullInventory.FirstNullInRange(
                RulesofGame.InventoryRules.StartBagSlot,
                RulesofGame.InventoryRules.StartBagSlot as number + Inventory.BagSize);
            if (emtySlot != null && RulesofGame.InventoryRules.IsBagSlot(emtySlot)) {
                Inventory.FullInventory.Set(emtySlot, createInventoryItemFromLoot(lootItem));
                if (lootItem.Data.Count == 0) {
                    return null;
                }
                else {
                    AddItemAndReturnLootItemInOldSlot(lootItem);
                }
            }
            return null;
        }


        var Inventory: I.Inventory = {
            AddItemFromCount: (type, count) => {
                var sourceItem = createInventoryItemFromTypeAndCount(type, count);
                var residue = AddItemAndReturnLootItemInOldSlot(sourceItem);
            },
            AddItem: (type, data) => {
                var sourceItem = createInventoryItemFromTypeAndData(type, data);
                var residue = AddItemAndReturnLootItemInOldSlot(sourceItem);
            },
            MoveInventoryItemRequest: (oldSlot, newSlot) => {
                if (!RulesofGame.InventoryRules.IsRelocatableSlot(oldSlot)
                    || !RulesofGame.InventoryRules.IsRelocatableSlot(newSlot)) {
                    OnBadRequest();
                    return;
                }
                var oldItem = Inventory.FullInventory.TryGet(oldSlot);
                var newItem = Inventory.FullInventory.TryGet(newSlot);
                if (oldItem == null) {
                    OnBadRequest();
                    return;
                }

                if (newItem != null && oldItem.Type == newItem.Type && newItem.Type.maxStack > newItem.Data.Count) {
                    //Стакаем слоты
                    newItem.Data.Count += oldItem.Data.Count;
                    if (newItem.Data.Count > newItem.Type.maxStack) {
                        oldItem.Data.Count = newItem.Data.Count - newItem.Type.maxStack;
                    } else {
                        Inventory.FullInventory.Remove(oldSlot);
                        if (RulesofGame.InventoryRules.IsDressSlot(oldSlot)) OnDressChanged(oldSlot, null);
                    }
                } else {
                    //Обмениваем местами
                    Inventory.FullInventory.Set(newSlot, oldItem);
                    if (RulesofGame.InventoryRules.IsDressSlot(newSlot)) OnDressChanged(newSlot, oldItem);
                    if (newItem == null) {
                        Inventory.FullInventory.Remove(oldSlot);
                        if (RulesofGame.InventoryRules.IsDressSlot(oldSlot)) OnDressChanged(oldSlot, null);
                    } else {
                        Inventory.FullInventory.Set(oldSlot, newItem);
                        if (RulesofGame.InventoryRules.IsDressSlot(oldSlot)) OnDressChanged(oldSlot, newItem);
                    }  
                }
            },
            PickupInventoryItemRequest: (slot) => {
                if (!RulesofGame.InventoryRules.IsThrownSlot(slot)) {
                    OnBadRequest();
                    return;
                }
                var lootItem = Inventory.FullInventory.TryGet(slot);
                if (lootItem == null) {
                    OnBadRequest();
                    return;
                }
                Inventory.FullInventory.Set(slot, AddItemAndReturnLootItemInOldSlot(lootItem));
            },
            AddItemRequest: (type, count) => {
                //ХЗ что тут делать? Клиент не может сам добавить что-либо!
                //Потом сделаю очередь, и в ней будет проверка соответствия
            },
            FullInventory: createDictionary<I.InventorySlot, I.InventoryItem>(),
            OnDressChanged: (callback) => OnDressChangedEventHandler = callback,
            OnBadRequest: (callback) => OnBadRequestEventHandler = callback,
            GetFullDress: () => {
                return Inventory.FullInventory
                    .Where((k, v) => RulesofGame.InventoryRules.IsDressSlot(k))
                    .ToArray((k, v) => createInventoryItemRecord(k, v));
            },
            Load: (array) => {
                Inventory.FullInventory = createDictionary<I.InventorySlot, I.InventoryItem>();
                //for (var i = 0, l = array.length; i < l; i++) {
                for (var i in array) {
                    var rec = array[i];
                    Inventory.FullInventory.Set(rec.Slot, createInventoryItem(rec));
                }

            },
            Save: () => {
                var val = Inventory.FullInventory
                    .Where((k, v) => RulesofGame.InventoryRules.IsSavedSlot(k))
                    .ToArray((k, v) => createInventoryItemRecord(k, v));
                return val;
            },
            GetFullRecord: () => {
                return Inventory.FullInventory
                    .ToArray((k, v) => createInventoryItemRecord(k, v));
            },
            BagSize: bagSize
        };
        return Inventory;
    }
    //Constructors-----------------------------------------

    initManager.onSubscribePackets(function () {

        RTSession.onPacket(PacketCode.MoveInventoryItemRequest, function (packet) {
            var type = packet.getData().getNumber(1);
            var index = packet.getData().getNumber(2);
            var oldSlot = packet.getData().getNumber(3);
            var newSlot = packet.getData().getNumber(4);
            var unit = getUnit(type, index);
            if (unit != null && unit.owner == packet.getSender().getPeerId()) {
                var Inventory = unit.Inventory;
                if (Inventory != null) {
                    Inventory.MoveInventoryItemRequest(oldSlot, newSlot);
                }
            }
        });

        RTSession.onPacket(PacketCode.Move, function (packet) {
            var type = packet.getData().getNumber(1);
            var index = packet.getData().getNumber(2);
            var charPos = packet.getData().getFloatArray(3);
            var unit = getUnit(type, index);
            if (unit != null && unit.owner == packet.getSender().getPeerId()) {
                try {
                    unit.move(charPos);
                }
                catch (exception) {
                    log(exception.name + " " + exception.message);
                }
            }
        });

        RTSession.onPacket(PacketCode.Request, function (packet) {
            var requestId = packet.getData().getNumber(1);
            var requestType = packet.getData().getNumber(2);
            function data() {
                var rtData = RTSession.newData()
                    .setNumber(1, requestId);
                return rtData;
            }

            function send(sendData) {
                RTSession.newPacket().setOpCode(PacketCode.Response).setReliable(true).setData(sendData).setTargetPeers([packet.getSender().getPeerId()]).send();
            }


            var sendData = null;
            if (requestType == RequestType.RuntimeObjectInfo) {
                (function () {
                    var type = packet.getData().getNumber(3);
                    var id = packet.getData().getNumber(4);

                    var unit = getUnit(type, id);
                    send(data()
                        .setNumber(2, unit.owner == packet.getSender().getPeerId() ? 1 : 0)
                    );
                })();
            }
            else if (requestType == RequestType.RuntimeObjectDescription) {
                (function () {
                    var type = packet.getData().getNumber(3);
                    var id = packet.getData().getNumber(4);
                    var unit = getUnit(type, id);

                    if (type == ObjectType.Player) {
                        var player = Players.TryGet(id);
                        var char = player.PlayerState.getCharacter();
                        var characterName = char.CharacterName;
                        var characterClass = char.CharacterData.CharacterClass;
                        send(data()
                            .setString(2, characterName)
                            .setString(3, characterClass)
                        );
                    }
                    else if (type == ObjectType.Mob) {
                        var mobName = unit.mob.info.MobType + "[" + unit.pos.getMapCell().cx + "," + unit.pos.getMapCell().cy + "]";
                        var mobClass = unit.mob.info.MobType as string;
                        send(data()
                            .setString(2, mobName)
                            .setString(3, mobClass)
                        );
                    }

                    if (unit.owner != packet.getSender().getPeerId()) {
                        unit.sendPos([packet.getSender().getPeerId()]);
                    }
                })();
            }
            else if (requestType == RequestType.Lock) {
                (function () {
                    var type = packet.getData().getNumber(3);
                    var id = packet.getData().getNumber(4);
                    var who = getUnit(type, id);

                    var targetType = packet.getData().getNumber(5);
                    var targetid = packet.getData().getNumber(6);

                    if (who.owner == packet.getSender().getPeerId()) {
                        var rez = who.trySetLock(Identifier(targetType, targetid));
                        send(data()
                            .setNumber(2, rez ? 1 : 0)
                            .setNumber(3, who.target.type)
                            .setNumber(4, who.target.id)
                        );
                    }
                })();
            }
        });

        RTSession.onPacket(PacketCode.RequestInteraction, function (packet) {
            var unitType = packet.getData().getNumber(1);
            var unitIndex = packet.getData().getNumber(2);
            var targetType = packet.getData().getNumber(3);
            var targetIndex = packet.getData().getNumber(4);
            var unit = getUnit(unitType, unitIndex);
            if (unit == null) return;
            if (unit.owner == packet.getSender().getPeerId()) {
                if (targetType == ObjectType.StaticObject) {
                    var obj = StaticObjects.TryGet(targetIndex);
                    if (obj != null) obj.useFrom(unit);
                } else {
                    var targetUnit = getUnit(targetType, targetIndex);
                    if (targetUnit != null) {
                        var useFrom = targetUnit["useFrom"];
                        if (useFrom != null) targetUnit.useFrom(unit);
                    }
                }
                var clients = unit.getAllVisiblePlayers();
                Network.drawInteraction(unit, targetType, targetIndex, clients);
            }


        });
    });

    (function initServer() {

        initManager.onServerStart(function (serverStat:I.ServerState) {
            log("onServerStart");
            var _log = "onServerInit :";
            var complete = false;
            try {
                _log += "[ weaponSystem";
                var weaponSystemItems = serverStat.getServerInfo().WeaponSystemTypeData;
                for (var i = 0, l = weaponSystemItems.length; i < l; i++) {
                    var weaponSystemType = createWeaponSystemType(weaponSystemItems[i]);
                }
                _log += " - Ok] ";

                _log += "[ inventoryItems";
                var inventoryItems = serverStat.getServerInfo().InventoryItemsData;
                for (var i = 0, l = inventoryItems.length; i < l; i++) {
                    var inventoryItem = createInventoryItemType(inventoryItems[i]);
                }
                _log += " - Ok] ";

                if (serverStat.getServerInfo().LocationData != null) {
                    _log += "[ StaticObjects";
                    var list = serverStat.getServerInfo().LocationData.StaticObjects;
                    for (var i = 0, l = list.length; i < l; i++) {
                        var staticObject = createStaticObject(list[i]);
                    }
                    _log += " - Ok] ";

                    _log += "[ Mobs";
                    var mobs = serverStat.getServerInfo().LocationData.Mobs;
                    for (var i = 0, l = mobs.length; i < l; i++) {
                        var mob = createMob(mobs[i]);
                    }
                    _log += " - Ok] ";
                }
                complete = true;
            }
            catch (exception) {
                _log += " - Exception: " + exception.name + " - '" + exception.message + "' ]";
            }
            log(_log);
            
        });

        initManager.onConnectPlayer(function (playerStat: I.PlayerState) {
            log("onConnectPlayer");
            createPlayer(playerStat);
            RTSession.newPacket().setOpCode(PacketCode.PlayerConnectOK).setTargetPeers([playerStat.getPeerId()]).send();
            StaticObjects.Foreach((id, so) => {
                if (so.state != 1) {
                    Network.sendChangeStateOfStaticObject(id, so.state, [playerStat.getPeerId()]);
                }
            });
            //print("connectPlayer: " + playerStat.getPlayerName() + " character: " + playerStat.getCharacter().getName());
        });

        initManager.onDisconnectPlayer(function (playerStat: I.PlayerState) {
            var unit = getUnit(ObjectType.Player, playerStat.getPeerId());
            if (unit != null) {
                log("onDisconnectPlayer " + playerStat.getPlayerName());
                if (unit.Inventory != null) {
                    playerStat.getCharacter().CharacterData.Inventory = unit.Inventory.Save();
                    //Добавить сюда прочее параметры для сохранения
                    playerStat.SaveCharacter();
                }
                unit.destroy();
                Players.Remove(playerStat.getPeerId());
            }
            else {
                log("diconnected player is null");
            }
            //print("disconnectPlayer: "+ playerStat.getPlayerName() + " character: " + playerStat.getCharacter().getName());
        });

        initManager.start();
    })();
}

var cloadAPI = createCloadAPI();
ServerRTGame();