import axios from "axios";

export const axiosInstance = axios.create({
    baseURL: "http://localhost:5196/",
    timeout: 1000
})