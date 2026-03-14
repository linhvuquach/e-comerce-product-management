// T7.2 — Filter Panel (deferred; see ADR-07)
//
// This component will expose the following filter controls:
//   - Category multi-select (recursive tree, sourced from categoriesApi.tree())
//   - Price range (min / max numeric inputs or a dual-handle slider)
//   - Size chips  (multi-select from active variant sizes)
//   - Color swatches (multi-select from active variant colors)
//
// State management plan (ADR-07):
//   - Filter values will live in a Zustand `useFilterStore`
//   - The store will be URL-synced via React Router search params so filters
//     survive page refresh and are shareable via link
//   - ProductListPage will read from the store and pass params to useProducts()
//
// Backend params (already supported by GET /api/v1/products):
//   categoryId, minPrice, maxPrice, sizes[], colors[]
//
// Usage (once implemented):
//   <FilterPanel />   ← placed in ProductListPage alongside <SearchBar />

export default function FilterPanel() {
  // TODO: implement when T7.2 is scheduled
  return null
}
