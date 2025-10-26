"use client"

import { useTheme } from "@mui/material";

export default function BaseContainer({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    const theme = useTheme();

    return (
        <div style={{ backgroundColor: theme.palette.background.paper, width: "100vw", height: "100vh" }}>
            {children}
        </div>
    );
}