import { useParams } from 'react-router'

export default function ProductEditPage() {
  const { id } = useParams<{ id: string }>()

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900">Edit Product</h1>
      <p className="mt-1 text-sm text-gray-500">ID: {id} — coming in Phase 6.</p>
    </div>
  )
}
