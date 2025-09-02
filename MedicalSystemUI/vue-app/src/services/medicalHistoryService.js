import apiClient from './api'

export const medicalHistoryService = {
  async getMedicalHistoryByPatientId(patientId) {
    const response = await apiClient.get(`/PovijestiBolestiAPI/by-pacijent/${patientId}`)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async createMedicalHistory(historyData) {
    // Transform frontend data to backend DTO format
    const createDTO = {
      pacijentId: parseInt(historyData.pacijentId),
      nazivBolesti: historyData.nazivBolesti,
      datumPocetka: historyData.datumPocetka,
      datumZavrsetka: historyData.datumZavrsetka || null
    }
    
    const response = await apiClient.post('/PovijestiBolestiAPI', createDTO)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async updateMedicalHistory(id, historyData) {
    // Transform frontend data to backend DTO format
    const updateDTO = {
      pacijentId: parseInt(historyData.pacijentId),
      nazivBolesti: historyData.nazivBolesti,
      datumPocetka: historyData.datumPocetka,
      datumZavrsetka: historyData.datumZavrsetka || null
    }
    
    const response = await apiClient.put(`/PovijestiBolestiAPI/${id}`, updateDTO)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async deleteMedicalHistory(id) {
    const response = await apiClient.delete(`/PovijestiBolestiAPI/${id}`)
    return response.data.data || response.data // Handle ApiResponse wrapper
  }
}