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
    
    // Transform to match frontend expectations - backend only returns static codes
    return types.map((type, index) => ({
      id: index + 1, // Generate sequential IDs (1-13)
      kod: type.sifra,
      sifra: type.sifra,
      naziv: type.naziv
    }))
  },

  // Helper method to get examination type ID from code  
  async getExaminationTypeId(code) {
    // Since backend uses hardcoded types, map codes to sequential IDs (1-13)
    const typeMapping = {
      'GP': 1, 'KRV': 2, 'X-RAY': 3, 'CT': 4, 'MR': 5, 'ULTRA': 6, 
      'EKG': 7, 'ECHO': 8, 'EYE': 9, 'DERM': 10, 'DENTA': 11, 'MAMMO': 12, 'NEURO': 13
    }
    return typeMapping[code] || 1
  }
}