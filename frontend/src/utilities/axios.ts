import axios from "axios";

const baseURL = process.env.NODE_ENV === "production" ? `http://${window.location.hostname}:5000/` : "http://localhost:5196/";

export const axiosInstance = axios.create({
    baseURL: baseURL,
    timeout: 1000
})