<template>
  <div class="patients">
    <div class="header">
      <h1>Pacijenti</h1>
      <button @click="showAddForm = true" class="btn btn-primary">
        Dodaj pacijenta
      </button>
    </div>

    <!-- Search -->
    <div class="search-section">
      <input 
        v-model="searchTerm" 
        type="text" 
        placeholder="Pretraži po prezimenu ili OIB-u"
        class="search-input"
        @input="searchPatients"
      >
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
      <div v-else-if="!patients">Nema pacijenata</div>
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

<script>
import { ref, onMounted, computed } from 'vue'
import { usePatientsStore } from '@/stores/patients'
import { patientService } from '@/services/patientService'

export default {
  name: 'PatientsView',
  setup() {
    const patientsStore = usePatientsStore()
    const showAddForm = ref(false)
    const editingPatient = ref(null)
    const searchTerm = ref('')
    
    const patientForm = ref({
      ime: '',
      prezime: '',
      oib: '',
      datumRodenja: '',
      spol: ''
    })

    const patients = computed(() => patientsStore.patients)
    const loading = computed(() => patientsStore.loading)

    onMounted(async () => {
      await loadPatients()
    })

    const loadPatients = async () => {
      try {
        patientsStore.setLoading(true)
        const data = await patientService.getAllPatients()
		console.log(data.data);
        patientsStore.setPatients(data.data)
      } catch (error) {
        console.error('Error loading patients:', error)
      } finally {
        patientsStore.setLoading(false)
      }
    }

    const searchPatients = async () => {
      if (!searchTerm.value.trim()) {
        await loadPatients()
        return
      }
      
      try {
        patientsStore.setLoading(true)
        const data = await patientService.searchPatients(searchTerm.value)
        patientsStore.setPatients(data)
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

    return {
      showAddForm,
      editingPatient,
      searchTerm,
      patientForm,
      patients,
      loading,
      savePatient,
      editPatient,
      cancelForm,
      searchPatients
    }
  }
}
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
  margin-bottom: 20px;
}

.search-input {
  width: 100%;
  max-width: 400px;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
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