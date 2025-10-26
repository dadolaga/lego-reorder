import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import ThemeProviderWrapper from "@/component/ThemeProviderLayout";
import BaseContainer from "@/component/BaseContainer";
import Drawer from "@/component/Drawer";
import { Box } from "@mui/material";

const geistSans = Geist({
    variable: "--font-geist-sans",
    subsets: ["latin"],
});

const geistMono = Geist_Mono({
    variable: "--font-geist-mono",
    subsets: ["latin"],
});

export const metadata: Metadata = {
    title: "Lego reorder",
    description: "This application helps you to reorder your Lego collection.",
};

const drawerWidth: number = 240;

export default function RootLayout({
    children,
}: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        <html lang="en">
            <body className={`${geistSans.variable} ${geistMono.variable}`} style={{ margin: 0, width: "100vw", height: "100vh" }}>
                <ThemeProviderWrapper>
                    <BaseContainer>
                        <Drawer />
                        <Box sx={{width: `calc(100% - ${drawerWidth}px)`, height: "100%", ml: `${drawerWidth}px`, p: 2, boxSizing: "border-box"}}>
                            {children}
                        </Box>
                    </BaseContainer>
                </ThemeProviderWrapper>
            </body>
        </html>
    );
}