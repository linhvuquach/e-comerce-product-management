import Badge from '@/components/ui/Badge'
import type { ProductStatus } from '@/types/product'

const variantMap: Record<ProductStatus, 'default' | 'success' | 'warning' | 'danger'> = {
  Draft: 'default',
  Active: 'success',
  Archived: 'danger',
}

export default function ProductStatusBadge({ status }: { status: ProductStatus }) {
  return <Badge variant={variantMap[status]}>{status}</Badge>
}
