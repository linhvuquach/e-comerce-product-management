import apiClient from './client'
import type {
  ProductSummaryDto,
  ProductDetailDto,
  PagedResult,
  CreateProductRequest,
  UpdateProductRequest,
} from '../types/product'

export interface GetProductsParams {
  q?: string
  categoryId?: string
  status?: string
  minPrice?: number
  maxPrice?: number
  page?: number
  pageSize?: number
  sortBy?: string
  sortDescending?: boolean
}

export interface ProductWithEtag {
  product: ProductDetailDto
  etag: string
}

export const productsApi = {
  list: (params?: GetProductsParams) =>
    apiClient
      .get<PagedResult<ProductSummaryDto>>('/products', { params })
      .then((r) => r.data),

  /** Returns the product AND the ETag header needed for optimistic concurrency on PUT */
  getById: async (id: string): Promise<ProductWithEtag> => {
    const r = await apiClient.get<ProductDetailDto>(`/products/${id}`)
    // Axios normalises header names to lowercase.
    const rawEtag = r.headers['etag']
    return {
      product: r.data,
      etag: typeof rawEtag === 'string' ? rawEtag : '',
    }
  },

  /** Returns the new product ID (201 Created body) */
  create: (data: CreateProductRequest) =>
    apiClient
      .post<string>('/products', data)
      .then((r) => r.data),

  /** ETag must be the value from getById — sent as If-Match header */
  update: (id: string, data: UpdateProductRequest, etag: string) =>
    apiClient
      .put<void>(`/products/${id}`, data, {
        headers: { 'If-Match': etag },
      })
      .then(() => undefined),

  patchStatus: (id: string, status: string) =>
    apiClient
      .patch<void>(`/products/${id}/status`, { status })
      .then(() => undefined),

  delete: (id: string) =>
    apiClient.delete<void>(`/products/${id}`).then(() => undefined),
}
