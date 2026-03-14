// ── DTOs (mirrors backend camelCase response shapes) ──────────────────────

export type ProductStatus = 'Draft' | 'Active' | 'Archived'

export interface ProductVariantDto {
  id: string
  sku: string
  size: string
  color: string | null
  colorHex: string | null
  priceOverride: number | null
  effectivePrice: number
  stockQuantity: number
  isActive: boolean
  attributes: Record<string, unknown> | null
}

export interface ProductImageDto {
  id: string
  cdnUrl: string | null
  blobUrl: string
  altText: string | null
  sortOrder: number
  isPrimary: boolean
  variantId: string | null
}

export interface ProductSummaryDto {
  id: string
  name: string
  slug: string
  brand: string
  categoryId: string
  categoryName: string
  basePrice: number
  currency: string
  status: ProductStatus
  primaryImageUrl: string | null
  variantCount: number
  createdAt: string
}

export interface ProductDetailDto {
  id: string
  name: string
  slug: string
  description: string | null
  brand: string
  categoryId: string
  categoryName: string
  basePrice: number
  currency: string
  status: ProductStatus
  attributes: Record<string, unknown> | null
  variants: ProductVariantDto[]
  images: ProductImageDto[]
  createdAt: string
  updatedAt: string | null
}

/** Flat pagination envelope returned by the backend */
export interface PagedResult<T> {
  data: T[]
  totalItems: number
  page: number
  pageSize: number
  totalPages: number
}

// ── Request shapes ─────────────────────────────────────────────────────────

/** POST /api/v1/products — no status field; product starts as Draft */
export interface CreateProductRequest {
  name: string
  brand: string
  categoryId: string
  basePrice: number
  currency: string
  description?: string
  attributes?: Record<string, unknown>
}

/** PUT /api/v1/products/:id — same shape; ETag sent as If-Match header */
export type UpdateProductRequest = CreateProductRequest

export interface CreateVariantRequest {
  sku: string
  size: string
  color: string
  colorHex: string
  priceOverride?: number
  attributes?: Record<string, unknown>
}
