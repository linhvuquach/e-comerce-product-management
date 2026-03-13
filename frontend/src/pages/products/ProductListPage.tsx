import { Link } from 'react-router'

export default function ProductListPage() {
  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-gray-900">Products</h1>
        <Link
          to="/products/new"
          className="rounded-lg bg-brand-600 px-4 py-2 text-sm font-medium text-white hover:bg-brand-700 transition-colors"
        >
          + New Product
        </Link>
      </div>
      <p className="mt-1 text-sm text-gray-500">Product list — coming in Phase 6.</p>
    </div>
  )
}
