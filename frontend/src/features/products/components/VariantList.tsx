import type { ProductVariantDto } from '@/types/product'
import Badge from '@/components/ui/Badge'
import EmptyState from '@/components/ui/EmptyState'

interface VariantListProps {
  variants: ProductVariantDto[]
}

export default function VariantList({ variants }: VariantListProps) {
  if (variants.length === 0) {
    return (
      <EmptyState
        title="No variants"
        description="Variant management will be available in a future release."
      />
    )
  }

  return (
    <div className="overflow-hidden rounded-lg border border-gray-200">
      <table className="min-w-full divide-y divide-gray-200 text-sm">
        <thead className="bg-gray-50">
          <tr>
            {['SKU', 'Size', 'Colour', 'Price', 'Stock', 'Active'].map((h) => (
              <th
                key={h}
                className="px-4 py-3 text-left text-xs font-medium uppercase tracking-wide text-gray-500"
              >
                {h}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-gray-100 bg-white">
          {variants.map((v) => (
            <tr key={v.id} className="hover:bg-gray-50">
              <td className="px-4 py-3 font-mono text-xs text-gray-700">{v.sku}</td>
              <td className="px-4 py-3 text-gray-700">{v.size}</td>
              <td className="px-4 py-3">
                {v.color ? (
                  <div className="flex items-center gap-2">
                    {v.colorHex && (
                      <span
                        className="inline-block h-4 w-4 rounded-full border border-gray-200"
                        style={{ backgroundColor: v.colorHex }}
                        title={v.colorHex}
                      />
                    )}
                    <span className="text-gray-700">{v.color}</span>
                  </div>
                ) : (
                  <span className="text-gray-400">—</span>
                )}
              </td>
              <td className="px-4 py-3 text-gray-700">
                {v.priceOverride != null ? (
                  <span className="font-medium">{v.effectivePrice.toFixed(2)}</span>
                ) : (
                  <span className="text-gray-400">Base price</span>
                )}
              </td>
              <td className="px-4 py-3 text-gray-700">{v.stockQuantity}</td>
              <td className="px-4 py-3">
                <Badge variant={v.isActive ? 'success' : 'default'}>
                  {v.isActive ? 'Active' : 'Inactive'}
                </Badge>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <div className="border-t border-gray-200 bg-gray-50 px-4 py-2 text-xs text-gray-400">
        Variant create / edit / delete coming in a future release (T2.6 / T2.7).
      </div>
    </div>
  )
}
