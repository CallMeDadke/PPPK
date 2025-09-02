import apiClient from './api'

export const patientService = {
  async getAllPatients() {
    const response = await apiClient.get('/Pacijenti')
    return response.data.data
  },

  async getPatientById(id) {
    const response = await apiClient.get(`/Pacijenti/${id}`)
    return response.data
  },

  async createPatient(patientData) {
    const response = await apiClient.post('/Pacijenti', patientData)
    return response.data
  },

  async updatePatient(id, patientData) {
    const response = await apiClient.put(`/Pacijenti/${id}`, patientData)
    return response.data
  },

  async deletePatient(id) {
    const response = await apiClient.delete(`/Pacijenti/${id}`)
    return response.data
  },

  async searchPatients(searchTerm) {
    const response = await apiClient.get(`/Pacijenti/search?term=${encodeURIComponent(searchTerm)}`)
    return response.data
  }
}