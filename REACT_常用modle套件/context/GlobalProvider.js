// Global provider
import React, { useContext, createContext, useReducer, useEffect } from "react";
import axios from "axios";
import config from "../config"; // 假設你的 API URL 在 config.js 中

export const GlobalContext = createContext(null);

const globalReducer = (state, action) => {
    switch (action.type) {
        case "SET_USER":
            return {
                ...state,
                user: action.payload,
                isAuthenticated: true,
                isLoading: false, // 載入完成
            }
        case "LOGOUT_USER":
            return {
                ...state,
                user: null,
                isAuthenticated: false,
                isLoading: false, // 載入完成
            }
        case "SET_LOADING":
            return {
                ...state,
                isLoading: action.payload,
            }
        case "OPEN_MODAL":
            return {
                ...state,
                modals: {
                    ...state.modals,
                    [action.payload]: true,
                }
            }
        case "CLOSE_MODAL":
            return {
                ...state,
                modals: {
                    ...state.modals,
                    [action.payload]: false,
                }
            }
        default:
            return state;
    }
}

export const GlobalProvider = ({ children }) => {
    const [state, dispatch] = useReducer(globalReducer, {
        user: null,
        isAuthenticated: false,
        isLoading: true, // 初始為 true，等待檢查 localStorage
        modals: {
            loginSystem: false,
        }
    });

    const login = async (userData) => {
        dispatch({ type: "SET_LOADING", payload: true });
        try {
            dispatch({ type: "SET_USER", payload: userData });
            localStorage.setItem('user', JSON.stringify(userData));
            dispatch({ type: "CLOSE_MODAL", payload: "loginSystem" });
            console.log('Login success, saved to localStorage:', userData);
        } catch (error) {
            console.error('Login failed:', error);
            dispatch({ type: "SET_LOADING", payload: false });
        }
    };
    const register = async (userData) => {
        dispatch({ type: "SET_LOADING", payload: true });
        try {
            const response = await axios.post(
                `${config.apiBaseUrl}/schedule/register`,
               {           
                    ...userData
                }
            );
            dispatch({ type: "SET_USER", payload: response.data });
            localStorage.setItem('user', JSON.stringify(response.data));
            console.log('Registration success, saved to localStorage:', response.data);
        } catch (error) {
            console.error('Registration failed:', error);
        } finally {
            dispatch({ type: "SET_LOADING", payload: false });
        }
    }
    const logout = () => {
        dispatch({ type: "LOGOUT_USER" });
        localStorage.removeItem('user');
        // console.log('Logged out, cleared localStorage');
    };

    const checkAuth = () => {
        try {
            const savedUser = localStorage.getItem('user');
            // console.log('Checking auth from localStorage:', savedUser);
            
            if (savedUser) {
                const userData = JSON.parse(savedUser);
                dispatch({ type: "SET_USER", payload: userData });
                // console.log('Auth restored successfully:', userData);
            } else {
                console.log('No user data found in localStorage');
                dispatch({ type: "LOGOUT_USER" });
            }
        } catch (error) {
            console.error('Error checking auth:', error);
            dispatch({ type: "LOGOUT_USER" });
        }
    };

    // 🔥 關鍵：初始化時自動檢查 localStorage
    useEffect(() => {
        // console.log('GlobalProvider mounted, checking auth...');
        checkAuth();
    }, []);


    return (
        <GlobalContext.Provider value={{
            state,
            dispatch,
            login,
            logout,
            checkAuth,
            register
        }}>
            {children}
        </GlobalContext.Provider>
    );
}

export const useAuth = () => {
    const context = useContext(GlobalContext);

    if (!context) {
        throw new Error('useAuth must be used within a GlobalProvider');
    }

    const { state, login, logout, checkAuth , register } = context;

    return {
        user: state.user,
        isAuthenticated: state.isAuthenticated,
        isLoading: state.isLoading,
        modals: state.modals,
        login,
        logout,
        checkAuth
    };
};