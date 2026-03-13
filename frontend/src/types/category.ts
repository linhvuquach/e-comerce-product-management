// ── DTOs ───────────────────────────────────────────────────────────────────

export interface CategoryDto {
  id: string
  name: string
  slug: string
  description: string | null
  parentId: string | null
  sortOrder: number
  children: CategoryDto[]
}

// ── Request shapes ─────────────────────────────────────────────────────────

export interface CreateCategoryRequest {
  name: string
  description?: string
  parentId?: string
  sortOrder?: number
}

export interface UpdateCategoryRequest extends CreateCategoryRequest {
  name: string
}
