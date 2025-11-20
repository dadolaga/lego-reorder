import { getPieces } from "@/utilities/request";
import { LegoPiece, PiecesFilter } from "@/utilities/type";
import { getTextColorFromBackground, toHex } from "@/utilities/utils";
import { Box, Chip, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, TableSortLabel, Typography } from "@mui/material";
import { ChangeEvent, MouseEvent, RefObject, use, useCallback, useEffect, useImperativeHandle, useState } from "react";

export interface PiecesTableRef {
    reloadPieces: () => void;
}

interface IProps {
    ref?: RefObject<PiecesTableRef>;
    piecesFilter?: PiecesFilter;
}

export default function PiecesTable({
    ref,
    piecesFilter,
}: IProps) {
    const [pieces, setPieces] = useState<LegoPiece[]>([]);
    const [piecesCount, setPiecesCount] = useState<number>(0);
    const [page, setPage] = useState<number>(0);
    const [rowsPerPage, setRowsPerPage] = useState<number>(25);

    const reloadPieces = useCallback(() => {
        console.log("Reload pieces");

        getPieces(1, {
            limit: rowsPerPage,
            page: page,
            sort: []
        }).then(pieces => {
            setPieces(pieces.data);
            setPiecesCount(pieces.count);
        });
    }, [page, rowsPerPage]);

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

    return (
        <Paper sx={{ height: "100%", display: "flex", flexDirection: "column", justifyContent: "space-between" }}>
            <TableContainer sx={{ height: "100%" }}>
                <Table stickyHeader>
                    <TableHead>
                        <TableRow>
                            <TableCell width={70}></TableCell>
                            <TableCell>Name</TableCell>
                            <TableCell>
                                <TableSortLabel>
                                    Color
                                </TableSortLabel>
                            </TableCell>
                            <TableCell width={70}>
                                <TableSortLabel>
                                    Qta
                                </TableSortLabel>
                            </TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {pieces.map(piece => (
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