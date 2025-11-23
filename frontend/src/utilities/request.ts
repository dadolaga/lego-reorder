import { axiosInstance as axios } from '@/utilities/axios'
import { AxiosError, AxiosRequestConfig, AxiosResponse } from 'axios';
import { LegoPiece, LegoSet } from './type';

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
    sort: OrderOptions[]
}

export interface OrderOptions {
    name: string,
    direction: "asc" | "desc"
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

export async function getMyLegoSet() {
    return request<LegoSet[]>("GET", "LegoSet");
}

export async function getPieces(setId: number, filter?: FilterOptions) {
    return requestList<LegoPiece>("GET", `LegoPieces/set/${setId}`, filter);
}

function convertInOrderString(orderOptions: OrderOptions[]): string {
    return orderOptions.map<string>(value => `${value.direction === "asc" ? "+" : "-"}${value.name}`).join(',');
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
                data = {
                    page: filter.page,
                    limit: filter.limit,
                    sort: convertInOrderString(filter.sort)
                };
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