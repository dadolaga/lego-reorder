import { LegoSet } from "@/utilities/type";
import { Button, Card, CardActions, CardContent, CardMedia, Typography } from "@mui/material";

export interface IButtonList {
    name: string,
    onClick: ( set: LegoSet) => () => void,
}

interface IProps {
    legoSet: LegoSet,
    buttonList?: IButtonList[],
}

export default function LegoCard({
    legoSet,
    buttonList = []
} : IProps) {
    return (
        <Card>
            <CardMedia sx={{ height: 200 }} image={legoSet.imageUrl} />
            <CardContent>
                <Typography variant="h5" textOverflow="ellipsis" whiteSpace="nowrap" overflow="hidden">{legoSet.name}</Typography>
                <Typography variant="body2">Years: {legoSet.year || "???"}</Typography>
                <Typography variant="body2">Pieces: {"???"}</Typography>
            </CardContent>
            <CardActions sx={{justifyContent: "end"}}>
                {buttonList.map(button => (
                    <Button key={button.name} onClick={button.onClick(legoSet)}>{button.name}</Button>
                ))}
            </CardActions>
        </Card>
    );
}