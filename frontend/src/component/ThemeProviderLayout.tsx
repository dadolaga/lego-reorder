'use client';

import { createTheme, ThemeOptions, ThemeProvider } from '@mui/material/styles';

export const themeOptions: ThemeOptions = {
    palette: {
        mode: 'dark',
        primary: {
            main: '#d62828',
        },
        secondary: {
            main: '#f48c06',
        },
    },
};

const theme = createTheme(themeOptions);

export default function ThemeProviderWrapper({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        <ThemeProvider theme={theme}>
            {children}
        </ThemeProvider>);
}