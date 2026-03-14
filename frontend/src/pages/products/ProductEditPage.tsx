import { useParams, Link } from 'react-router'
import { useProduct } from '../../features/products/hooks/useProducts'
import ProductForm from '../../features/products/components/ProductForm'
import { FullPageSpinner } from '../../components/ui/Spinner'

export default function ProductEditPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError } = useProduct(id!)

  if (isLoading) return <FullPageSpinner />

  if (isError || !data) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-6 text-sm text-red-700">
        Failed to load product.{' '}
        <Link to="/products" className="underline">Back to list</Link>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <Link to={`/products/${id}`} className="text-sm text-brand-600 hover:underline">
          ← Back to product
        </Link>
        <h1 className="mt-2 text-2xl font-semibold text-gray-900">Edit: {data.product.name}</h1>
      </div>
      <ProductForm product={data.product} etag={data.etag} />
    </div>
  )
}
