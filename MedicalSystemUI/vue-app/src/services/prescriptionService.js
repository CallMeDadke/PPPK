import apiClient from './api'

export const prescriptionService = {
  async getAllPrescriptions() {
    const response = await apiClient.get('/Recepti')
    return response.data.data
  },

  async getPrescriptionById(id) {
    const response = await apiClient.get(`/Recepti/${id}`)
    return response.data.data
  },

  async getPrescriptionsByPatient(pacijentId) {
    const response = await apiClient.get(`/Recepti/by-pacijent/${pacijentId}`)
    return response.data.data
  },

  async createPrescription(prescriptionData) {
    const response = await apiClient.post('/Recepti', prescriptionData)
    return response.data
  },

  async updatePrescription(id, prescriptionData) {
    const response = await apiClient.put(`/Recepti/${id}`, prescriptionData)
    return response.data
  },

  async deletePrescription(id) {
    const response = await apiClient.delete(`/Recepti/${id}`)
    return response.data
  }
}