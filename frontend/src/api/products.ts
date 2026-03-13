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
  minPrice?: number
  maxPrice?: number
  size?: string[]
  color?: string[]
  status?: string
  page?: number
  pageSize?: number
  sortBy?: string
  sortDir?: 'asc' | 'desc'
}

export const productsApi = {
  list: (params?: GetProductsParams) =>
    apiClient
      .get<PagedResult<ProductSummaryDto>>('/products', { params })
      .then((r) => r.data),

  getById: (id: string) =>
    apiClient
      .get<ProductDetailDto>(`/products/${id}`)
      .then((r) => r.data),

  create: (data: CreateProductRequest) =>
    apiClient
      .post<{ id: string }>('/products', data)
      .then((r) => r.data),

  update: (id: string, data: UpdateProductRequest, etag: string) =>
    apiClient
      .put<void>(`/products/${id}`, data, {
        headers: { 'If-Match': etag },
      })
      .then((r) => r.data),

  patchStatus: (id: string, status: string) =>
    apiClient
      .patch<void>(`/products/${id}`, { status })
      .then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<void>(`/products/${id}`).then((r) => r.data),
}
