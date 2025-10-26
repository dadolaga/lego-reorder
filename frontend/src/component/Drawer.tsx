"use client"

import { Drawer as Draw, List, ListItem, ListItemButton, ListItemIcon, ListItemText } from "@mui/material";
import { Icon } from "@iconify/react"

export const drawerWidth: number = 240;

export default function Drawer() {
    return (
        <Draw variant="permanent" sx={{ flexShrink: 0, width: drawerWidth, '& .MuiDrawer-paper': { width: drawerWidth } }}>
            <List>
                <ListItem disablePadding>
                    <ListItemButton>
                        <ListItemIcon>
                            <Icon icon="material-symbols:home" />
                        </ListItemIcon>
                        <ListItemText primary={"Homepage"} />
                    </ListItemButton>
                </ListItem>
            </List>
        </Draw>
    );
}