import { getPieces, OrderOptions } from "@/utilities/request";
import { LegoPiece, LegoSet, PiecesFilter } from "@/utilities/type";
import { getTextColorFromBackground, toHex } from "@/utilities/utils";
import { Box, Chip, LinearProgress, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, TableSortLabel, Typography } from "@mui/material";
import { ChangeEvent, MouseEvent, RefObject, use, useCallback, useEffect, useImperativeHandle, useState } from "react";

export interface PiecesTableRef {
    reloadPieces: () => void;
}

interface IProps {
    ref?: RefObject<PiecesTableRef>;
    piecesFilter: PiecesFilter;
}

export default function PiecesTable({
    ref,
    piecesFilter,
}: IProps) {
    const [loading, setLoading] = useState<boolean>(false)
    const [pieces, setPieces] = useState<LegoPiece[]>([]);
    const [piecesCount, setPiecesCount] = useState<number>(0);
    const [page, setPage] = useState<number>(0);
    const [rowsPerPage, setRowsPerPage] = useState<number>(25);
    const [order, setOrder] = useState<OrderOptions>({ name: "color", direction: "asc" });

    const reloadPieces = useCallback(() => {
        setLoading(true);
        setPieces([]);

        if (piecesFilter.setId !== undefined) {
            getPieces(piecesFilter?.setId, {
                limit: rowsPerPage,
                page: page,
                sort: [order]
            }).then(pieces => {
                setPieces(pieces.data);
                setPiecesCount(pieces.count);
            }).finally(() => {
                setLoading(false);
            });
        }
    }, [page, piecesFilter.setId, rowsPerPage, order]);

    useEffect(() => {
        reloadPieces();
    }, [rowsPerPage, page, reloadPieces]);

    useEffect(() => {
        reloadPieces();
    }, [reloadPieces]);

    useImperativeHandle(ref, () => {
        return {
            reloadPieces
        }
    }, [reloadPieces]);

    const handleChangePage = useCallback((event: MouseEvent<HTMLButtonElement, globalThis.MouseEvent> | null, page: number) => {
        console.log(event, page);
        setPage(page);
    }, []);

    const handleChangeRowsPerPage = useCallback((event: ChangeEvent<HTMLInputElement>) => {
        console.log(event);
        setRowsPerPage(parseInt(event.target.value, 10));
    }, []);

    const handleOrder = useCallback((name: string) => () => {
        if(order.name === name) {
            setOrder(v => ({ ...v, direction: v.direction === "asc" ? "desc" : "asc" }));
        } else {
            setOrder({ name, direction: "asc" });
        }
    }, [order]);

    return (
        <Paper sx={{ height: "100%", display: "flex", flexDirection: "column", justifyContent: "space-between" }}>
            <TableContainer sx={{ height: "100%" }}>
                <Table stickyHeader>
                    <TableHead>
                        <TableRow>
                            <TableCell width={70}></TableCell>
                            <TableCell>Name</TableCell>
                            <TableCell>
                                <TableSortLabel active={order.name === "color"} direction={order.direction} onClick={handleOrder("color")}>
                                    Color
                                </TableSortLabel>
                            </TableCell>
                            <TableCell width={70}>
                                <TableSortLabel active={order.name === "quantity"} direction={order.direction} onClick={handleOrder("quantity")}>
                                    Qta
                                </TableSortLabel>
                            </TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {loading && <TableRow><TableCell sx={{ p: 0, border: "none" }} colSpan={4}><LinearProgress /></TableCell></TableRow>}
                        {!loading && pieces.map(piece => (
                            <TableRow key={piece.databaseId}>
                                <TableCell>
                                    <img src={piece.imageUrl} alt={piece.name} width={70} height={70} />
                                </TableCell>
                                <TableCell>
                                    <Typography>{piece.name}</Typography>
                                </TableCell>
                                <TableCell>
                                    <Chip sx={{ backgroundColor: `#${toHex(piece.color?.value, 6)}`, "& span": { color: getTextColorFromBackground(piece.color!.value!) } }} label={piece.color?.name} ></Chip>
                                </TableCell>
                                <TableCell>
                                    <Typography>{piece.quantity}</Typography>
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </TableContainer>
            <TablePagination
                component="div"
                sx={{ overflow: "hidden" }}
                rowsPerPageOptions={[10, 25, 100]}
                count={piecesCount}
                rowsPerPage={rowsPerPage}
                page={page}
                onPageChange={handleChangePage}
                onRowsPerPageChange={handleChangeRowsPerPage}
            />
        </Paper>
    );
}