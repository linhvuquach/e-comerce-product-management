import { useMutation, useQueryClient } from '@tanstack/react-query'
import { productsApi } from '@/api/products'
import { productKeys } from '@/features/products/hooks/useProducts'
import type { CreateProductRequest, UpdateProductRequest } from '@/types/product'

export function useCreateProduct() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateProductRequest) => productsApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.all })
    },
  })
}

export function useUpdateProduct(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ data, etag }: { data: UpdateProductRequest; etag: string }) =>
      productsApi.update(id, data, etag),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.detail(id) })
      qc.invalidateQueries({ queryKey: productKeys.all })
    },
  })
}

export function usePatchProductStatus(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (status: string) => productsApi.patchStatus(id, status),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.detail(id) })
      qc.invalidateQueries({ queryKey: productKeys.all })
    },
  })
}

export function useDeleteProduct() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => productsApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: productKeys.all })
    },
  })
}
