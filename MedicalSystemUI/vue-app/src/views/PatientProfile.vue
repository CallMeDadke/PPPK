<template>
  <div class="patient-profile">
    <div v-if="loading">{{ $t('common.loading') }}</div>
    <div v-else-if="!patient">Pacijent nije pronađen</div>
    <div v-else>
      <div class="header">
        <button @click="$router.go(-1)" class="btn btn-secondary">Natrag</button>
        <h1>{{ patient.ime }} {{ patient.prezime }}</h1>
      </div>

      <!-- Patient Info -->
      <div class="patient-info">
        <h3>Osnovne informacije</h3>
        <div class="info-grid">
          <div class="info-item">
            <strong>Ime:</strong> {{ patient.ime }}
          </div>
          <div class="info-item">
            <strong>Prezime:</strong> {{ patient.prezime }}
          </div>
          <div class="info-item">
            <strong>OIB:</strong> {{ patient.oib }}
          </div>
          <div class="info-item">
            <strong>Datum rođenja:</strong> {{ patient.datumRodenja }}
          </div>
          <div class="info-item">
            <strong>Spol:</strong> {{ patient.spol === 'M' ? 'Muško' : 'Žensko' }}
          </div>
        </div>
      </div>

      <!-- Medical History -->
      <div class="medical-history">
        <div class="section-header">
          <h3>Medicinska povijest</h3>
          <button @click="showAddHistoryForm = true" class="btn btn-primary">Dodaj bolest</button>
        </div>

        <!-- Add/Edit History Form -->
        <div v-if="showAddHistoryForm" class="modal-overlay" @click="closeHistoryForm">
          <div class="modal" @click.stop>
            <div class="modal-header">
              <h3>{{ editingHistory ? 'Uredi povijest bolesti' : 'Dodaj povijest bolesti' }}</h3>
              <button @click="closeHistoryForm" class="btn-close">×</button>
            </div>
            
            <form @submit.prevent="saveHistory" class="history-form">
              <div class="form-group">
                <label>Naziv bolesti:</label>
                <input type="text" v-model="historyForm.nazivBolesti" required>
              </div>

              <div class="form-group">
                <label>Datum početka:</label>
                <input type="date" v-model="historyForm.datumPocetka" required>
              </div>

              <div class="form-group">
                <label>Datum završetka:</label>
                <input type="date" v-model="historyForm.datumZavrsetka">
                <small>Ostavite prazno ako je bolest u tijeku</small>
              </div>

              <div class="form-actions">
                <button type="button" @click="closeHistoryForm" class="btn btn-secondary">Odustani</button>
                <button type="submit" class="btn btn-primary">{{ editingHistory ? 'Spremi' : 'Dodaj' }}</button>
              </div>
            </form>
          </div>
        </div>

        <div v-if="medicalHistory.length === 0" class="no-data">Nema zabilježene medicinske povijesti</div>
        <div v-else>
          <div v-for="history in medicalHistory" :key="history.id" class="history-item">
            <div class="history-content">
              <div><strong>{{ history.nazivBolesti }}</strong></div>
              <div>{{ history.datumPocetka }} - 
                {{ history.datumZavrsetka ? history.datumZavrsetka : 'U tijeku' }}
              </div>
            </div>
            <div class="history-actions">
              <button @click="editHistory(history)" class="btn-edit">Uredi</button>
              <button @click="deleteHistory(history.id)" class="btn-delete">Obriši</button>
            </div>
          </div>
        </div>
      </div>

      <!-- Examinations -->
      <div class="examinations">
        <h3>Povјesnica pregleda</h3>
        <div v-if="examinations.length === 0">Nema zabilježenih pregleda</div>
        <div v-else>
          <table class="examinations-table">
            <thead>
              <tr>
                <th>Datum</th>
                <th>Vrsta pregleda</th>
                <th>Doktor</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="examination in examinations" :key="examination.pregledId">
                <td>{{ examination.datumVrijeme }}</td>
                <td>{{ examination.vrstaPregleda?.naziv || 'N/A' }}</td>
                <td>{{ examination.doktor?.ime }} {{ examination.doktor?.prezime }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Prescriptions -->
      <div class="prescriptions">
        <h3>Recepti</h3>
        <div v-if="prescriptions.length === 0">Nema zabilježenih recepata</div>
        <div v-else>
          <div v-for="prescription in prescriptions" :key="prescription.receptId" class="prescription-item">
            <div><strong>Datum:</strong> {{ prescription.datum }}</div>
            <div><strong>Doktor:</strong> {{ prescription.doktor?.ime }} {{ prescription.doktor?.prezime }}</div>
            <div class="medications">
              <div v-for="item in prescription.stavkeRecepta" :key="item.id" class="medication">
                {{ item.lijek?.naziv }} - {{ item.kolicina }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { usePatientsStore } from '@/stores/patients'
import { patientService } from '@/services/patientService'
import { medicalHistoryService } from '@/services/medicalHistoryService'

export default {
  name: 'PatientProfile',
  setup() {
    const route = useRoute()
    const patientsStore = usePatientsStore()
    const loading = ref(false)
    const medicalHistory = ref([])
    const examinations = ref([])
    const prescriptions = ref([])
    const showAddHistoryForm = ref(false)
    const editingHistory = ref(null)
    const historyForm = ref({
      nazivBolesti: '',
      datumPocetka: '',
      datumZavrsetka: ''
    })
    
    const patientId = route.params.id
    const patient = computed(() => patientsStore.currentPatient)

    onMounted(async () => {
      await loadPatientData()
    })

    const loadPatientData = async () => {
      try {
        loading.value = true
        const patientData = await patientService.getPatientById(patientId)
        patientsStore.setCurrentPatient(patientData)
        
        // Load medical history
        const historyData = await medicalHistoryService.getMedicalHistoryByPatientId(patientId)
        medicalHistory.value = historyData
        
        // For now, set other arrays empty (to be implemented later)
        examinations.value = patientData.pregledi || []
        prescriptions.value = patientData.recepti || []
        
      } catch (error) {
        console.error('Error loading patient:', error)
      } finally {
        loading.value = false
      }
    }

    const closeHistoryForm = () => {
      showAddHistoryForm.value = false
      editingHistory.value = null
      resetHistoryForm()
    }

    const resetHistoryForm = () => {
      historyForm.value = {
        nazivBolesti: '',
        datumPocetka: '',
        datumZavrsetka: ''
      }
    }

    const saveHistory = async () => {
      try {
        const historyData = {
          ...historyForm.value,
          pacijentId: patientId,
          datumPocetka: new Date(historyForm.value.datumPocetka).toISOString(),
          datumZavrsetka: historyForm.value.datumZavrsetka 
            ? new Date(historyForm.value.datumZavrsetka).toISOString() 
            : null
        }

        if (editingHistory.value) {
          await medicalHistoryService.updateMedicalHistory(editingHistory.value.id, historyData)
        } else {
          await medicalHistoryService.createMedicalHistory(historyData)
        }

        // Reload medical history
        const historyData2 = await medicalHistoryService.getMedicalHistoryByPatientId(patientId)
        medicalHistory.value = historyData2
        closeHistoryForm()
      } catch (error) {
        console.error('Error saving medical history:', error)
        alert('Greška pri spremanju medicinske povijesti')
      }
    }

    const editHistory = (history) => {
      editingHistory.value = history
      historyForm.value = {
        nazivBolesti: history.nazivBolesti,
        datumPocetka: new Date(history.datumPocetka).toISOString().split('T')[0],
        datumZavrsetka: history.datumZavrsetka 
          ? new Date(history.datumZavrsetka).toISOString().split('T')[0] 
          : ''
      }
      showAddHistoryForm.value = true
    }

    const deleteHistory = async (id) => {
      if (confirm('Jeste li sigurni da želite obrisati ovu povijest bolesti?')) {
        try {
          await medicalHistoryService.deleteMedicalHistory(id)
          // Reload medical history
          const historyData = await medicalHistoryService.getMedicalHistoryByPatientId(patientId)
          medicalHistory.value = historyData
        } catch (error) {
          console.error('Error deleting medical history:', error)
          alert('Greška pri brisanju medicinske povijesti')
        }
      }
    }

    return {
      patient,
      loading,
      medicalHistory,
      examinations,
      prescriptions,
      showAddHistoryForm,
      editingHistory,
      historyForm,
      closeHistoryForm,
      saveHistory,
      editHistory,
      deleteHistory,
    }
  }
}
</script>

<style scoped>
.patient-profile {
  padding: 20px;
}

.header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 30px;
}

.patient-info,
.medical-history,
.examinations,
.prescriptions {
  background: #f9f9f9;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 15px;
  margin-top: 15px;
}

.info-item {
  padding: 10px;
  background: white;
  border-radius: 4px;
}

.history-item {
  padding: 10px;
  background: white;
  border-radius: 4px;
  margin-bottom: 10px;
}

.examinations-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 15px;
  background: white;
}

.examinations-table th,
.examinations-table td {
  padding: 10px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.examinations-table th {
  background-color: #f5f5f5;
}

.prescription-item {
  padding: 15px;
  background: white;
  border-radius: 4px;
  margin-bottom: 15px;
}

.medications {
  margin-top: 10px;
}

.medication {
  padding: 5px 0;
  border-bottom: 1px solid #eee;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  text-decoration: none;
  display: inline-block;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn:hover {
  opacity: 0.8;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background: white;
  padding: 20px;
  border-radius: 8px;
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.btn-close {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
}

.history-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-group label {
  margin-bottom: 5px;
  font-weight: bold;
}

.form-group input {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-group small {
  color: #666;
  font-size: 0.9em;
  margin-top: 5px;
}

.form-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  margin-top: 20px;
}

.history-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  background: white;
  border-radius: 4px;
  margin-bottom: 10px;
}

.history-content {
  flex: 1;
}

.history-actions {
  display: flex;
  gap: 5px;
}

.btn-edit {
  background: #28a745;
  color: white;
  border: none;
  padding: 5px 10px;
  border-radius: 3px;
  cursor: pointer;
  font-size: 0.9em;
}

.btn-delete {
  background: #dc3545;
  color: white;
  border: none;
  padding: 5px 10px;
  border-radius: 3px;
  cursor: pointer;
  font-size: 0.9em;
}

.no-data {
  text-align: center;
  color: #666;
  font-style: italic;
  padding: 20px;
}
</style>