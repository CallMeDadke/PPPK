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

  async searchPatients(oib = null, ime = null) {
    let params = new URLSearchParams()
    if (oib) params.append('oib', oib)
    if (ime) params.append('ime', ime)
    
    const response = await apiClient.get(`/Pacijenti/search?${params.toString()}`)
    return response.data.data
  }
}