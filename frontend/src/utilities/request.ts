import { axiosInstance as axios } from '@/utilities/axios'
import { AxiosError, AxiosResponse } from 'axios';
import { LegoSet } from './type';

export interface Response<T = null> {
    code: number,
    message: string,
    data: T;
}

export async function searchLego(search: string) {
    return request<LegoSet[]>("GET", "SearchLegoSet", { search: search });
}

export async function addNewLegoSet(set: LegoSet) {
    return request<unknown>("POST", "LegoSet", set);
}

export async function addPieceToLegoSet(set: LegoSet) {
    return request<void>("POST", "InsertPiecesFromApi", set);
}

export async function getMyLegoSet() {
    return request<LegoSet[]>("GET", "LegoSet");
}

async function request<T>(method: "GET" | "POST", url: string, data: unknown = undefined): Promise<T | null> {
    let request: Promise<AxiosResponse<Response<T>>> | undefined = undefined;

    switch (method) {
        case "GET":
            request = axios.get<Response<T>>(url, { params: data });
            break;
        case "POST":
            request = axios.post<Response<T>>(url, data);
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