import React from "react";
import { NavigateFunction, NavigateOptions, To, useNavigate as routerUseNavigate, useParams } from "react-router-dom";
import { useGetCurrentUserQuery } from "../store/serviceApi";

export const useNavigate: typeof routerUseNavigate = () => {
    const navigate = routerUseNavigate();
    
    const customNavigate: NavigateFunction = React.useCallback(
        (to: To | number, options: NavigateOptions = {}) => {
            if (typeof to !== "number") {
                const urlParams = new URLSearchParams(location.search);
                const impersonateKey = "impersonate";
                const featuresKey = "features";
                const impersonate = urlParams.get(impersonateKey);
                const features = urlParams.get(featuresKey);
                
                // Build query params to append
                const paramsToAppend: string[] = [];
                if (impersonate) {
                    paramsToAppend.push(`${impersonateKey}=${impersonate}`);
                }
                if (features) {
                    paramsToAppend.push(`${featuresKey}=${features}`);
                }
                
                if (paramsToAppend.length > 0) {
                    const queryString = paramsToAppend.join('&');
                    
                    if (typeof to === "string") {
                        const startingChar = to.includes('?') ? '&' : '?';
                        to = `${to}${startingChar}${queryString}`;
                    } else {
                        to.search = to.search || "?";
                        const separator = to.search.endsWith('?') ? '' : '&';
                        to.search += separator + queryString;
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
    scopeId: string;
    tournamentId: string; // added for usage in tournament details page
    teamId: string; // added for usage in team view page
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
        scopeId: params.scopeId,
        tournamentId: params.tournamentId, //added for usage in tournament details page
        teamId: params.teamId, // added for usage in team view page
    };
    return updatedParams;
}
