import { useState } from 'react'
import { useParams, Link, useNavigate } from 'react-router'
import { useProduct } from '../../features/products/hooks/useProducts'
import { usePatchProductStatus, useDeleteProduct } from '../../features/products/hooks/useProductMutations'
import ProductStatusBadge from '../../features/products/components/ProductStatusBadge'
import VariantList from '../../features/products/components/VariantList'
import { FullPageSpinner } from '../../components/ui/Spinner'
import { ConfirmModal } from '../../components/ui/Modal'
import Button from '../../components/ui/Button'
import Select from '../../components/ui/Select'
import type { ProductStatus } from '../../types/product'

const STATUS_OPTIONS = [
  { value: 'Draft', label: 'Draft' },
  { value: 'Active', label: 'Active' },
  { value: 'Archived', label: 'Archived' },
]

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const [confirmDelete, setConfirmDelete] = useState(false)
  const [statusValue, setStatusValue] = useState<string>('')

  const { data, isLoading, isError } = useProduct(id!)
  const patchStatus = usePatchProductStatus(id!)
  const { mutate: deleteProduct, isPending: isDeleting } = useDeleteProduct()

  // Initialise local status from loaded product
  const product = data?.product
  const currentStatus = product?.status ?? 'Draft'
  const displayStatus = (statusValue || currentStatus) as ProductStatus

  if (isLoading) return <FullPageSpinner />

  if (isError || !product) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-6 text-sm text-red-700">
        Product not found.{' '}
        <Link to="/products" className="underline">Back to list</Link>
      </div>
    )
  }

  const handleStatusChange = (newStatus: string) => {
    setStatusValue(newStatus)
    patchStatus.mutate(newStatus)
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-start justify-between gap-4">
        <div>
          <Link to="/products" className="text-sm text-brand-600 hover:underline">
            ← Products
          </Link>
          <h1 className="mt-1 text-2xl font-semibold text-gray-900">{product.name}</h1>
          <p className="text-sm text-gray-400">{product.slug}</p>
        </div>
        <div className="flex shrink-0 items-center gap-2">
          <Link
            to={`/products/${id}/edit`}
            className="rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 transition-colors"
          >
            Edit
          </Link>
          <Button variant="danger" size="md" onClick={() => setConfirmDelete(true)}>
            Delete
          </Button>
        </div>
      </div>

      {/* Details card */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2 space-y-6">
          {/* Info */}
          <div className="rounded-lg border border-gray-200 bg-white p-6 space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Details</h2>
            <dl className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <dt className="text-gray-500">Brand</dt>
                <dd className="font-medium text-gray-900">{product.brand}</dd>
              </div>
              <div>
                <dt className="text-gray-500">Category</dt>
                <dd className="font-medium text-gray-900">{product.categoryName}</dd>
              </div>
              <div>
                <dt className="text-gray-500">Base Price</dt>
                <dd className="font-medium text-gray-900">
                  {product.currency} {product.basePrice.toFixed(2)}
                </dd>
              </div>
              <div>
                <dt className="text-gray-500">Created</dt>
                <dd className="font-medium text-gray-900">
                  {new Date(product.createdAt).toLocaleDateString()}
                </dd>
              </div>
              {product.description && (
                <div className="col-span-2">
                  <dt className="text-gray-500">Description</dt>
                  <dd className="mt-1 text-gray-700 whitespace-pre-wrap">{product.description}</dd>
                </div>
              )}
            </dl>
          </div>

          {/* Variants */}
          <div className="rounded-lg border border-gray-200 bg-white p-6 space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
              Variants ({product.variants.length})
            </h2>
            <VariantList variants={product.variants} />
          </div>
        </div>

        {/* Sidebar */}
        <div className="space-y-4">
          <div className="rounded-lg border border-gray-200 bg-white p-6 space-y-4">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">Status</h2>
            <div className="flex items-center gap-2">
              <ProductStatusBadge status={displayStatus} />
            </div>
            <Select
              options={STATUS_OPTIONS}
              value={statusValue || currentStatus}
              onChange={(e) => handleStatusChange(e.target.value)}
              disabled={patchStatus.isPending}
            />
            {patchStatus.isError && (
              <p className="text-xs text-red-600">
                {(patchStatus.error as any)?.problem?.title ?? 'Status update failed'}
              </p>
            )}
          </div>

          {product.images.length > 0 && (
            <div className="rounded-lg border border-gray-200 bg-white p-6 space-y-3">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-gray-500">
                Images ({product.images.length})
              </h2>
              <div className="grid grid-cols-3 gap-2">
                {product.images.map((img) => (
                  <div key={img.id} className="relative aspect-square">
                    <img
                      src={img.cdnUrl ?? img.blobUrl}
                      alt={img.altText ?? product.name}
                      className="h-full w-full rounded object-cover"
                    />
                    {img.isPrimary && (
                      <span className="absolute bottom-1 left-1 rounded bg-black/60 px-1 py-0.5 text-[10px] text-white">
                        Primary
                      </span>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      <ConfirmModal
        open={confirmDelete}
        title="Delete product"
        message={`Delete "${product.name}"? This cannot be undone.`}
        confirmLabel="Delete"
        loading={isDeleting}
        onConfirm={() =>
          deleteProduct(id!, {
            onSuccess: () => navigate('/products'),
          })
        }
        onClose={() => setConfirmDelete(false)}
      />
    </div>
  )
}
