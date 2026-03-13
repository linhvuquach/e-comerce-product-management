import { useParams } from 'react-router'
import { Link } from 'react-router'

export default function ProductDetailPage() {
  const { id } = useParams<{ id: string }>()

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Product Detail</h1>
        <Link
          to={`/products/${id}/edit`}
          className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 transition-colors"
        >
          Edit
        </Link>
      </div>
      <p className="mt-1 text-sm text-gray-500">ID: {id} — coming in Phase 6.</p>
    </div>
  )
}
