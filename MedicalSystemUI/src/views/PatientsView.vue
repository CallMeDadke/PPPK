<template>
  <div class="patients">
    <div class="header">
      <h1>Pacijenti</h1>
      <button @click="showAddForm = true" class="btn btn-primary">
        Dodaj pacijenta
      </button>
    </div>

    <!-- Search and Filters -->
    <div class="search-section">
      <div class="search-filters">
        <div class="filter-group">
          <input 
            v-model="searchTerm" 
            type="text" 
            placeholder="Pretraži po imenu, prezimenu ili OIB-u"
            class="search-input"
            @input="debouncedSearch"
          >
        </div>
        
        <div class="filter-group">
          <select v-model="genderFilter" @change="searchPatients" class="filter-select">
            <option value="">Svi spolovi</option>
            <option value="Muško">Muško</option>
            <option value="Žensko">Žensko</option>
          </select>
        </div>


        <button @click="clearFilters" class="btn btn-small">Očisti filtere</button>
      </div>
    </div>

    <!-- Add/Edit Form -->
    <div v-if="showAddForm || editingPatient" class="form-section">
      <h3>{{ editingPatient ? 'Uredi pacijenta' : 'Dodaj pacijenta' }}</h3>
      <form @submit.prevent="savePatient">
        <div class="form-group">
          <label>Ime</label>
          <input v-model="patientForm.ime" type="text" required>
        </div>
        
        <div class="form-group">
          <label>Prezime</label>
          <input v-model="patientForm.prezime" type="text" required>
        </div>
        
        <div class="form-group">
          <label>OIB</label>
          <input v-model="patientForm.oib" type="text" maxlength="11" required>
        </div>
        
        <div class="form-group">
          <label>Datum rođenja</label>
          <input v-model="patientForm.datumRodenja" type="date" required>
        </div>
        
        <div class="form-group">
          <label>Spol</label>
          <select v-model="patientForm.spol" required>
            <option value="">Odaberite...</option>
            <option value="Muško">Muško</option>
            <option value="Žensko">Žensko</option>
          </select>
        </div>
        
        <div class="form-actions">
          <button type="submit" class="btn btn-primary">Spremi</button>
          <button type="button" @click="cancelForm" class="btn btn-secondary">Odustani</button>
        </div>
      </form>
    </div>

    <!-- Patient List -->
    <div class="patient-list">
      <div v-if="loading">Učitavanje...</div>
      <div v-else-if="!patients || patients.length === 0">Nema pacijenata</div>
      <div v-else>
        <table class="patient-table">
          <thead>
            <tr>
              <th>Ime</th>
              <th>Prezime</th>
              <th>OIB</th>
              <th>Datum rođenja</th>
              <th>Spol</th>
              <th>Akcije</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="patient in patients" :key="patient.pacijentId">
              <td>{{ patient.ime }}</td>
              <td>{{ patient.prezime }}</td>
              <td>{{ patient.oib }}</td>
              <td>{{ patient.datumRodenja }}</td>
              <td>{{ patient.spol }}</td>
              <td>
                <button @click="editPatient(patient)" class="btn btn-small">Uredi</button>
                <router-link :to="`/patients/${patient.pacijentId}`" class="btn btn-small">Profil</router-link>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { usePatientsStore } from '@/stores/patients'
import { patientService } from '@/services/patientService'

const patientsStore = usePatientsStore()
const showAddForm = ref(false)
const editingPatient = ref(null)
const searchTerm = ref('')
const genderFilter = ref('')

const patientForm = ref({
  ime: '',
  prezime: '',
  oib: '',
  datumRodenja: '',
  spol: ''
})

const patients = computed(() => patientsStore.patients)
const loading = computed(() => patientsStore.loading)

const loadPatients = async () => {
  try {
    patientsStore.setLoading(true)
    const data = await patientService.getAllPatients()
    console.log('Loaded patients:', data);
    patientsStore.setPatients(data)
  } catch (error) {
    console.error('Error loading patients:', error)
  } finally {
    patientsStore.setLoading(false)
  }
}


const searchPatients = async () => {
  try {
    patientsStore.setLoading(true)
    
    // Start with all patients if no text search
    let patientsData = []
    if (searchTerm.value.trim()) {
      // Determine if search term is OIB (11 digits) or name
      const isOIB = /^\d{11}$/.test(searchTerm.value.trim())
      if (isOIB) {
        patientsData = await patientService.searchPatients(searchTerm.value.trim(), null)
      } else {
        patientsData = await patientService.searchPatients(null, searchTerm.value.trim())
      }
    } else {
      patientsData = await patientService.getAllPatients()
    }
    
    // Apply additional filters on the frontend
    let filtered = patientsData
    
    // Filter by gender
    if (genderFilter.value) {
      filtered = filtered.filter(p => p.spol === genderFilter.value)
    }
    
    
    // Enhanced text search that includes first name
    if (searchTerm.value.trim() && patientsData.length > 0) {
      const searchLower = searchTerm.value.toLowerCase()
      filtered = filtered.filter(p => {
        const fullName = `${p.ime} ${p.prezime}`.toLowerCase()
        const oib = p.oib || ''
        return fullName.includes(searchLower) || 
               oib.includes(searchLower) ||
               p.ime.toLowerCase().includes(searchLower) ||
               p.prezime.toLowerCase().includes(searchLower)
      })
    }
    
    patientsStore.setPatients(filtered)
  } catch (error) {
    console.error('Error searching patients:', error)
  } finally {
    patientsStore.setLoading(false)
  }
}

const savePatient = async () => {
  try {
    if (editingPatient.value) {
      const updated = await patientService.updatePatient(editingPatient.value.pacijentId, patientForm.value)
      patientsStore.updatePatient(updated)
    } else {
      const newPatient = await patientService.createPatient(patientForm.value)
      patientsStore.addPatient(newPatient)
    }
    cancelForm()
  } catch (error) {
    console.error('Error saving patient:', error)
  }
}

const editPatient = (patient) => {
  editingPatient.value = patient
  patientForm.value = { ...patient }
  showAddForm.value = false
}

const cancelForm = () => {
  showAddForm.value = false
  editingPatient.value = null
  patientForm.value = {
    ime: '',
    prezime: '',
    oib: '',
    datumRodenja: '',
    spol: ''
  }
}

const clearFilters = () => {
  searchTerm.value = ''
  genderFilter.value = ''
  loadPatients()
}

onMounted(async () => {
  await loadPatients()
})
</script>

<style scoped>
.patients {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.search-section {
  background-color: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.search-filters {
  display: grid;
  grid-template-columns: 2fr 1fr auto;
  gap: 15px;
  align-items: center;
}

.filter-group {
  display: flex;
  flex-direction: column;
}

.search-input,
.filter-select {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

@media (max-width: 768px) {
  .search-filters {
    grid-template-columns: 1fr;
    gap: 10px;
  }
}

.form-section {
  background: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 30px;
}

.form-group {
  margin-bottom: 15px;
}

.form-group label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group input,
.form-group select {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-actions {
  display: flex;
  gap: 10px;
}

.patient-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 20px;
}

.patient-table th,
.patient-table td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.patient-table th {
  background-color: #f5f5f5;
  font-weight: bold;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  display: inline-block;
  margin-right: 5px;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-small {
  padding: 4px 8px;
  font-size: 12px;
}

.btn:hover {
  opacity: 0.8;
}
</style>