import apiClient from './api'

export const medicineService = {
  async getAllMedicines() {
    const response = await apiClient.get('/Lijekovi')
    return response.data.data
  },

  async getMedicineById(id) {
    const response = await apiClient.get(`/Lijekovi/${id}`)
    return response.data.data
  },

  async searchMedicines(naziv) {
    const response = await apiClient.get(`/Lijekovi/search?naziv=${encodeURIComponent(naziv)}`)
    return response.data.data
  },

  async createMedicine(medicineData) {
    const response = await apiClient.post('/Lijekovi', medicineData)
    return response.data
  },

  async updateMedicine(id, medicineData) {
    const response = await apiClient.put(`/Lijekovi/${id}`, medicineData)
    return response.data
  },

  async deleteMedicine(id) {
    const response = await apiClient.delete(`/Lijekovi/${id}`)
    return response.data
  }
}