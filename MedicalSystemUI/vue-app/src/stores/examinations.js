import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useExaminationsStore = defineStore('examinations', () => {
  const examinations = ref([])
  const examinationTypes = ref([])
  const currentExamination = ref(null)
  const loading = ref(false)
  const error = ref(null)

  const examinationCount = computed(() => examinations.value.length)

  const getExaminationsByPatientId = computed(() => {
    return (patientId) => examinations.value.filter(exam => exam.pacijentId === patientId)
  })

  function setExaminations(newExaminations) {
    examinations.value = newExaminations
  }

  function setExaminationTypes(types) {
    examinationTypes.value = types
  }

  function addExamination(examination) {
    examinations.value.push(examination)
  }

  function updateExamination(updatedExamination) {
    const index = examinations.value.findIndex(e => e.pregledId === updatedExamination.pregledId)
    if (index !== -1) {
      examinations.value[index] = updatedExamination
    }
  }

  function removeExamination(examinationId) {
    const index = examinations.value.findIndex(e => e.pregledId === examinationId)
    if (index !== -1) {
      examinations.value.splice(index, 1)
    }
  }

  function setCurrentExamination(examination) {
    currentExamination.value = examination
  }

  function setLoading(isLoading) {
    loading.value = isLoading
  }

  function setError(errorMessage) {
    error.value = errorMessage
  }

  function clearError() {
    error.value = null
  }

  return {
    examinations,
    examinationTypes,
    currentExamination,
    loading,
    error,
    examinationCount,
    getExaminationsByPatientId,
    setExaminations,
    setExaminationTypes,
    addExamination,
    updateExamination,
    removeExamination,
    setCurrentExamination,
    setLoading,
    setError,
    clearError
  }
})