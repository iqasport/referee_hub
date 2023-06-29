import React from "react";
import { NavigateFunction, NavigateOptions, To, useNavigate as routerUseNavigate, useParams } from "react-router-dom";
import { useGetCurrentUserQuery } from "../store/serviceApi";

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

type NavigationParameters = {
    refereeId: string;
    testId: string;
    ngbId: string;
    importScope: string;
}

export const useNavigationParams = <Keys extends keyof NavigationParameters>() : Readonly<Partial<Pick<NavigationParameters, Keys>>> => {
    const params = useParams<NavigationParameters>();
    
    const { currentData: currentUser } = useGetCurrentUserQuery()
    const currentUserId = currentUser?.userId;

    const updatedParams: Partial<NavigationParameters> = {
        refereeId: currentUserId === params.refereeId ? "me" : params.refereeId,
        testId: params.testId,
        ngbId: params.ngbId,
        importScope: params.importScope,
    };
    return updatedParams;
}
