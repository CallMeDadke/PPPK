import apiClient from './api'

export const examinationService = {
  async getAllExaminations() {
    const response = await apiClient.get('/Pregledi')
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async getExaminationById(id) {
    const response = await apiClient.get(`/Pregledi/${id}`)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async getExaminationsByPatientId(patientId) {
    const response = await apiClient.get(`/Pregledi/by-pacijent/${patientId}`)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async createExamination(examinationData) {
    // Transform frontend data to backend DTO format
    const createDTO = {
      pacijentId: parseInt(examinationData.pacijentId),
      vrstaPregledaId: await this.getExaminationTypeId(examinationData.vrstaPregleda),
      datumPregleda: examinationData.datumPregleda,
      doktorId: examinationData.doktorId ? parseInt(examinationData.doktorId) : null
    }
    
    const response = await apiClient.post('/Pregledi', createDTO)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async updateExamination(id, examinationData) {
    // Transform frontend data to backend DTO format
    const updateDTO = {
      pacijentId: parseInt(examinationData.pacijentId),
      vrstaPregledaId: await this.getExaminationTypeId(examinationData.vrstaPregleda),
      datumPregleda: examinationData.datumPregleda,
      doktorId: examinationData.doktorId ? parseInt(examinationData.doktorId) : null
    }
    
    const response = await apiClient.put(`/Pregledi/${id}`, updateDTO)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async deleteExamination(id) {
    const response = await apiClient.delete(`/Pregledi/${id}`)
    return response.data.data || response.data // Handle ApiResponse wrapper
  },

  async getExaminationTypes() {
    const response = await apiClient.get('/VrstePregleda')
    const types = response.data.data || response.data // Handle ApiResponse wrapper
    
    // Transform to match frontend expectations
    return types.map(type => ({
      id: type.sifra, // Use code as ID
      kod: type.sifra,
      sifra: type.sifra,
      naziv: type.naziv
    }))
  },

  // Helper method to get examination type ID from code
  async getExaminationTypeId(code) {
    const types = await this.getExaminationTypes()
    const type = types.find(t => t.kod === code)
    if (!type) {
      throw new Error(`Unknown examination type: ${code}`)
    }
    // For this API, we need to find the actual ID from the VrstePregleda table
    // Since the backend expects VrstaPregledaId (integer), we need to map codes to IDs
    const typeMapping = {
      'GP': 1, 'KRV': 2, 'X-RAY': 3, 'CT': 4, 'MR': 5, 'ULTRA': 6, 
      'EKG': 7, 'ECHO': 8, 'EYE': 9, 'DERM': 10, 'DENTA': 11, 'MAMMO': 12, 'NEURO': 13
    }
    return typeMapping[code] || 1
  }
}