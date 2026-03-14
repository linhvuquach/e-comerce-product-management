import { z } from 'zod'

// ── Shared ─────────────────────────────────────────────────────────────────

export const colorHexSchema = z
  .string()
  .regex(/^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$/, 'Must be a valid hex colour (e.g. #fff or #ffffff)')

// ── Product ────────────────────────────────────────────────────────────────

/**
 * Create — no status field (backend defaults to Draft).
 * Attributes are arbitrary key-value pairs stored as JSON.
 */
export const createProductSchema = z.object({
  name: z.string().min(1, 'Name is required').max(200),
  description: z.string().max(4000).optional(),
  brand: z.string().min(1, 'Brand is required').max(100),
  categoryId: z.string('Must select a valid category'),
  basePrice: z.number().positive('Price must be greater than 0'),
  currency: z.string().min(3).max(3).default('USD'),
})

export type CreateProductFormValues = z.infer<typeof createProductSchema>

export const updateProductSchema = createProductSchema

export type UpdateProductFormValues = z.infer<typeof updateProductSchema>

export const patchStatusSchema = z.object({
  status: z.enum(['Draft', 'Active', 'Archived'] as const),
})

export type PatchStatusFormValues = z.infer<typeof patchStatusSchema>

// ── Variant ────────────────────────────────────────────────────────────────

export const createVariantSchema = z.object({
  sku: z.string().min(1, 'SKU is required').max(100),
  size: z.string().min(1, 'Size is required'),
  color: z.string().min(1, 'Colour name is required').max(50),
  colorHex: colorHexSchema,
  priceOverride: z.number().positive().optional(),
})

export type CreateVariantFormValues = z.infer<typeof createVariantSchema>

// ── Category ───────────────────────────────────────────────────────────────

export const createCategorySchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  description: z.string().max(500).optional(),
  parentId: z.string().uuid().optional(),
  sortOrder: z.number().int().min(0).default(0).optional(),
})

export type CreateCategoryFormValues = z.infer<typeof createCategorySchema>
