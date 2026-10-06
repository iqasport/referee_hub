import React from "react";
import { NavigateFunction, NavigateOptions, To, useNavigate as routerUseNavigate, useParams, useSearchParams } from "react-router-dom";
import { useGetCurrentUserQuery } from "../store/serviceApi";

const IMPERSONATE_KEY = "impersonate";
const FEATURES_KEY = "features";

const getNavigationParamsToAppend = (currentSearch: string): string[] => {
    const urlParams = new URLSearchParams(currentSearch);
    const impersonate = urlParams.get(IMPERSONATE_KEY);
    const features = urlParams.get(FEATURES_KEY);

    const paramsToAppend: string[] = [];
    if (impersonate) {
        paramsToAppend.push(`${IMPERSONATE_KEY}=${impersonate}`);
    }
    if (features) {
        paramsToAppend.push(`${FEATURES_KEY}=${features}`);
    }

    return paramsToAppend;
};

export const applyNavigationParams = (to: To, currentSearch: string = location.search): To => {
    const paramsToAppend = getNavigationParamsToAppend(currentSearch);
    if (paramsToAppend.length === 0) {
        return to;
    }

    const queryString = paramsToAppend.join("&");

    if (typeof to === "string") {
        const startingChar = to.includes("?") ? "&" : "?";
        return `${to}${startingChar}${queryString}`;
    }

    const search = to.search || "?";
    const separator = search.endsWith("?") ? "" : "&";

    return {
        ...to,
        search: search + separator + queryString,
    };
};

export const createHref = (href: string, currentSearch: string = location.search): string => {
    return applyNavigationParams(href, currentSearch) as string;
};

export const useNavigate: typeof routerUseNavigate = () => {
    const navigate = routerUseNavigate();
    
    const customNavigate: NavigateFunction = React.useCallback(
        (to: To | number, options: NavigateOptions = {}) => {
            if (typeof to !== "number") {
                to = applyNavigationParams(to);
            }

            navigate(to as To, options);
    }, [navigate]);

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

    const shouldFetchCurrentUser = Boolean(params.refereeId);
    const { currentData: currentUser } = useGetCurrentUserQuery(undefined, {
        skip: !shouldFetchCurrentUser,
    });
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

export const useSearchParam = <T extends string = string> (key: string, options?: { defaultValue?: T, deleteIfDefault?: boolean, replaceHistory?: boolean }): [T, (value: T) => void] => {
    const [searchParams, setSearchParams] = useSearchParams();
    const value = searchParams.get(key) ?? options?.defaultValue;

    const setValue = (newValue: T) => {
        if (newValue === undefined || (options?.deleteIfDefault && newValue === options?.defaultValue)) {
            searchParams.delete(key);
        } else {
            searchParams.set(key, newValue);
        }
        // We intend to use this hook as a more persistent state, which matters for navigating away and back,
        // but less so for each state change to be recorded in the history.
        setSearchParams(searchParams, { replace: options?.replaceHistory });
    };

    return [value as T, setValue];
}
