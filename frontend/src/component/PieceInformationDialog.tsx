import { LegoPiece, LegoSet } from "@/utilities/type";
import { Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, LinearProgress, SxProps, Theme, Typography } from "@mui/material";
import { useCallback, useEffect, useState } from "react";

const noWrap: SxProps<Theme> = {
    whiteSpace: "nowrap",
    overflow: "hidden",
    textOverflow: "ellipsis",
}

interface IProps {
    piece?: LegoPiece | undefined,
    set?: LegoSet | undefined,
    onClose: () => void,
}

export default function PieceInformationDialog({
    piece,
    onClose: close,
}: IProps) {
    const [loading, setLoading] = useState<boolean>(false);

    useEffect(() => {

    }, [piece]);

    const closeHandler = useCallback(() => {
        close();
    }, [close]);

    return (
        <Dialog fullWidth maxWidth="sm" open={piece !== undefined}>
            {loading && <LinearProgress />}
            <DialogTitle>{piece?.name}</DialogTitle>
            <DialogContent sx={{ height: "100%" }}>
                <Box width="100%" display="flex" flexDirection="row" justifyContent="start" gap={2}>
                    {/* eslint-disable-next-line @next/next/no-img-element */}
                    {piece && (<Box sx={{ width: "50%", height: "40%", overflow: "hidden" }}><img style={{ width: "100%", height: "100%", objectFit: "cover" }} src={piece!.imageUrl} alt={piece!.name} /></Box>)}
                    <Box display="flex" flexDirection={"column"} alignItems="start" width="100%" overflow="hidden">
                        <Typography sx={noWrap}>Name: <Typography color="textSecondary" component="span" title={piece?.name}>{piece?.name}</Typography></Typography>
                        <Typography sx={noWrap}>Codice: <Typography color="textSecondary" component="span">{piece?.legoId}</Typography></Typography>
                        <Typography sx={noWrap}>Colore: <Typography color="textSecondary" component="span">{piece?.color?.name}</Typography></Typography>

                    
                    </Box>
                </Box>

            </DialogContent>
            <DialogActions>
                <Button onClick={closeHandler}>Close</Button>
            </DialogActions>
        </Dialog>
    );
}