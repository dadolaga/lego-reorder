import { BaseWebSocket, LegoPiece, LegoSet } from "@/utilities/type";
import { useSnackbar } from "notistack";
import { useCallback, useEffect, useState } from "react";

export function useAddNewLegoWS() {
    const [sendText, setSendTest] = useState<string>("");
    const [updateMessage, setUpdateMessage] = useState<string>("");
    const [checkPiece, setCheckPiece] = useState<LegoPiece[] | null>(null);
    const [websocket, setWebSocket] = useState<WebSocket | null>(null);
    const { enqueueSnackbar } = useSnackbar();

    useEffect(() => {
        if(sendText.length > 0) {
            if(websocket) {
                console.log(`Sending message: ${sendText}`);
                websocket.send(sendText);
            }

            console.error("Websocket is null, not able to send message");
        }
    }, [sendText, websocket])

    return {
        run: useCallback((lego: LegoSet) => {
            return new Promise((resolve, reject) => {
                let state: "INIT" | "CONNECT" | "ADD" | "ERROR" = "INIT";
                let uuid: string | null = null;
                const socket = new WebSocket("ws://localhost:5196/ws/AddNewLegoSet");
                setSendTest("");
                setWebSocket(() => socket);
                setUpdateMessage("Attendo che mi arrivi l'UUID");

                socket.onmessage = (message) => {
                    const rowData: string = message.data;
                    const data: BaseWebSocket<object> | null = rowData[0] == "{" ? JSON.parse(rowData) : null;

                    if (data && data.code >= 10) {
                        state = "ERROR";

                        console.log(data.message);
                        switch (data.code) {
                            case 11:
                                enqueueSnackbar("Set lego già presente", { variant: "error" });
                                reject(data.message);
                                socket.close();
                                break;
                            case 13:
                                setCheckPiece(data.data as LegoPiece[]);
                                break;
                            default:
                                enqueueSnackbar("Errore sconosciuto nell inserimento dei pezzi", { variant: "error" });
                                break;
                        }

                        return;
                    }

                    switch (state) {
                        case "INIT":
                            if (rowData[0] !== "{") {
                                state = "CONNECT";
                                uuid = rowData;

                                setUpdateMessage("Invio del set lego da agggiungere");

                                const value: BaseWebSocket<LegoSet> = {
                                    code: 1,
                                    message: "Lego set to add",
                                    data: lego
                                };

                                socket.send(JSON.stringify(value));
                            } else {
                                console.error(`State machine state unexpected, for first message i have received an invalid message ${rowData}`);
                            }
                            break;
                        case "CONNECT":
                            if(data?.code === 0) {
                                state = "ADD";

                                setUpdateMessage("Aggiunta dei pezzi lego del set");
                            }
                            break;
                    }
                }

                socket.onclose = () => {
                    console.log("Socket closed");
                    resolve("ok");
                };
            });
        }, []),
        updateMessage,
        checkPiece,
        sendMessage: setSendTest,
    }
}