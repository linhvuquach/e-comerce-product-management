import { useState } from 'react'
import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  createColumnHelper,
} from '@tanstack/react-table'
import { Link } from 'react-router'
import type { ProductSummaryDto } from '../../../types/product'
import ProductStatusBadge from './ProductStatusBadge'
import { ConfirmModal } from '../../../components/ui/Modal'
import { Spinner } from '../../../components'
import { useDeleteProduct } from '../hooks/useProductMutations'

const col = createColumnHelper<ProductSummaryDto>()

interface ProductTableProps {
  data: ProductSummaryDto[]
  isLoading: boolean
  page: number
  totalItems: number
  totalPages: number
  onPageChange: (page: number) => void
}

export default function ProductTable({
  data,
  isLoading,
  page,
  totalItems,
  totalPages,
  onPageChange,
}: ProductTableProps) {
  const [deleteTarget, setDeleteTarget] = useState<ProductSummaryDto | null>(null)
  const { mutate: deleteProduct, isPending: isDeleting } = useDeleteProduct()

  const columns = [
    col.accessor('primaryImageUrl', {
      header: '',
      size: 56,
      cell: (info) =>
        info.getValue() ? (
          <img
            src={info.getValue()!}
            alt={info.row.original.name}
            className="h-10 w-10 rounded object-cover"
          />
        ) : (
          <div className="h-10 w-10 rounded bg-gray-100 flex items-center justify-center text-gray-300">
            <svg className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        ),
    }),
    col.accessor('name', {
      header: 'Name',
      cell: (info) => (
        <div>
          <Link
            to={`/products/${info.row.original.id}`}
            className="font-medium text-gray-900 hover:text-brand-600"
          >
            {info.getValue()}
          </Link>
          <p className="text-xs text-gray-400">{info.row.original.slug}</p>
        </div>
      ),
    }),
    col.accessor('brand', {
      header: 'Brand',
      cell: (info) => <span className="text-sm text-gray-700">{info.getValue()}</span>,
    }),
    col.accessor('categoryName', {
      header: 'Category',
      cell: (info) => <span className="text-sm text-gray-700">{info.getValue()}</span>,
    }),
    col.accessor('basePrice', {
      header: 'Price',
      cell: (info) => (
        <span className="text-sm font-medium text-gray-900">
          {info.row.original.currency} {info.getValue().toFixed(2)}
        </span>
      ),
    }),
    col.accessor('variantCount', {
      header: 'Variants',
      cell: (info) => <span className="text-sm text-gray-600">{info.getValue()}</span>,
    }),
    col.accessor('status', {
      header: 'Status',
      cell: (info) => <ProductStatusBadge status={info.getValue()} />,
    }),
    col.display({
      id: 'actions',
      header: '',
      cell: (info) => (
        <div className="flex items-center justify-end gap-2">
          <Link
            to={`/products/${info.row.original.id}/edit`}
            className="rounded px-2 py-1 text-xs font-medium text-brand-600 hover:bg-brand-50"
          >
            Edit
          </Link>
          <button
            type="button"
            onClick={() => setDeleteTarget(info.row.original)}
            className="rounded px-2 py-1 text-xs font-medium text-red-600 hover:bg-red-50"
          >
            Delete
          </button>
        </div>
      ),
    }),
  ]

  const table = useReactTable({ data, columns, getCoreRowModel: getCoreRowModel() })

  return (
    <>
      <div className="overflow-hidden rounded-lg border border-gray-200 bg-white">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            {table.getHeaderGroups().map((hg) => (
              <tr key={hg.id}>
                {hg.headers.map((h) => (
                  <th
                    key={h.id}
                    className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wide text-gray-500"
                  >
                    {flexRender(h.column.columnDef.header, h.getContext())}
                  </th>
                ))}
              </tr>
            ))}
          </thead>
          <tbody className="divide-y divide-gray-100 bg-white">
            {isLoading ? (
              <tr>
                <td colSpan={columns.length} className="py-12 text-center">
                  <Spinner className="mx-auto" />
                </td>
              </tr>
            ) : table.getRowModel().rows.length === 0 ? (
              <tr>
                <td colSpan={columns.length} className="py-12 text-center text-sm text-gray-500">
                  No products found.
                </td>
              </tr>
            ) : (
              table.getRowModel().rows.map((row) => (
                <tr key={row.id} className="hover:bg-gray-50">
                  {row.getVisibleCells().map((cell) => (
                    <td key={cell.id} className="px-4 py-3">
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>

        {/* Pagination */}
        {!isLoading && totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-gray-200 bg-white px-4 py-3">
            <p className="text-sm text-gray-600">
              {totalItems} products — page {page} of {totalPages}
            </p>
            <div className="flex gap-1">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => onPageChange(page - 1)}
                className="rounded px-3 py-1 text-sm text-gray-600 hover:bg-gray-100 disabled:opacity-40"
              >
                Previous
              </button>
              <button
                type="button"
                disabled={page >= totalPages}
                onClick={() => onPageChange(page + 1)}
                className="rounded px-3 py-1 text-sm text-gray-600 hover:bg-gray-100 disabled:opacity-40"
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      <ConfirmModal
        open={!!deleteTarget}
        title="Delete product"
        message={`Delete "${deleteTarget?.name}"? This cannot be undone.`}
        confirmLabel="Delete"
        loading={isDeleting}
        onConfirm={() => {
          if (!deleteTarget) return
          deleteProduct(deleteTarget.id, { onSuccess: () => setDeleteTarget(null) })
        }}
        onClose={() => setDeleteTarget(null)}
      />
    </>
  )
}
