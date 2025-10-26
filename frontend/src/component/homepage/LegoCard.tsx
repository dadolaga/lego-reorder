import { Button, Card, CardActions, CardContent, CardMedia, Typography } from "@mui/material";

export interface IButtonList {
    name: string,
    onClick: ( id: string | number) => () => void,
}

interface IProps {
    id: string | number,
    imageUrl: string,
    title: string,
    years?: number,
    pieces?: number,
    buttonList?: IButtonList[],
}

export default function LegoCard({
    id,
    imageUrl,
    title,
    years,
    pieces,
    buttonList = []
} : IProps) {
    return (
        <Card>
            <CardMedia sx={{ height: 200 }} image={imageUrl} />
            <CardContent>
                <Typography variant="h5" textOverflow="ellipsis" whiteSpace="nowrap" overflow="hidden">{title}</Typography>
                <Typography variant="body2">Years: {years || "???"}</Typography>
                <Typography variant="body2">Pieces: {pieces || "???"}</Typography>
            </CardContent>
            <CardActions sx={{justifyContent: "end"}}>
                {buttonList.map(button => (
                    <Button key={button.name} onClick={button.onClick(id)}>{button.name}</Button>
                ))}
            </CardActions>
        </Card>
    );
}