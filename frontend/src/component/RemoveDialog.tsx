import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

export interface RemoveOption {
    open: boolean,
    text: string,
    hide: (confirm: boolean) => void,
}

export default function RemoveDialog({
    open,
    text,
    hide
}: RemoveOption) {
    return (
        <Dialog
            open={open}
            onClose={() => hide(false)}>
            <DialogTitle>Conferma eliminazione</DialogTitle>
            <DialogContent>
                <DialogContentText> {text} </DialogContentText>
            </DialogContent>
            <DialogActions>
                <Button onClick={() => hide(false)}>Annulla</Button>
                <Button onClick={() => hide(true)} autoFocus>
                    Elimina
                </Button>
            </DialogActions>
        </Dialog>
    );
}
