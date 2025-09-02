import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const usePatientsStore = defineStore('patients', () => {
  const patients = ref([])
  const currentPatient = ref(null)
  const loading = ref(false)
  const error = ref(null)

  const patientCount = computed(() => patients.value.length)

  const getPatientById = computed(() => {
    return (id) => patients.value.find(patient => patient.pacijentId === id)
  })

  function setPatients(newPatients) {
    patients.value = newPatients
  }

  function addPatient(patient) {
    patients.value.push(patient)
  }

  function updatePatient(updatedPatient) {
    const index = patients.value.findIndex(p => p.pacijentId === updatedPatient.pacijentId)
    if (index !== -1) {
      patients.value[index] = updatedPatient
    }
  }

  function removePatient(patientId) {
    const index = patients.value.findIndex(p => p.pacijentId === patientId)
    if (index !== -1) {
      patients.value.splice(index, 1)
    }
  }

  function setCurrentPatient(patient) {
    currentPatient.value = patient
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
    patients,
    currentPatient,
    loading,
    error,
    patientCount,
    getPatientById,
    setPatients,
    addPatient,
    updatePatient,
    removePatient,
    setCurrentPatient,
    setLoading,
    setError,
    clearError
  }
})