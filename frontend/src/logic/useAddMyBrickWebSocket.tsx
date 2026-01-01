import { FilterOptions } from "@/utilities/request";
import { BaseWebSocket, LegoPiece, LegoSet } from "@/utilities/type";
import { useSnackbar } from "notistack";
import { useCallback, useEffect, useRef, useState } from "react";

export function useAddMyBrickWS() {
    const websocket = useRef<WebSocket | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [sendText, setSendTest] = useState<string>("");
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

        console.log("Send data", data, websocket.current);

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
            websocket.current = new WebSocket("ws://localhost:5196/ws/AddMyBrick");
            console.log("Web socket run");
            setSendTest("");
            setLoading(true);

            websocket.current.onmessage = (message) => {
                const rowData: string = message.data;
                const data: BaseWebSocket<object> | null = rowData[0] == "{" ? JSON.parse(rowData) : null;

                if (data) {
                    console.log("Received", data);

                    switch(data.code) {
                        case 10:
                            setSelectedPieceId(data.data as number[]);
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
            };
        }, [enqueueSnackbar]),
        loading,
        selectedPieceId,
        sendActive,
        sendDeactivate,
        sendMessage: setSendTest,
    }
}