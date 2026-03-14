import { useQuery } from '@tanstack/react-query'
import { productsApi, type GetProductsParams } from '@/api/products'

export const productKeys = {
  all: ['products'] as const,
  list: (params: GetProductsParams) => ['products', 'list', params] as const,
  detail: (id: string) => ['products', 'detail', id] as const,
}

export function useProducts(params: GetProductsParams = {}) {
  return useQuery({
    queryKey: productKeys.list(params),
    queryFn: () => productsApi.list(params),
  })
}

export function useProduct(id: string) {
  return useQuery({
    queryKey: productKeys.detail(id),
    queryFn: () => productsApi.getById(id),
    enabled: !!id,
  })
}
