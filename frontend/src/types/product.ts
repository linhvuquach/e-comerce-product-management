// ── DTOs (mirrors backend response shapes) ────────────────────────────────

export type ProductStatus = 'Draft' | 'Active' | 'Archived'

export interface ProductVariantDto {
  id: string
  sku: string
  size: string
  color: string
  colorHex: string
  priceOverride: number | null
  effectivePrice: number
  attributes: Record<string, string>
  isActive: boolean
}

export interface ProductImageDto {
  id: string
  cdnUrl: string
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
  basePrice: number
  currency: string
  status: ProductStatus
  primaryImageUrl: string | null
  variantCount: number
  categoryId: string
  createdAt: string
}

export interface ProductDetailDto extends ProductSummaryDto {
  description: string | null
  attributes: Record<string, string>
  variants: ProductVariantDto[]
  images: ProductImageDto[]
  updatedAt: string | null
}

export interface PagedResult<T> {
  data: T[]
  pagination: {
    page: number
    pageSize: number
    totalItems: number
    totalPages: number
  }
}

// ── Request shapes ─────────────────────────────────────────────────────────

export interface CreateProductRequest {
  name: string
  slug?: string
  description?: string
  brand: string
  categoryId: string
  basePrice: number
  currency?: string
  status: ProductStatus
  attributes?: Record<string, string>
}

export interface UpdateProductRequest extends CreateProductRequest {
  rowVersion: string
}

export interface CreateVariantRequest {
  sku: string
  size: string
  color: string
  colorHex: string
  priceOverride?: number
  attributes?: Record<string, string>
}
