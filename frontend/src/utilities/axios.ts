import axios from "axios";

const getBaseUrl = () => {
    if (process.env.NODE_ENV === 'production') {
        if (typeof window === 'undefined') {
            return 'http://localhost:5000/';
        } else {
            return `http://${window.location.hostname}:5000/`;
        }
    }
    
    return 'http://localhost:5196/';
}

const baseURL = getBaseUrl();

export const axiosInstance = axios.create({
    baseURL: baseURL,
    timeout: 1000
})