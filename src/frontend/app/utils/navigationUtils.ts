import React from "react";
import { NavigateFunction, NavigateOptions, To, useNavigate as routerUseNavigate } from "react-router-dom";

export const useNavigate: typeof routerUseNavigate = () => {
    const navigate = routerUseNavigate();
    
    const customNavigate: NavigateFunction = React.useCallback(
        (to: To | number, options: NavigateOptions = {}) => {
            if (typeof to !== "number") {
                const impersonateKey = "impersonate";
                const impersonate = new URLSearchParams(location.search).get(impersonateKey);
                if (impersonate) {
                    if (typeof to === "string") {
                        const startingChar = to.includes('?') ? '&' : '?';
                        to = `${to}${startingChar}${impersonateKey}=${impersonate}`;
                    } else {
                        to.search = to.search || "?";
                        to.search += to.search.includes('&') ? "&" : "";
                        to.search += `${impersonateKey}=${impersonate}`;
                    }
                }
            }

            navigate(to as To, options);
    }, []);

    return customNavigate;
}