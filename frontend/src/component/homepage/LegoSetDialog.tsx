import { LegoPiece, LegoSet } from "@/utilities/type";
import { Box, Button, Dialog, DialogContent, DialogTitle, Typography, useMediaQuery, useTheme } from "@mui/material";
import PiecesTable from "../PiecesTable";
import { useCallback, useEffect, useState } from "react";
import SetMyBricksDialog from "../SetMyBrickDialog";
import { getPieces } from "@/utilities/request";
import { getTextColorFromBackground } from "@/utilities/utils";

interface State {
    name: string,
    color: string
}

const STATES: { [key: string]: State } = {
    inventory_pending: {
        name: "inventory pending",
        color: "#bcb8b1"
    },
    review_ongoing: {
        name: "Reviewing...",
        color: "#ffd60a"
    },
    parts_missing: {
        name: "Parts missing",
        color: "#f44336"
    },
    ready_to_build: {
        name: "Ready to build",
        color: "#4caf50"
    },
    builded: {
        name: "Builded",
        color: "#2196f3"
    },
    loading: {
        name: "...",
        color: "#000000"
    }
}

interface IProps {
    legoSet?: LegoSet,
    onClose: () => void,
}

export default function LegoSetDialog({
    legoSet,
    onClose,
}: IProps) {
    const theme = useTheme();
    const isExtraLarge = useMediaQuery(theme.breakpoints.up("xl"));
    const isSmall = useMediaQuery(theme.breakpoints.down("md"));

    const [showAddMyBrick, setShowAddMyBrick] = useState<boolean>(false);
    const [pieces, setPieces] = useState<LegoPiece[]>([]);
    const [state, setState] = useState<State>(STATES.loading);

    useEffect(() => {
        if (legoSet === undefined)
            return;

        getPieces(legoSet.databaseId, {
            limit: 1000,
            page: 0,
            sort: []
        }).then(pieces => {
            setPieces(pieces.data);
        });
    }, [legoSet]);

    const showAddMyBricksDialog = useCallback(() => {
        setShowAddMyBrick(true);
    }, []);

    const hideAddMyBricksDialog = useCallback(() => {
        setShowAddMyBrick(false);
    }, []);

    useEffect(() => {
        if (pieces.length === 0)
            return;

        console.log(pieces);

        if (pieces.every(piece => piece.quantityHave !== null)
            && pieces.every(piece => piece.quantityHave !== undefined && piece.quantityHave >= piece.quantity))
            setState(STATES.ready_to_build);
        else if (pieces.every(piece => piece.quantityHave === null))
            setState(STATES.inventory_pending);
        else if (pieces.every(piece => piece.quantityHave !== null)
            && pieces.some(piece => piece.quantityHave !== undefined && piece.quantityHave < piece.quantity))
            setState(STATES.parts_missing);
        else
            setState(STATES.review_ongoing);

    }, [pieces])

    return (
        <Dialog fullWidth maxWidth="lg" sx={{ "& .MuiPaper-root": { height: "100%" } }} open={legoSet !== undefined} onClose={onClose}>
            {showAddMyBrick && <SetMyBricksDialog legoSet={showAddMyBrick ? legoSet : undefined} onClose={hideAddMyBricksDialog} />}
            <DialogTitle>Info - {legoSet?.name}</DialogTitle>
            <DialogContent sx={{ height: "100%" }}>
                <Box sx={{ height: "100%", overflowY: "auto" }} display="flex" flexDirection="row" gap={2}>
                    <Box width="100%" display="flex" flexDirection="column" justifyContent="space-between">
                        <Box width="100%" height="100%" display="flex" flexDirection="column" gap={1} overflow="auto">
                            {/* eslint-disable-next-line @next/next/no-img-element */}
                            {legoSet && (<Box sx={{width: "100%", height: "40%", overflow: "hidden", backgroundColor: "red"}}><img style={{ width: "100%", height: "100%", objectFit: "cover"}} src={legoSet!.imageUrl} alt={legoSet!.name} /></Box>)}
                            <Box sx={{ backgroundColor: state.color }} display="flex" justifyContent="center" p={1.5}>
                                <Typography color={getTextColorFromBackground(state.color)} fontWeight="bold">{state.name}</Typography>
                            </Box>
                            <Box>
                                <Typography variant="h5">{legoSet?.name}</Typography>
                                <Box display="flex" flexDirection={isExtraLarge ? "column" : "row"} justifyContent="space-between" width="100%" gap={2}>
                                    <Box width="100%">
                                        <Typography>Theme: {legoSet?.theme.name}</Typography>
                                        <Typography>Code: {legoSet?.legoCode}</Typography>
                                        <Typography>Year: {legoSet?.year}</Typography>
                                    </Box>
                                    <Box width="100%">
                                        <Typography>Different pieces: {pieces.length > 0 ? pieces.length : "..."}</Typography>
                                        <Typography>Different pieces i have: {pieces.length > 0 ? pieces.reduce((accumulator, piece) => accumulator + (piece.quantityHave && piece.quantityHave > 0 ? 1 : 0), 0) : "..."}</Typography>
                                        <Typography>Number of pieces: {pieces.length > 0 ? pieces.reduce((accumulator, piece) => accumulator + piece.quantity, 0) : "..."}</Typography>
                                        <Typography>Number of pieces i have: {pieces.length > 0 ? pieces.reduce((accumulator, piece) => accumulator + (piece.quantityHave || 0), 0) : "..."}</Typography>
                                    </Box>
                                </Box>
                            </Box>
                        </Box>
                        <Box p="0px 32px" display="flex" flexDirection="column" gap={1}>
                            <Button fullWidth variant="contained" onClick={showAddMyBricksDialog}>
                                Add my bricks
                            </Button>
                        </Box>
                    </Box>
                    {!isSmall && <Box width="100%">
                        <PiecesTable piecesFilter={{ setId: legoSet?.databaseId }} />
                    </Box>}
                </Box>
            </DialogContent>
        </Dialog>
    );
}