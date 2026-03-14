import { useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate } from 'react-router'
import { useQuery } from '@tanstack/react-query'
import Input from '@/components/ui/Input'
import Select from '@/components/ui/Select'
import Button from '@/components/ui/Button'
import { updateProductSchema } from '@/lib/schemas'
import type { ProductDetailDto } from '@/types/product'
import { categoriesApi } from '@/api/categories'
import { useCreateProduct, useUpdateProduct } from '@/features/products/hooks/useProductMutations'

// ── Flatten recursive category tree for a <select> ──────────────────────

interface FlatCategory { id: string; label: string }

function flattenTree(
  nodes: { id: string; name: string; children: any[] }[],
  depth = 0,
): FlatCategory[] {
  return nodes.flatMap((n) => [
    { id: n.id, label: `${'—'.repeat(depth)} ${n.name}`.trimStart() },
    ...flattenTree(n.children ?? [], depth + 1),
  ])
}

// ── Component ────────────────────────────────────────────────────────────

interface ProductFormProps {
  /** Provide when editing an existing product */
  product?: ProductDetailDto
  etag?: string
}

export default function ProductForm({ product, etag }: ProductFormProps) {
  const navigate = useNavigate()
  const isEdit = Boolean(product)

  const { data: categoryTree = [] } = useQuery({
    queryKey: ['categories', 'tree'],
    queryFn: categoriesApi.tree,
  })
  const categoryOptions = flattenTree(categoryTree).map((c) => ({ value: c.id, label: c.label }))

  // ── Form setup ──────────────────────────────────────────────────────────

  const { register, control, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm({
    resolver: zodResolver(updateProductSchema),
    defaultValues: isEdit && product
      ? {
          name: product.name,
          description: product.description ?? '',
          brand: product.brand,
          categoryId: product.categoryId,
          basePrice: product.basePrice,
          currency: product.currency,
        }
      : {
          currency: 'USD',
        },
  })

  // Re-populate when product loads (e.g. on direct URL navigation)
  useEffect(() => {
    if (product) {
      reset({
        name: product.name,
        description: product.description ?? '',
        brand: product.brand,
        categoryId: product.categoryId,
        basePrice: product.basePrice,
        currency: product.currency,
      })
    }
  }, [product, reset])

  // ── Mutations ───────────────────────────────────────────────────────────

  const createMutation = useCreateProduct()
  const updateMutation = useUpdateProduct(product?.id ?? '')

  const onSubmit = async (values: ReturnType<typeof updateProductSchema.parse>) => {
    const payload = {
      name: values.name,
      description: values.description,
      brand: values.brand,
      categoryId: values.categoryId,
      basePrice: values.basePrice,
      currency: values.currency,
    }

    console.log('payload', payload)

    if (isEdit) {
      await updateMutation.mutateAsync({ data: payload, etag: etag ?? '' })
      navigate(`/products/${product!.id}`)
    } else {
      const id = await createMutation.mutateAsync(payload)
      navigate(`/products/${id}`)
    }
  }

  const serverError = isEdit
    ? updateMutation.error
    : createMutation.error

  // ── Render ──────────────────────────────────────────────────────────────

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 max-w-2xl">
      {serverError && (
        <div className="rounded-lg border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          {(serverError as any)?.problem?.title ?? 'An error occurred. Please try again.'}
        </div>
      )}

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="sm:col-span-2">
          <Input
            label="Name"
            {...register('name')}
            error={errors.name?.message}
            placeholder="e.g. Classic White T-Shirt"
          />
        </div>

        <Input
          label="Brand"
          {...register('brand')}
          error={errors.brand?.message}
          placeholder="e.g. Acme Co."
        />

        <Controller
          name="categoryId"
          control={control}
          render={({ field }) => (
            <Select
              label="Category"
              options={categoryOptions}
              placeholder="Select a category"
              value={field.value ?? ''}
              onChange={field.onChange}
              onBlur={field.onBlur}
              error={errors.categoryId?.message}
            />
          )}
        />

        <Input
          label="Base Price"
          type="number"
          step="0.01"
          min="0"
          {...register('basePrice', { valueAsNumber: true })}
          error={errors.basePrice?.message}
          placeholder="0.00"
        />

        <Input
          label="Currency"
          {...register('currency')}
          error={errors.currency?.message}
          placeholder="USD"
          maxLength={3}
        />

        <div className="sm:col-span-2">
          <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
          <textarea
            rows={4}
            {...register('description')}
            className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent"
            placeholder="Optional product description…"
          />
          {errors.description && (
            <p className="mt-1 text-xs text-red-600">{errors.description.message}</p>
          )}
        </div>
      </div>

      <div className="flex items-center gap-3 pt-2">
        <Button type="submit" loading={isSubmitting}>
          {isEdit ? 'Save changes' : 'Create product'}
        </Button>
        <Button
          type="button"
          variant="secondary"
          onClick={() => navigate(isEdit ? `/products/${product!.id}` : '/')}
        >
          Cancel
        </Button>
      </div>
    </form>
  )
}
