export function getTextColorFromBackground(hexColorValue: number): 'white' | 'black' {
    const r = (hexColorValue >> 16) & 0xFF;
    const g = (hexColorValue >> 8) & 0xFF;
    const b = hexColorValue & 0xFF;

    // Calculate (Luminosity)
    const perceivedLuminosity = (r * 0.2126) + (g * 0.7152) + (b * 0.0722);
    const threshold = 150;

    return perceivedLuminosity > threshold ? 'black' : 'white';
}

export function toHex(decimalValue: number | undefined | null, paddingLength: number = 0): string {
    if(decimalValue === undefined || decimalValue === null)
        return "NaN";

    const hexString = decimalValue.toString(16).toUpperCase();
    return hexString.padStart(paddingLength, '0');
}