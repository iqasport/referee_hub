import { SerializedError } from "@reduxjs/toolkit"
import { FetchBaseQueryError } from "@reduxjs/toolkit/dist/query"

const isSerializedError = (error: FetchBaseQueryError | SerializedError): error is SerializedError => {
    return error["message"] !== undefined;
}

export const getErrorString = (error: FetchBaseQueryError | SerializedError | null | undefined): string | null => {
    if (!error) return null;
    if (isSerializedError(error)) {
        return `${error.code} ${error.message}`;
    }

    return `${error.status} ${JSON.stringify(error.data)}`;
}