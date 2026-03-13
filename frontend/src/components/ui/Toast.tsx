import { useEffect } from 'react'

type ToastVariant = 'success' | 'error' | 'warning' | 'info'

export interface ToastMessage {
  id: string
  variant: ToastVariant
  title: string
  description?: string
}

const variantClasses: Record<ToastVariant, string> = {
  success: 'border-green-200 bg-green-50 text-green-800',
  error:   'border-red-200 bg-red-50 text-red-800',
  warning: 'border-yellow-200 bg-yellow-50 text-yellow-800',
  info:    'border-blue-200 bg-blue-50 text-blue-800',
}

const icons: Record<ToastVariant, string> = {
  success: '✓',
  error:   '✕',
  warning: '⚠',
  info:    'ℹ',
}

interface ToastItemProps {
  toast: ToastMessage
  onDismiss: (id: string) => void
  duration?: number
}

export function ToastItem({ toast, onDismiss, duration = 4000 }: ToastItemProps) {
  useEffect(() => {
    const timer = setTimeout(() => onDismiss(toast.id), duration)
    return () => clearTimeout(timer)
  }, [toast.id, onDismiss, duration])

  return (
    <div className={`flex items-start gap-3 rounded-lg border px-4 py-3 shadow-sm min-w-72 ${variantClasses[toast.variant]}`}>
      <span className="mt-0.5 shrink-0 text-sm font-bold">{icons[toast.variant]}</span>
      <div className="flex-1 text-sm">
        <p className="font-medium">{toast.title}</p>
        {toast.description && <p className="mt-0.5 opacity-80">{toast.description}</p>}
      </div>
      <button
        type="button"
        onClick={() => onDismiss(toast.id)}
        className="shrink-0 opacity-60 hover:opacity-100"
        aria-label="Dismiss"
      >
        <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
          <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>
  )
}

interface ToastContainerProps {
  toasts: ToastMessage[]
  onDismiss: (id: string) => void
}

export function ToastContainer({ toasts, onDismiss }: ToastContainerProps) {
  if (toasts.length === 0) return null

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      {toasts.map((t) => (
        <ToastItem key={t.id} toast={t} onDismiss={onDismiss} />
      ))}
    </div>
  )
}
