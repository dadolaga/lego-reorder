import { axiosInstance as axios } from '@/utilities/axios'
import { AxiosError, AxiosRequestConfig, AxiosResponse } from 'axios';
import { LegoColor, LegoPiece, LegoSet } from './type';

export interface Response<T = null> {
    code: number,
    message: string,
    count?: number,
    data: T;
}

export interface ListResponse<T = null> {
    count: number,
    data: T[];
}

export interface FilterOptions {
    page: number,
    limit: number,
    sort: OrderOptions[],
    where?: { [key: string]: WhereOptions }
}

interface FilterOptionsQuery {
    page: number,
    limit: number,
    sort: string,
    where?: string
}

export interface OrderOptions {
    name: string,
    direction: "asc" | "desc"
}

export interface WhereOptions {
    equal?: string | number | boolean,
    less?: number,
    greater?: number,
    in?: number[]
}

export async function searchLego(search: string) {
    return request<LegoSet[]>("GET", "SearchLegoSet", { search: search });
}

export async function addNewLegoSet(set: LegoSet) {
    return request<unknown>("POST", "LegoSet", set);
}

export async function deleteLegoSet(id: number) {
    return request<void>("DELETE", `LegoSet/${id}`);
}

export async function addPieceToLegoSet(set: LegoSet) {
    return request<void>("POST", "InsertPiecesFromApi", set, undefined, { timeout: 60000 });
}

export async function getLegoPieceColorsFromSet(set: LegoSet) {
    return request<LegoColor[]>("GET", `LegoSet/Colors/${set.databaseId}`);
}

export async function getMyLegoSet() {
    return request<LegoSet[]>("GET", "LegoSet");
}

export async function getPieces(setId: number, filter?: FilterOptions) {
    return requestList<LegoPiece>("GET", `LegoPieces/set/${setId}`, filter, {timeout: 60000});
}

function convertInOrderString(orderOptions: OrderOptions[]): string {
    return orderOptions.map<string>(value => `${value.direction === "asc" ? "+" : "-"}${value.name}`).join(',');
}

function convertInWhereString(orderOptions?: { [key: string]: WhereOptions }): string | undefined {
    return Object.keys(orderOptions ?? {}).map<string>(key => {
        const value = orderOptions![key];
        let result = `${key}#`;

        if (value.in) {
            result += `[${value.in.join(',')}]`;
        }

        if (value.less) {
            result += `${result.length > 0 ? "|" : ""}<${value.equal === true ? "=" : ""}${value.less}`;
        }

        if (value.greater) {
            result += `${result.length > 0 ? "|" : ""}>${value.equal === true ? "=" : ""}${value.greater}`;
        }

        if (value.equal !== undefined) {
            if (value.equal !== false && value.equal !== true) {
                result += `${result.length > 0 ? "|" : ""}=${value.equal}`;
            } else if (result.length > 0) {
                throw Error("Where options parsing error");
            }
        }

        return result;
    }).join(';');
}

function convertFilterToQuery(filter: FilterOptions): FilterOptionsQuery {
    return {
        page: filter.page,
        limit: filter.limit,
        sort: convertInOrderString(filter.sort),
        where: convertInWhereString(filter.where)
    }
}

async function request<T>(method: "GET" | "POST" | "DELETE", url: string, data: object | undefined = undefined, filter: FilterOptions | undefined = undefined, axiosConfig: AxiosRequestConfig = {}): Promise<T | null> {
    let request: Promise<AxiosResponse<Response<T>>> | undefined = undefined;

    switch (method) {
        case "GET":
            if (filter) {
                data = {
                    page: filter.page,
                    limit: filter.limit,
                    sort: convertInOrderString(filter.sort),
                    ...data
                };
            }

            request = axios.get<Response<T>>(url, { params: data, ...axiosConfig });
            break;
        case "POST":
            request = axios.post<Response<T>>(url, data, axiosConfig);
            break;
        case "DELETE":
            request = axios.delete<Response<T>>(url, { params: data, ...axiosConfig });
            break;
    }

    return request.then(res => {
        const data: Response<T> = res.data;

        if (data.code == 0) {
            return data.data as T;
        }

        return null;
    }).catch((err: AxiosError<Response<T>>) => {
        console.log(err);
        throw err;
    });
}

async function requestList<T>(method: "GET", url: string, filter: FilterOptions | undefined = undefined, axiosConfig: AxiosRequestConfig = {}): Promise<ListResponse<T>> {
    let data = {};
    let request: Promise<AxiosResponse<Response<T>>> | undefined = undefined;

    switch (method) {
        case "GET":
            if (filter) {
                data = convertFilterToQuery(filter);
            }

            request = axios.get<Response<T>>(url, { params: data, ...axiosConfig });
            break;
    }

    return request.then(res => {
        const data: Response<T> = res.data;

        if (data.code == 0) {
            return {
                count: data.count!,
                data: data.data as T
            } as ListResponse<T>;
        }

        return {
            count: 0,
            data: []
        } as ListResponse<T>;
    }).catch((err: AxiosError<Response<T>>) => {
        console.log(err);
        throw err;
    });
}