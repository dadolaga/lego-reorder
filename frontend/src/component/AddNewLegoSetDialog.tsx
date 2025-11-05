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
                <LegoPieceCorrectorDialog sendMessage={sendEditPieceHandler} piece1={checkPiece[0]} piece2={checkPiece[1]} />
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
    piece1: LegoPiece,
    piece2: LegoPiece,
}

function LegoPieceCorrectorDialog({
    sendMessage,
    piece1,
    piece2,
}: LegoPieceCorrectorProps) {
    const { enqueueSnackbar } = useSnackbar();
    const [piece1Code, setPiece1Code] = useState<string>(piece1.legoId || "");
    const [piece2Code, setPiece2Code] = useState<string>(piece2.legoId || "");

    useEffect(() => {
        setPiece1Code(piece1.legoId || "");
        setPiece2Code(piece2.legoId || "");
    }, [piece1, piece2])

    const changeValue1Handler = useCallback((event: ChangeEvent<HTMLInputElement>) => {
        setPiece1Code(event.target.value);
    }, []);

    const changeValue2Handler = useCallback((event: ChangeEvent<HTMLInputElement>) => {
        setPiece2Code(event.target.value);
    }, []);

    const saveHandler = useCallback(() => {
        if (piece1Code === piece2Code) {
            enqueueSnackbar("I codici dei pezzi non possono essere uguali", { variant: "error" });
            return;
        }

        const data: BaseWebSocket<LegoPiece[]> = {
            code: 0,
            message: "Update piece code",
            data: [{ ...piece1, legoId: piece1Code }, { ...piece2, legoId: piece2Code }]
        };

        sendMessage(JSON.stringify(data));
    }, [enqueueSnackbar, piece1, piece1Code, piece2, piece2Code, sendMessage]);

    return (
        <Dialog open={true} onClose={() => { }}>
            <DialogContent>
                <Box display="flex" gap={3}>
                    <Card>
                        <CardMedia sx={{ height: 250 }} image={piece1.imageUrl} />
                        <CardContent sx={{ display: "flex", flexDirection: "column", gap: 2 }} >
                            <Typography variant="h5" textOverflow="ellipsis" whiteSpace="nowrap" overflow="hidden" title={piece1.name}>{piece1.name}</Typography>
                            <TextField fullWidth label="Lego code" size="small" variant="outlined" value={piece1Code} onChange={changeValue1Handler} />
                        </CardContent>
                    </Card>
                    <Card>
                        <CardMedia sx={{ height: 250 }} image={piece2.imageUrl} />
                        <CardContent sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                            <Typography variant="h5" textOverflow="ellipsis" whiteSpace="nowrap" overflow="hidden" title={piece2.name}>{piece2.name}</Typography>
                            <TextField fullWidth label="Lego code" size="small" variant="outlined" value={piece2Code} onChange={changeValue2Handler} />
                        </CardContent>
                    </Card>
                </Box>
            </DialogContent>
            <DialogActions>
                <Button color="secondary" onClick={() => { }}>Annulla</Button>
                <Button onClick={saveHandler}>Salva</Button>
            </DialogActions>
        </Dialog>
    )
}