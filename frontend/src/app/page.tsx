"use client"

import { Box, Button, Card, CardActions, CardContent, CardMedia, Grid, Paper, Typography } from "@mui/material";
import LegoCard, { IButtonList } from "@/component/homepage/LegoCard";
import AddNewLegoSetDialog from "@/component/AddNewLegoSetDialog";
import { useEffect, useState } from "react";
import { getMyLegoSet, deleteLegoSet } from "@/utilities/request";
import { LegoSet } from "@/utilities/type";
import RemoveDialog, { RemoveOption } from "@/component/RemoveDialog";
import { enqueueSnackbar } from "notistack";
import LegoSetDialog from "@/component/homepage/LegoSetDialog";

export default function Home() {
    const [showAddNewLegoSetDialog, setShowAddNewLegoSetDialog] = useState<boolean>(false);
    const [legoSets, setLegoSets] = useState<LegoSet[]>([]);
    const [removeOption, setRemoveOption] = useState<RemoveOption>({ open: false, text: "", hide: () => { } });
    const [legoSetInfo, setLegoSetInfo] = useState<LegoSet | undefined>(undefined);    
    const [buttonList,] = useState<IButtonList[]>([{
        name: "Remove",
        onClick: (legoSet: LegoSet) => () => {
            setRemoveOption({
                open: true,
                text: `Sei sicuro di voler rimuovere il set "${legoSet.name}" dalla tua collezione?`,
                hide: (del: boolean) => {
                    if (del) {
                        deleteLegoSet(legoSet.databaseId).then(() => {
                            enqueueSnackbar(`Set "${legoSet.legoCode}" rimosso correttamente`, { variant: "success" });
                            loadMySetLego();    
                            setRemoveOption(v => ({ ...v, open: false }));
                        });
                    } else {
                        setRemoveOption(v => ({ ...v, open: false }));
                    }
                }
            });
        }
    }, {
        name: "info",
        onClick: (legoSet: LegoSet) => () => {
            setLegoSetInfo(legoSet);
        }
    }]);

    useEffect(() => {
        loadMySetLego();
    }, []);

    function loadMySetLego() {
        getMyLegoSet().then(legoSets => {
            setLegoSets(legoSets!);
        });
    }

    const clickAddNewLegoSetHandler = () => {
        setShowAddNewLegoSetDialog(true);
    }

    const hideAddNewLegoSetDialog = (save: boolean) => {
        setShowAddNewLegoSetDialog(false);

        if (save) {
            loadMySetLego();
        }
    }

    return (
        <Box display="flex" flexDirection="column" gap={4} height="100%">
            <RemoveDialog {...removeOption} />
            <AddNewLegoSetDialog hide={hideAddNewLegoSetDialog} open={showAddNewLegoSetDialog} />
            <LegoSetDialog legoSet={legoSetInfo} onClose={() => setLegoSetInfo(undefined)} />
            <Typography variant="h1" color="primary" align="center">Lego Reorder</Typography>
            <Typography variant="h4" color="secondary" align="center">Ecco la tua collezione lego</Typography>
            <Paper sx={{ width: "100%", height: "100%", p: 2, boxSizing: "border-box", display: "flex", flexDirection: "column", gap: 2, overflow: "auto"}}>
                <Box display="flex" justifyContent="end">
                    <Button variant="contained" onClick={clickAddNewLegoSetHandler}>Aggiungi set</Button>
                </Box>
                <Box overflow="auto" padding="0 1em">
                    <Grid container spacing={1.5}>
                        {legoSets.map(set => (
                            <Grid key={set.databaseId} size={{ lg: 3, md: 4, sm: 6, xs: 12 }}>
                                <LegoCard legoSet={set} buttonList={buttonList} />
                            </Grid>
                        ))}
                    </Grid>
                </Box>
            </Paper>
        </Box>
    );
}
