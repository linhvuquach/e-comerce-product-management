import axios, { type AxiosError } from 'axios'

export interface ProblemDetails {
  type?: string
  title: string
  status: number
  detail?: string
  instance?: string
  errors?: Record<string, string[]>
}

export interface ApiError {
  problem: ProblemDetails
  status: number
}

const apiClient = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' },
  timeout: 15_000, // 15 seconds
})

// Normalise all error responses to RFC 7807 ProblemDetails shape
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const status = error.response?.status ?? 0
    const data = error.response?.data as Partial<ProblemDetails> | undefined

    const problem: ProblemDetails = {
      title: data?.title ?? error.message ?? 'An unexpected error occurred',
      status,
      detail: data?.detail,
      instance: data?.instance,
      errors: data?.errors,
    }

    return Promise.reject({ problem, status } satisfies ApiError)
  },
)

export default apiClient
