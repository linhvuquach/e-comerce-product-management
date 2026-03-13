import apiClient from './client'
import type { CategoryDto, CreateCategoryRequest, UpdateCategoryRequest } from '../types/category'

export const categoriesApi = {
  tree: () =>
    apiClient
      .get<CategoryDto[]>('/categories')
      .then((r) => r.data),

  getById: (id: string) =>
    apiClient
      .get<CategoryDto>(`/categories/${id}`)
      .then((r) => r.data),

  create: (data: CreateCategoryRequest) =>
    apiClient
      .post<{ id: string }>('/categories', data)
      .then((r) => r.data),

  update: (id: string, data: UpdateCategoryRequest) =>
    apiClient
      .put<void>(`/categories/${id}`, data)
      .then((r) => r.data),

  delete: (id: string) =>
    apiClient.delete<void>(`/categories/${id}`).then((r) => r.data),
}
