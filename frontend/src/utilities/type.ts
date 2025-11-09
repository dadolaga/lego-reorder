export interface BaseWebSocket<T> {
    code: number,
    message?: string,
    data: T;
}

export interface LegoSet {
    databaseId: number;
    apiId: string;
    legoCode: number;
    name: string;
    year: number,
    imageUrl: string;
}

export interface LegoPiece {
    databaseId?: number; 
    legoId?: string | null; 
    apiId?: string; 
    name?: string;
    imageUrl?: string;
    color?: LegoColor;
    quantity: number;
    isSpare: boolean;
    legoSets: LegoSet[];
}

export interface LegoColor {
    databaseId?: number; 
    apiId?: string; 
    name?: string; 
    value?: number; 
    trasparent?: boolean; 
}