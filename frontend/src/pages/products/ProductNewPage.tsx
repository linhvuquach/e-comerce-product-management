import ProductForm from '@/features/products/components/ProductForm'

export default function ProductNewPage() {
  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold text-gray-900">New Product</h1>
      <ProductForm />
    </div>
  )
}
