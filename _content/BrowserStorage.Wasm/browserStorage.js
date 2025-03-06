// Cookie Utility Functions

export function setCookie(name, value, days) {
    const date = new Date();
    date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000)); // Convert days to milliseconds
    document.cookie = `${name}=${value};expires=${date.toUTCString()};path=/`;
}

export function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
}

export function deleteCookie(name) {
    document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/`;
}

export function getAllCookies() {
    const cookies = document.cookie.split('; ');
    const result = {};
    for (const cookie of cookies) {
        const [key, val] = cookie.split('=');
        result[key] = val;
    }
    return result;
}


// Session Storage Utility Functions

export function setSessionItem(key, value) {
    sessionStorage.setItem(key, value); // Store as a JSON string
}

export function getSessionItem(key) {
    const item = sessionStorage.getItem(key);
    return item ? item : null; 
}

export function removeSessionItem(key) {
    sessionStorage.removeItem(key);
}

export function clearSessionStorage() {
    sessionStorage.clear();
}

export function getAllSessionItems() {
    const result = {};
    for (let i = 0; i < sessionStorage.length; i++) {
        const key = sessionStorage.key(i);
        result[key] = sessionStorage.getItem(key);
    }
    return result;
}


// Local Storage Utility Functions

export function setLocalItem(key, value) {
    localStorage.setItem(key, JSON.stringify(value)); // Store as a JSON string
}

export function getLocalItem(key) {
    const item = localStorage.getItem(key);
    return item ? JSON.parse(item) : null; // Parse JSON if it exists
}

export function removeLocalItem(key) {
    localStorage.removeItem(key);
}

export function clearLocalStorage() {
    localStorage.clear();
}

export function getAllLocalItems() {
    const result = {};
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        result[key] = JSON.parse(localStorage.getItem(key));
    }
    return result;
}
