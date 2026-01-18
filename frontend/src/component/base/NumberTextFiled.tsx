import { TextField, TextFieldProps } from "@mui/material"
import { memo, useCallback } from "react"


interface IProps extends Omit<TextFieldProps, 'variant'> {
    value?: number;
    onValueChange?: (value: number | undefined) => void;
}

const NumberTextField = memo((props: IProps) => {
    const onChangeHandler = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value

        if (props.onValueChange) {
            if(RegExp(/^\d*$/).test(value)) {
                const intValue = parseInt(value);
                props.onValueChange(!Number.isNaN(intValue) ? intValue : undefined);
            }
        }
        
    }, [props])

    return (
        <TextField {...props} value={`${(props.value !== undefined && props.value !== null) ? `${props.value}` : ""}`} onChange={onChangeHandler}/>
    )
})

NumberTextField.displayName = "NumberTextField";
export default NumberTextField