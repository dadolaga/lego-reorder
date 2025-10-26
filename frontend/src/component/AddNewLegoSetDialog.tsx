import { Response, searchLego } from "@/utilities/request";
import { LegoSet } from "@/utilities/type";
import { Icon } from "@iconify/react";
import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Grid, IconButton, TextField, Typography, useTheme } from "@mui/material";
import { ChangeEvent, useCallback, useState, memo, useRef, useEffect } from "react";
import LegoCard, { IButtonList } from "./homepage/LegoCard";
import { addNewLegoSet, addPieceToLegoSet } from "@/utilities/request"
import { Axios, AxiosError } from "axios";

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
            setLoadingMessage("Aggiungi lego set al database");

            addNewLegoSet(set!).then(() => {
                setLoadingMessage("Aggiungi pezzi lego al database");

                addPieceToLegoSet(set!).then(() => {
                    hide(false);
                }).finally(() => {
                    setLoading(false);
                    setLoadingMessage("");
                });
            }).catch((err: AxiosError<Response>) => {
                if(err.response?.data.code == 11) {
                    console.log(err.response.data.message);
                }

                setLoading(false);
            });
        }
    }]);

    useEffect(() => {
        legoSetRef.current = legoSets;
    }, [legoSets]);

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

    return (
        <Dialog fullWidth maxWidth="md" open={open} onClose={closeDialog} slotProps={{ paper: { sx: { height: "100%" } } }}>
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