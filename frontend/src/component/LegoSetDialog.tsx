import { Dialog } from "@mui/material";

interface Iprops {
    open: boolean,
    hide: (save: boolean) => void,
}

export default function LegoSetDialog({
    open,
    hide
}: Iprops)
{
    return (
        <Dialog open={open} onClose={() => hide(false)}>
            
        </Dialog>
    );
}