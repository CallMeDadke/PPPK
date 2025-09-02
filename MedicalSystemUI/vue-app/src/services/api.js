import axios from 'axios'

const API_BASE_URL = process.env.VUE_APP_API_URL || 'https://localhost:7214/api'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  }
})

apiClient.interceptors.request.use(
  (config) => {
    console.log(`Making ${config.method?.toUpperCase()} request to ${config.url}`)
    return config
  },
  (error) => {
    console.error('Request error:', error)
    return Promise.reject(error)
  }
)

apiClient.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    console.error('Response error:', error)
    
    if (error.response) {
      console.error('Status:', error.response.status)
      console.error('Data:', error.response.data)
    }
    
    return Promise.reject(error)
  }
)

export default apiClient