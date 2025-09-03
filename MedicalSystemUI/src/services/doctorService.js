import apiClient from './api'

export const doctorService = {
  async getAllDoctors() {
    const response = await apiClient.get('/Doktori')
    return response.data.data || response.data
  },

  async getDoctorById(id) {
    const response = await apiClient.get(`/Doktori/${id}`)
    return response.data.data || response.data
  },

  async createDoctor(doctorData) {
    const response = await apiClient.post('/Doktori', doctorData)
    return response.data.data || response.data
  },

  async updateDoctor(id, doctorData) {
    const response = await apiClient.put(`/Doktori/${id}`, doctorData)
    return response.data.data || response.data
  },

  async deleteDoctor(id) {
    const response = await apiClient.delete(`/Doktori/${id}`)
    return response.data.data || response.data
  }
}