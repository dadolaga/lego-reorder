const getBaseUrl = () => {
    if (process.env.NODE_ENV === 'production') {
        if (typeof window === 'undefined') {
            return 'ws://localhost:5000/ws/';
        } else {
            return `ws://${window.location.hostname}:5000/ws/`;
        }
    }

    return 'ws://localhost:5196/ws/';
}

export default function createWebSocket(url: string): WebSocket {
    const baseUrl = getBaseUrl();

    return new WebSocket(`${baseUrl}${url}`);
}