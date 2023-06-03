import { GetProductsSchema } from "../schemas/getProductsSchema";
import { baseAxios } from "./utils";

export interface ProductsResponse {
  products: GetProductsSchema[];
}

export interface SessionResponse {
  sessionId: string;
}

export interface CreateSessionRequest {
  certificationId: string;
  price: string;
}

export async function getProducts(): Promise<ProductsResponse> {
  const url = "checkouts/products";

  try {
    const productsResponse = await baseAxios.get(url);

    return {
      products: productsResponse.data,
    };
  } catch (err) {
    throw err;
  }
}

export async function createSession(session: CreateSessionRequest): Promise<SessionResponse> {
  const url = "checkouts";

  try {
    const sessionResponse = await baseAxios.post<SessionResponse>(url, { ...session });

    return {
      sessionId: sessionResponse.data.sessionId,
    };
  } catch (err) {
    throw err;
  }
}
