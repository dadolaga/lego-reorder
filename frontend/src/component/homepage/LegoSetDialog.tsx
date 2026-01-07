import { LegoPiece, LegoSet } from "@/utilities/type";
import { Badge, Box, Button, Dialog, DialogContent, DialogTitle, Typography } from "@mui/material";
import PiecesTable from "../PiecesTable";
import { useCallback, useEffect, useState } from "react";
import SetMyBricksDialog from "../SetMyBrickDialog";
import { getPieces } from "@/utilities/request";

interface IProps {
    legoSet?: LegoSet,
    onClose: () => void,
}

export default function LegoSetDialog({
    legoSet,
    onClose,
}: IProps) {
    const [showAddMyBrick, setShowAddMyBrick] = useState<boolean>(false);
    const [pieces, setPieces] = useState<LegoPiece[]>([]);

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

    return (
        <Dialog fullWidth maxWidth="lg" sx={{ "& .MuiPaper-root": { height: "100%" } }} open={legoSet !== undefined} onClose={onClose}>
            {showAddMyBrick && <SetMyBricksDialog legoSet={showAddMyBrick ? legoSet : undefined} onClose={hideAddMyBricksDialog} />}
            <DialogTitle>Info - {legoSet?.name}</DialogTitle>
            <DialogContent sx={{ height: "100%" }}>
                <Box sx={{ height: "100%", overflowY: "auto" }} display="flex" flexDirection="row" gap={2}>
                    <Box width="100%" display="flex" flexDirection="column" justifyContent="space-between">
                        <Box width="100%" display="flex" flexDirection="column" gap={1}>
                            {legoSet && <img style={{ width: "100%", height: 400, objectFit: "cover" }} src={legoSet!.imageUrl} alt={legoSet!.name} />}
                            <Box sx={{ backgroundColor: "red" }} display="flex" justifyContent="center" p={1.5}>
                                <Typography>- STATE -</Typography>
                            </Box>
                            <Box display="flex" flexDirection="column">
                                <Typography variant="h5">{legoSet?.name}</Typography>
                                <Typography>Theme: {legoSet?.theme.name}</Typography>
                                <Typography>Code: {legoSet?.legoCode}</Typography>
                                <Typography>Year: {legoSet?.year}</Typography>
                                <br />
                                <Typography>Pieces: {pieces.length > 0 ? pieces.length : "..."}</Typography>
                                <Typography>Pieces i have: {pieces.length > 0 ? pieces.reduce((accumulator, piece) => accumulator + (piece.quantityHave && piece.quantityHave > 0 ? 1 : 0), 0) : "..."}</Typography>
                                <Typography>Number of pieces: {pieces.length > 0 ? pieces.reduce((accumulator, piece) => accumulator + piece.quantity, 0) : "..."}</Typography>
                            </Box>
                        </Box>
                        <Box p="0px 32px" display="flex" flexDirection="column" gap={1}>
                            <Badge badgeContent={4} color="secondary">
                                <Button fullWidth variant="contained" onClick={showAddMyBricksDialog}>
                                    Add my bricks
                                </Button>
                            </Badge>
                        </Box>
                    </Box>
                    <Box width="100%">
                        <PiecesTable piecesFilter={{ setId: legoSet?.databaseId }} />
                    </Box>
                </Box>
            </DialogContent>
        </Dialog>
    );
}