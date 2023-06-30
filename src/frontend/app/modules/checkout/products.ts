import { createSlice, PayloadAction } from "@reduxjs/toolkit";

import { getProducts as getProductsApi, ProductsResponse } from "../../apis/checkout";
import { GetProductsSchema } from "../../schemas/getProductsSchema";
import { AppThunk } from "../../store";

export interface ProductsState {
  products: GetProductsSchema[];
  error: string | null;
}

const initialState: ProductsState = {
  error: null,
  products: [],
};

const products = createSlice({
  initialState,
  name: "products",
  reducers: {
    getProductsSuccess(state: ProductsState, action: PayloadAction<ProductsResponse>) {
      state.error = null;
      state.products = action.payload.products;
    },
    getProductsFailure(state: ProductsState, action: PayloadAction<string>) {
      state.error = action.payload;
      state.products = [];
    },
  },
});

const { getProductsFailure, getProductsSuccess } = products.actions;

export const getProducts = (): AppThunk => async (dispatch) => {
  try {
    const productsResponse = await getProductsApi();
    dispatch(getProductsSuccess(productsResponse));
  } catch (err) {
    dispatch(getProductsFailure(err.toString()));
  }
};

export default products.reducer;
