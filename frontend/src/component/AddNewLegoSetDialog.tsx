import { searchLego } from "@/utilities/request";
import { BaseWebSocket, LegoPiece, LegoSet } from "@/utilities/type";
import { Icon } from "@iconify/react";
import { Box, Button, Card, CardContent, CardMedia, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Grid, IconButton, TextField, Typography, useTheme } from "@mui/material";
import { ChangeEvent, useCallback, useState, memo, useRef, useEffect } from "react";
import LegoCard, { IButtonList } from "./homepage/LegoCard";
import { useAddNewLegoWS } from "@/logic/useAddNewLegoWebSocket";
import { useSnackbar } from "notistack";

interface Iprops {
    open: boolean,
    hide: (save: boolean) => void,
}

interface ILegoSetGridProps {
    sets: LegoSet[];
    buttonList: IButtonList[];
}

const LegoSetGrid = memo(({ sets, buttonList }: ILegoSetGridProps) => {
    return (
        <Grid container spacing={2} sx={{ overflowY: "auto" }}>
            {sets.map(set => (
                <Grid key={set.apiId} size={6}>
                    <LegoCard
                        legoSet={set}
                        buttonList={buttonList}
                    />
                </Grid>
            ))}
        </Grid>
    );
});
LegoSetGrid.displayName = 'LegoSetGrid';

export default function AddNewLegoSetDialog({
    open,
    hide
}: Iprops) {
    const centerFlex: React.CSSProperties = { display: "flex", justifyContent: "center", alignItems: "center" };

    const theme = useTheme();

    const { run: addNewLegoSet, updateMessage, checkPiece, sendMessage } = useAddNewLegoWS();
    const [openEditPieceDialog, setOpenEditPieceDialog] = useState<boolean>(false);
    const [loading, setLoading] = useState<boolean>(false);
    const [loadingMessage, setLoadingMessage] = useState<string>("");
    const [legoSearch, setLegoSearch] = useState<string>("");
    const [legoSets, setLegoSets] = useState<LegoSet[]>([]);

    const legoSetRef = useRef<LegoSet[]>(legoSets);

    const [buttonList,] = useState<IButtonList[]>([{
        name: "Add",
        onClick: (legoSet: LegoSet) => () => {
            const set = legoSetRef.current.find(set => set.apiId === legoSet.apiId);

            setLoading(true);

            addNewLegoSet(set!).finally(() => {
                setLoading(false);
            });
        }
    }]);

    useEffect(() => {
        legoSetRef.current = legoSets;
    }, [legoSets]);

    useEffect(() => {
        setLoadingMessage(updateMessage);
    }, [updateMessage]);

    useEffect(() => {
        if (checkPiece) {
            setOpenEditPieceDialog(true);
        }
    }, [checkPiece]);

    const editValueHandler = useCallback((event: ChangeEvent<HTMLInputElement>) => {
        setLegoSearch(event.target.value);
    }, []);

    const searchLegoHandler = useCallback(() => {
        setLoading(true);
        setLegoSets([]);

        searchLego(legoSearch).then((lego) => {
            setLegoSets(lego!);
        }).finally(() => {
            setLoading(false);
        });
    }, [legoSearch]);

    const closeDialog = () => {
        hide(false);
    };

    const sendEditPieceHandler = (text: string) => {
        setOpenEditPieceDialog(false);
        sendMessage(text);
    }

    return (
        <Dialog fullWidth maxWidth="md" open={open} onClose={closeDialog} slotProps={{ paper: { sx: { height: "100%" } } }}>
            {openEditPieceDialog && checkPiece && (
                <LegoPieceCorrectorDialog sendMessage={sendEditPieceHandler} pieces={checkPiece} />
            )}
            <DialogTitle>Aggiungi nuovo set Lego</DialogTitle>
            <DialogContent sx={{ boxSizing: "border-box" }}>
                <Box pt={1} display="flex" flexDirection="column" gap={3} height="100%" boxSizing="border-box">
                    <Box display="flex" gap={1}>
                        <TextField fullWidth size="small" label="Cerca..." value={legoSearch} onChange={editValueHandler} />
                        <IconButton onClick={searchLegoHandler}>
                            <Icon icon="material-symbols:search" />
                        </IconButton>
                    </Box>
                    <Box sx={{ position: "relative", height: "100%", width: "100%", overflow: "auto", ...((legoSets.length === 0 || loading) ? centerFlex : {}) }}>
                        {legoSets.length === 0 && !loading && (<p>Nessun risultato trovato</p>)}
                        {legoSets.length === 0 && loading && (<CircularProgress />)}
                        {legoSets.length !== 0 && (
                            <Box mx={1.25}>
                                <LegoSetGrid sets={legoSets} buttonList={buttonList} />
                                {loading && (
                                    <Box sx={{ backgroundColor: `${theme.palette.background.paper}60`, borderRadius: 1, flexDirection: "column", gap: 2, ...centerFlex }} position="absolute" top="0" left="0" width="100%" height="100%">
                                        <CircularProgress />
                                        {loadingMessage && <Typography>{loadingMessage}</Typography>}
                                    </Box>
                                )}
                            </Box>
                        )}
                    </Box>
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={closeDialog} variant="text">Chiudi</Button>
            </DialogActions>
        </Dialog >
    );
}

interface LegoPieceCorrectorProps {
    sendMessage: (text: string) => void,
    pieces: LegoPiece[],
}

function LegoPieceCorrectorDialog({
    sendMessage,
    pieces,
}: LegoPieceCorrectorProps) {
    const { enqueueSnackbar } = useSnackbar();
    const [pieceLegoCode, setPieceLegoCode] = useState<string[]>([]);

    useEffect(() => {
        setPieceLegoCode(pieces.map(piece => piece.legoId || ""));
    }, [pieces])

    const changeValueHandler = useCallback((index: number) => (event: ChangeEvent<HTMLInputElement>) => {
        setPieceLegoCode(values => {
            const newValues = [...values];
            newValues[index] = event.target.value;
            return newValues;
        });
    }, []);

    const saveHandler = useCallback(() => {
        const uniqueSet = new Set<string>();

        for(const piece of pieceLegoCode)
            uniqueSet.add(piece);

        if (uniqueSet.size !== pieceLegoCode.length) {
            enqueueSnackbar("I codici dei pezzi devono essere univoci", { variant: "error" });
            return;
        }

        const data: BaseWebSocket<LegoPiece[]> = {
            code: 0,
            message: "Update piece code",
            data: pieces.map((piece, index) => ({
                ...piece,
                legoId: pieceLegoCode[index]
            }))
        };

        sendMessage(JSON.stringify(data));
    }, [enqueueSnackbar, pieceLegoCode, pieces, sendMessage]);

    return (
        <Dialog open={true} onClose={() => { }}>
            <DialogContent>
                <Box display="flex" gap={3}>
                    {pieces.map((piece, index) => (
                        <Card key={index}>
                            <CardMedia sx={{ height: 250, backgroundSize: "contain" }} image={piece.imageUrl}  />
                            <CardContent sx={{ display: "flex", flexDirection: "column", gap: 2 }} >
                                <Typography variant="h5" textOverflow="ellipsis" whiteSpace="nowrap" overflow="hidden" title={piece.name}>{piece.name}</Typography>
                                <TextField fullWidth label="Lego code" size="small" variant="outlined" value={pieceLegoCode[index]} onChange={changeValueHandler(index)} />
                            </CardContent>
                        </Card>
                    ))}
                </Box>
            </DialogContent>
            <DialogActions>
                <Button color="secondary" onClick={() => { }}>Annulla</Button>
                <Button onClick={saveHandler}>Salva</Button>
            </DialogActions>
        </Dialog>
    )
}