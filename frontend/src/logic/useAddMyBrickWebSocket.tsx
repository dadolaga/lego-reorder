import { BaseWebSocket, LegoSet } from "@/utilities/type";
import createWebSocket from "@/utilities/webSocket";
import { useSnackbar } from "notistack";
import { useCallback, useEffect, useRef, useState } from "react";

type WebSocketState = "INIT" | "CONNECT" | "CLOSE" | "ERROR";

export interface SetPieceQuantityHave {
    pieceId: number,
    quantityHave: number,
}

export function useAddMyBrickWS() {
    const websocket = useRef<WebSocket | null>(null);
    const [state, setState] = useState<WebSocketState>("INIT");
    const [loading, setLoading] = useState<boolean>(false);
    const [sendText, setSendTest] = useState<string>("");
    const [piecesQuantityHave, setPiecesQuantityHave] = useState<SetPieceQuantityHave[] | null>(null);
    const [selectedPieceId, setSelectedPieceId] = useState<number[]>([]);

    const { enqueueSnackbar } = useSnackbar();

    useEffect(() => {
        if (sendText.length > 0) {
            if (websocket && websocket.current) {
                console.log(`Sending message: ${sendText}`);
                websocket.current.send(sendText);
            }

            console.error("Websocket is null, not able to send message");
        }
    }, [sendText])

    const sendActive = useCallback((pieceId: number) => {
        const data: BaseWebSocket<number> = {
            code: 10,
            message: "Update piece active",
            data: pieceId
        };

        websocket.current!.send(JSON.stringify(data));
    }, []);

    const sendPersonalQuantity = useCallback((pieceId: number, quantity: number) => {
        const data: BaseWebSocket<{ pieceId: number, quantity: number }> = {
            code: 11,
            message: "Update piece personal quantity",
            data: {
                pieceId,
                quantity
            }
        };

        websocket.current!.send(JSON.stringify(data));
    }, []);

    const sendDeactivate = useCallback((pieceId: number) => {
        const data: BaseWebSocket<number> = {
            code: 2,
            message: "Update piece deactivate",
            data: pieceId
        };

        websocket.current?.send(JSON.stringify(data));
    }, []);

    return {
        run: useCallback((lego: LegoSet) => {
            let uuid: string | null = null;
            websocket.current = createWebSocket("AddMyBrick");
            console.log("Web socket run");
            setSendTest("");
            setLoading(true);

            websocket.current.onopen = () => {
                setState("CONNECT");
            }

            websocket.current.onerror = () => {
                setState("ERROR");
            }

            websocket.current.onmessage = (message) => {
                const rowData: string = message.data;
                const data: BaseWebSocket<object> | null = rowData[0] == "{" ? JSON.parse(rowData) : null;

                if (data) {
                    console.log("Received", data);

                    switch (data.code) {
                        case 10:
                            setSelectedPieceId(data.data as number[]);
                            break;

                        case 20:
                            setPiecesQuantityHave(data.data as SetPieceQuantityHave[]);
                            break;

                        case 21:
                            setPiecesQuantityHave((values) => {
                                let found: boolean = false;
                                const updatedPiece = data.data as SetPieceQuantityHave;
                                const newArray = [...values!];

                                for(const piece of newArray) {
                                    if(piece.pieceId === updatedPiece.pieceId) {
                                        found = true;
                                        piece.quantityHave = updatedPiece.quantityHave;
                                    }
                                }

                                if(!found) {
                                    newArray.push(updatedPiece);
                                }
                                
                                return newArray;
                            });
                            break;

                        
                        default:
                            console.error(`Unknown code: ${data.code}, message: ${data.message}`);
                            break;
                    }

                    return;
                } else if (uuid == null) { // Received UUID
                    uuid = rowData;
                    setLoading(false);

                    console.log(`UUID received: ${uuid}`);

                    websocket.current?.send(JSON.stringify({
                        code: 1,
                        message: "Lego set to add",
                        data: lego
                    }));
                } else {
                    enqueueSnackbar(`Messaggio sconosciuto ricevuto: ${rowData}`, { variant: "warning" });
                    websocket.current?.close();
                }
            }

            websocket.current.onclose = () => {
                console.log("Socket closed");

                setState("CLOSE");
            };
        }, [enqueueSnackbar]),
        close: useCallback(() => {
            websocket.current?.close();
        }, []),
        loading,
        state,
        selectedPieceId,
        piecesQuantityHave,
        sendActive,
        sendPersonalQuantity,
        sendDeactivate,
        sendMessage: setSendTest,
    }
}