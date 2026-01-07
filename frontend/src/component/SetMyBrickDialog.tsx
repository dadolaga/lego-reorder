import { LegoColor, LegoPiece, LegoSet } from "@/utilities/type";
import { toHex, getTextColorFromBackground } from "@/utilities/utils";
import { Box, Button, Checkbox, Chip, Dialog, DialogActions, DialogContent, DialogTitle, FormControl, InputLabel, LinearProgress, MenuItem, OutlinedInput, Paper, Select, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, Typography } from "@mui/material";
import { ChangeEvent, useCallback, useEffect, useState, MouseEvent, KeyboardEvent, useRef } from "react";
import NumberTextField from "./base/NumberTextFiled";
import { useAddMyBrickWS } from "@/logic/useAddMyBrickWebSocket";
import { getLegoPieceColorsFromSet, getPieces } from "@/utilities/request";

interface IProps {
    legoSet?: LegoSet,
    onClose: () => void,
}

const ACTIVE = "#fad64330";
const ACTIVE_ON_OTHER_DEVICE = "#926c1520"

const TABLE_SIZE = 10;

export default function SetMyBrick({
    legoSet,
    onClose,
}: IProps) {
    const pieceTextRef = useRef<Map<number, HTMLInputElement>>(new Map());

    const { run,
        close: closeWS,
        loading: loadingWS,
        selectedPieceId: activePieceOnOtherDevice,
        piecesQuantityHave,
        sendActive,
        sendPersonalQuantity } = useAddMyBrickWS();
    const [page, setPage] = useState<number>(0);
    const [piecesCount, setPiecesCount] = useState<number>(0);
    const [loading, setLoading] = useState<boolean>(false);
    const [values, setValues] = useState<(number | undefined)[]>([]);
    const [colors, setColors] = useState<LegoColor[]>([]);
    const [selectedColor, setSelectedColor] = useState<LegoColor[]>([]);
    const [activePieceId, setActivePieceId] = useState<number>();

    const [pieces, setPieces] = useState<LegoPiece[]>([]);

    useEffect(() => {
        if (legoSet === undefined)
            return;

        run(legoSet);
    }, [legoSet, run])

    useEffect(() => {
        if (legoSet === undefined)
            return;

        getLegoPieceColorsFromSet(legoSet).then((apiColors) => {
            setColors(apiColors!);
        });
    }, [legoSet]);

    useEffect(() => {
        if (legoSet === undefined)
            return;

        getPieces(legoSet.databaseId, {
            limit: TABLE_SIZE,
            page: page,
            sort: [{ name: "color", direction: "asc" }, { name: "quantity", direction: "desc" }],
            where: {
                color: {
                    in: selectedColor.map(color => color.databaseId!)
                }
            }
        }).then(pieces => {
            setPieces(pieces.data);
            setPiecesCount(pieces.count);
        }).finally(() => {
            setLoading(false);
        });
    }, [legoSet, page, selectedColor]);

    useEffect(() => {
        if (piecesQuantityHave === null)
            return;

        const newValues: number[] = [];

        for (const pieceQuantity of piecesQuantityHave!) {
            newValues[pieceQuantity.pieceId] = pieceQuantity.quantityHave;
        }

        setValues(newValues);

    }, [piecesQuantityHave])

    const insertTextHandler = useCallback((piece: LegoPiece) => (value: number) => {
        setValues((values) => {
            const newValues = [...values];
            newValues[piece.databaseId!] = value;
            return newValues;
        });
    }, []);

    const colorSelectedChange = useCallback((event: ChangeEvent<HTMLInputElement> | (Event & { target: { value: LegoColor[]; name: string; }; })) => {
        const clickedColorId: number = parseInt(typeof event.target.value[event.target.value.length - 1] === "string" ? event.target.value[event.target.value.length - 1] as string : "0");

        console.log(clickedColorId);

        setSelectedColor((selectedColor) => {
            let removed = false
            const newSelectedColor: LegoColor[] = [];

            selectedColor.forEach((color) => {
                if (color.databaseId === clickedColorId)
                    removed = true;
                else
                    newSelectedColor.push(color);
            });

            if (!removed)
                if (colors.filter((color) => color.databaseId === clickedColorId).length > 0)
                    newSelectedColor.push(colors.filter((color) => color.databaseId === clickedColorId)[0]);
                else
                    console.error("Color not found");

            return newSelectedColor;
        });

    }, [colors]);

    const selectColorRenderValue = useCallback((selected: LegoColor[]) => {
        return (
            <Box display="flex" flexDirection="row" gap={1}>
                {selected.sort((a, b) => a.value! - b.value!).map((color) =>
                    <Box key={color.databaseId} width={24} height={24} sx={{ backgroundColor: `#${toHex(color.value, 6)}` }} />
                )}
            </Box>
        )
    }, []);

    const handleChangePage = useCallback((event: MouseEvent<HTMLButtonElement, globalThis.MouseEvent> | null, page: number) => {
        setPage(page);
    }, []);

    const focusOnPiece = useCallback((piece: LegoColor) => () => {
        setActivePieceId(piece.databaseId);
        sendActive(piece.databaseId!)
    }, [sendActive]);

    const keyPressedOnPieceHandler = useCallback((piece: LegoPiece) => (event: KeyboardEvent<HTMLDivElement>) => {
        if (event.key === "Enter") {
            sendPersonalQuantity(piece.databaseId!, values[piece.databaseId!]!);

            const inputs: HTMLInputElement[] = [...document.querySelectorAll<HTMLInputElement>(".quantity-have-field div input")];

            inputs.forEach((field, index) => {
                if (field === event.target) {
                    setTimeout(() => {
                        let timeoutIndex = index + 1;
                        let input = inputs[timeoutIndex];

                        // Jump all disabled input
                        while (input.disabled) {
                            timeoutIndex++;
                            input = inputs[timeoutIndex];
                        }

                        input.focus();
                    }, 10);
                }
            });
        }
    }, [sendPersonalQuantity, values]);

    const editValueButtonHandler = useCallback((piece: LegoPiece) => () => {
        setValues((values) => {
            const newValues = [...values];
            newValues[piece.databaseId!] = undefined;
            return newValues;
        });

        setTimeout(() => {
            pieceTextRef.current.get(piece.databaseId!)?.focus();
        }, 10);
    }, []);

    const closeHandler = useCallback(() => {
        closeWS();
        onClose();
    }, [closeWS, onClose]);

    return (
        <Dialog fullWidth maxWidth="md" sx={{ "& .MuiDialog-paper": { height: "60%" } }} open={legoSet !== undefined}>
            {(loading || loadingWS) && <LinearProgress />}
            <DialogTitle>Add my brick - {legoSet?.name}</DialogTitle>
            <DialogContent sx={{ height: "100%" }}>
                <Box height="100%" display="grid" gridTemplateRows="auto 1fr">
                    <Box>
                        <FormControl sx={{ m: 1, width: 300 }} size="small">
                            <InputLabel id="multi-color-select">Color</InputLabel>
                            <Select
                                labelId="multi-color-select"
                                size="small"
                                multiple
                                value={selectedColor}
                                onChange={colorSelectedChange}
                                input={<OutlinedInput label="Color" />}
                                renderValue={selectColorRenderValue}
                            >
                                {colors.map((color) => (
                                    <MenuItem key={color.databaseId} value={`${color.databaseId}`}>
                                        <Checkbox checked={selectedColor.filter((c) => c?.databaseId === color.databaseId).length > 0} />
                                        <Chip sx={{ backgroundColor: `#${toHex(color.value, 6)}`, "& span": { color: getTextColorFromBackground(color.value!) } }} label={color.name} ></Chip>
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    </Box>
                    <Paper sx={{ height: "100%", overflowY: "auto", display: "flex", flexDirection: "column", justifyContent: "space-between" }}>
                        <TableContainer sx={{ height: "100%" }}>
                            <Table stickyHeader>
                                <TableHead>
                                    <TableRow>
                                        <TableCell width={100}></TableCell>
                                        <TableCell>Code</TableCell>
                                        <TableCell>Name</TableCell>
                                        <TableCell>Color</TableCell>
                                        <TableCell sx={{ textAlign: "center" }} width={150}>Qta</TableCell>
                                        <TableCell width={150}></TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {selectedColor.length === 0 &&
                                        <TableRow>
                                            <TableCell sx={{ border: "none" }} colSpan={5}><Typography align="center" fontStyle="italic" color="textDisabled">Select color...</Typography></TableCell>
                                        </TableRow>
                                    }
                                    {!loading && pieces.map((piece) => (
                                        <TableRow
                                            key={piece.databaseId}
                                            sx={{
                                                backgroundColor: piece.databaseId == activePieceId ? ACTIVE : (activePieceOnOtherDevice.find(id => id === piece.databaseId) ? ACTIVE_ON_OTHER_DEVICE : undefined)
                                            }}>
                                            <TableCell>
                                                {/* eslint-disable-next-line @next/next/no-img-element*/}
                                                <img src={piece.imageUrl} alt={piece.name} width={100} height={100} />
                                            </TableCell>
                                            <TableCell>
                                                <Typography>{piece.legoId}</Typography>
                                            </TableCell>
                                            <TableCell>
                                                <Typography>{piece.name}</Typography>
                                            </TableCell>
                                            <TableCell>
                                                <Chip sx={{ backgroundColor: `#${toHex(piece.color?.value, 6)}`, "& span": { color: getTextColorFromBackground(piece.color!.value!) } }} label={piece.color?.name} ></Chip>
                                            </TableCell>
                                            <TableCell>
                                                <Typography sx={{ fontSize: 48, fontWeight: "bold", textAlign: "center" }}>{piece.quantity}</Typography>
                                            </TableCell>
                                            <TableCell>
                                                <Box display="flex" flexDirection="column" gap={2}>
                                                    <NumberTextField
                                                        inputRef={(node) => {
                                                            if (node) {
                                                                pieceTextRef.current.set(piece.databaseId!, node);
                                                            } else {
                                                                pieceTextRef.current.delete(piece.databaseId!);
                                                            }
                                                        }}
                                                        fullWidth
                                                        size="small"
                                                        className="quantity-have-field"
                                                        autoComplete="off"
                                                        value={values[piece.databaseId!]}
                                                        onValueChange={insertTextHandler(piece)}
                                                        onKeyUp={keyPressedOnPieceHandler(piece)}
                                                        onFocus={focusOnPiece(piece)}
                                                        disabled={piece.databaseId != activePieceId && (values[piece.databaseId!] !== undefined || (activePieceOnOtherDevice.find(id => id === piece.databaseId) !== undefined))} />
                                                    <Button
                                                        tabIndex={-1}
                                                        variant="contained"
                                                        onClick={editValueButtonHandler(piece)}
                                                        disabled={values[piece.databaseId!] === undefined || activePieceOnOtherDevice.find(id => id === piece.databaseId) !== undefined}>Edit</Button>
                                                </Box>
                                            </TableCell>
                                        </TableRow>
                                    ))}
                                </TableBody>
                            </Table>
                        </TableContainer>
                        <TablePagination
                            component="div"
                            sx={{ overflow: "hidden" }}
                            rowsPerPageOptions={[TABLE_SIZE]}
                            count={piecesCount}
                            rowsPerPage={TABLE_SIZE}
                            page={page}
                            onPageChange={handleChangePage}
                        />
                    </Paper>
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={closeHandler}>Close</Button>
            </DialogActions>
        </Dialog>
    );
}