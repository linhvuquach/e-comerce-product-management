import { createBrowserRouter } from 'react-router'
import AppLayout from '@/layouts/AppLayout'
import ProductListPage from '@/pages/products/ProductListPage'
import ProductNewPage from '@/pages/products/ProductNewPage'
import ProductDetailPage from '@/pages/products/ProductDetailPage'
import ProductEditPage from '@/pages/products/ProductEditPage'
import CategoriesPage from '@/pages/categories/CategoriesPage'
import RouteErrorPage from '@/pages/RouteErrorPage'

const router = createBrowserRouter([
  {
    path: '/',
    element: <AppLayout />,
    errorElement: <RouteErrorPage />,
    children: [
      { index: true, element: <ProductListPage /> },
      { path: 'products', element: <ProductListPage /> },
      { path: 'products/new', element: <ProductNewPage /> },
      { path: 'products/:id', element: <ProductDetailPage /> },
      { path: 'products/:id/edit', element: <ProductEditPage /> },
      { path: 'categories', element: <CategoriesPage /> },
      { path: '*', element: <RouteErrorPage /> },
    ],
  },
])

export default router
