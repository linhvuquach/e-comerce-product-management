import { useState } from 'react'
import { Link } from 'react-router'
import { useProducts } from '../../features/products/hooks/useProducts'
import ProductTable from '../../features/products/components/ProductTable'
import EmptyState from '../../components/ui/EmptyState'

export default function ProductListPage() {
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError } = useProducts({ page, pageSize })

  if (isError) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-6 text-sm text-red-700">
        Failed to load products. Check that the API is running.
      </div>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Products</h1>
        <Link
          to="/products/new"
          className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 transition-colors"
        >
          + New Product
        </Link>
      </div>

      {!isLoading && data?.totalItems === 0 ? (
        <EmptyState
          title="No products yet"
          description="Create your first product to get started."
          action={
            <Link
              to="/products/new"
              className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700"
            >
              + New Product
            </Link>
          }
        />
      ) : (
        <ProductTable
          data={data?.data ?? []}
          isLoading={isLoading}
          page={data?.page ?? page}
          totalItems={data?.totalItems ?? 0}
          totalPages={data?.totalPages ?? 0}
          onPageChange={setPage}
        />
      )}
    </div>
  )
}
