"use client"

import { Box, Button, Card, CardActions, CardContent, CardMedia, Grid, Paper, Typography } from "@mui/material";
import LegoCard, { IButtonList } from "@/component/homepage/LegoCard";
import AddNewLegoSetDialog from "@/component/AddNewLegoSetDialog";
import { useEffect, useState } from "react";
import { getMyLegoSet } from "@/utilities/request";
import { LegoSet } from "@/utilities/type";
import RemoveDialog, { RemoveOption } from "@/component/RemoveDialog";


export default function Home() {
    const [showAddNewLegoSetDialog, setShowAddNewLegoSetDialog] = useState<boolean>(false);
    const [legoSets, setLegoSets] = useState<LegoSet[]>([]);
    const [removeOption, setRemoveOption] = useState<RemoveOption>({ open: false, text: "", hide: () => { } });
    const [buttonList,] = useState<IButtonList[]>([{
        name: "Remove",
        onClick: (id: string | number) => () => {
            setRemoveOption({
                open: true,
                text: "Rimuovere questo set?",
                hide: (del: boolean) => {
                    if(!del)
                        setRemoveOption({ ...removeOption, open: false });
                }
            });

        }
    }]);

    useEffect(() => {
        getMyLegoSet().then(legoSets => {
            setLegoSets(legoSets!);

            console.log(legoSets);
        });
    }, []);

    const clickAddNewLegoSetHandler = () => {
        setShowAddNewLegoSetDialog(true);
    }

    const hideAddNewLegoSetDialog = () => {
        setShowAddNewLegoSetDialog(false);
    }

    return (
        <Box display="flex" flexDirection="column" gap={4} height="100%">
            <RemoveDialog {...removeOption} />
            <AddNewLegoSetDialog hide={hideAddNewLegoSetDialog} open={showAddNewLegoSetDialog} />
            <Typography variant="h1" color="primary" align="center">Lego Reorder</Typography>
            <Typography variant="h4" color="secondary" align="center">Ecco la tua collezione lego</Typography>
            <Paper sx={{ width: "100%", height: "100%", p: 2, boxSizing: "border-box", display: "flex", flexDirection: "column", gap: 2 }}>
                <Box display="flex" justifyContent="end">
                    <Button variant="contained" onClick={clickAddNewLegoSetHandler}>Aggiungi set</Button>
                </Box>
                <Box>
                    <Grid container>
                        {legoSets.map(set => (
                            <Grid key={set.databaseId} size={{ lg: 3, md: 4, sm: 6, xs: 12 }}>
                                <LegoCard id={set.databaseId} title={set.name} imageUrl={set.imageUrl} years={set.year} pieces={undefined} buttonList={buttonList} />
                            </Grid>
                        ))}
                    </Grid>
                </Box>
            </Paper>
        </Box>
    );
}
