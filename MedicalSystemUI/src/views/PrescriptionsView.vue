<template>
  <div class="prescriptions">
    <div class="header">
      <h1>Recepti</h1>
      <button @click="showModal = true" class="btn btn-primary">Dodaj novi recept</button>
    </div>

    <!-- Search and Filter Section -->
    <div class="filters-section">
      <div class="search-filters">
        <div class="filter-group">
          <label>Pretraži pacijente:</label>
          <input 
            type="text" 
            v-model="searchTerm" 
            placeholder="Unesite ime, prezime ili OIB pacijenta"
            @input="filterPrescriptions"
            class="search-input"
          >
        </div>

        <button @click="clearFilters" class="btn btn-small">Očisti filtere</button>
      </div>
    </div>

    <!-- Recepti lista -->
    <div class="prescriptions-list">
      <div v-if="loading" class="loading">Učitavanje recepti...</div>
      <div v-else-if="filteredPrescriptions.length === 0" class="no-data">
        {{ prescriptions.length === 0 ? 'Nema recepti za prikaz' : 'Nema recepti koji odgovaraju filtrima' }}
      </div>
      <table v-else class="data-table">
        <thead>
          <tr>
            <th>Pacijent</th>
            <th>OIB</th>
            <th>Datum izdavanja</th>
            <th>Broj lijekova</th>
            <th>Akcije</th>
          </tr>
        </thead>	
        <tbody>
          <tr v-for="prescription in filteredPrescriptions" :key="prescription.receptId">
            <td>{{ getPatientName(prescription.pacijentId) }}</td>
            <td>{{ getPatientOIB(prescription.pacijentId) }}</td>
            <td>{{ formatDisplayDate(prescription.datumIzdavanja) }}</td>
            <td>{{ prescription.stavke ? prescription.stavke.length : 0 }}</td>
            <td>
              <button @click="editPrescription(prescription)" class="btn btn-small">Uredi</button>
              <button @click="viewPrescription(prescription)" class="btn btn-small">Pregled</button>
              <button @click="deletePrescription(prescription.receptId)" class="btn btn-small btn-danger">Obriši</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal za dodavanje/uređivanje recepta -->
    <div v-if="showModal" class="modal-overlay" @click="closeModal">
      <div class="modal" @click.stop>
        <div class="modal-header">
          <h3>{{ isEditing ? 'Uredi recept' : 'Dodaj novi recept' }}</h3>
          <button @click="closeModal" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <form @submit.prevent="savePrescription">
            <div class="form-group">
              <label>Pacijent:</label>
              <select v-model="currentPrescription.pacijentId" required>
                <option value="">Odaberite pacijenta</option>
                <option v-for="patient in patients" :key="patient.pacijentId" :value="patient.pacijentId">
                  {{ patient.ime }} {{ patient.prezime }} ({{ patient.oib }})
                </option>
              </select>
            </div>

            <div class="form-group">
              <label>Datum izdavanja:</label>
              <input type="date" v-model="currentPrescription.datumIzdavanja" required>
            </div>

            <div class="form-group">
              <h4>Lijekovi</h4>
              <div v-for="(stavka, index) in currentPrescription.stavke" :key="index" class="medicine-item">
                <div class="medicine-row">
                  <select v-model="stavka.lijekId" required class="medicine-select">
                    <option value="">Odaberite lijek</option>
                    <option v-for="medicine in medicines" :key="medicine.lijekId" :value="medicine.lijekId">
                      {{ medicine.naziv }}
                    </option>
                  </select>
                  <input 
                    type="text" 
                    v-model="stavka.doziranje" 
                    placeholder="Doziranje (npr. 2x dnevno)"
                    required
                    class="dosage-input"
                  >
                  <button type="button" @click="removeStavka(index)" class="btn btn-small btn-danger">Ukloni</button>
                </div>
              </div>
              <button type="button" @click="addStavka" class="btn btn-small">Dodaj lijek</button>
            </div>

            <div class="form-actions">
              <button type="submit" class="btn btn-primary">{{ isEditing ? 'Spremi promjene' : 'Kreiraj recept' }}</button>
              <button type="button" @click="closeModal" class="btn">Odustani</button>
            </div>
          </form>
        </div>
      </div>
    </div>

    <!-- Modal za pregled recepta -->
    <div v-if="showViewModal" class="modal-overlay" @click="showViewModal = false">
      <div class="modal" @click.stop>
        <div class="modal-header">
          <h3>Recept #{{ viewingPrescription.receptId }}</h3>
          <button @click="showViewModal = false" class="close-btn">&times;</button>
        </div>
        <div class="modal-body">
          <div class="prescription-details">
            <p><strong>Pacijent:</strong> {{ getPatientName(viewingPrescription.pacijentId) }}</p>
            <p><strong>Datum izdavanja:</strong> {{ formatDisplayDate(viewingPrescription.datumIzdavanja) }}</p>
            <h4>Lijekovi:</h4>
            <ul v-if="viewingPrescription.stavke && viewingPrescription.stavke.length > 0">
              <li v-for="stavka in viewingPrescription.stavke" :key="stavka.stavkaReceptaId">
                <strong>{{ stavka.lijek }}</strong> - {{ stavka.doziranje }}
              </li>
            </ul>
            <p v-else>Nema lijekova na ovom receptu</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { prescriptionService } from '../services/prescriptionService'
import { medicineService } from '../services/medicineService'
import { patientService } from '../services/patientService'

const prescriptions = ref([])
const filteredPrescriptions = ref([])
const patients = ref([])
const medicines = ref([])
const loading = ref(false)
const showModal = ref(false)
const showViewModal = ref(false)
const isEditing = ref(false)
const currentPrescription = ref({
  receptId: null,
  pacijentId: '',
  datumIzdavanja: new Date().toISOString().split('T')[0],
  stavke: [{ lijekId: '', doziranje: '' }]
})
const viewingPrescription = ref({})

// Filter properties
const searchTerm = ref('')

const loadData = async () => {
  loading.value = true
  try {
    const [prescriptionsData, patientsData, medicinesData] = await Promise.all([
      prescriptionService.getAllPrescriptions(),
      patientService.getAllPatients(),
      medicineService.getAllMedicines()
    ])
    prescriptions.value = prescriptionsData || []
    filteredPrescriptions.value = prescriptions.value
    patients.value = patientsData || []
    medicines.value = medicinesData || []
  } catch (error) {
    console.error('Greška pri učitavanju podataka:', error)
    alert('Greška pri učitavanju podataka')
  } finally {
    loading.value = false
  }
}

const getPatientName = (pacijentId) => {
  const patient = patients.value.find(p => p.pacijentId === pacijentId)
  return patient ? `${patient.ime} ${patient.prezime}` : 'Nepoznat pacijent'
}

const getPatientOIB = (pacijentId) => {
  const patient = patients.value.find(p => p.pacijentId === pacijentId)
  return patient ? patient.oib : '-'
}

const editPrescription = (prescription) => {
  isEditing.value = true
  
  // Format the date properly for HTML date input
  let formattedDate = new Date().toISOString().split('T')[0];
  if (prescription.datumIzdavanja) {
    try {
      let date;
      if (prescription.datumIzdavanja.includes('T') || prescription.datumIzdavanja.includes('-')) {
        // ISO format (2024-01-15T10:30:00 or 2024-01-15)
        date = new Date(prescription.datumIzdavanja);
      } else if (prescription.datumIzdavanja.includes('.')) {
        // Croatian format (15.01.2024.)
        const dateStr = prescription.datumIzdavanja.replace(/\.$/, ''); // Remove trailing dot
        const parts = dateStr.split('.');
        if (parts.length >= 3) {
          const day = parseInt(parts[0]);
          const month = parseInt(parts[1]) - 1; // Month is 0-indexed
          const year = parseInt(parts[2]);
          date = new Date(year, month, day);
        }
      } else {
        date = new Date(prescription.datumIzdavanja);
      }
      
      if (date && !isNaN(date.getTime())) {
        formattedDate = date.toISOString().split('T')[0];
      }
    } catch (error) {
      console.warn('Could not parse date:', prescription.datumIzdavanja);
    }
  }
  
  currentPrescription.value = {
    receptId: prescription.receptId,
    pacijentId: prescription.pacijentId,
    datumIzdavanja: formattedDate,
    stavke: prescription.stavke && prescription.stavke.length > 0 
      ? prescription.stavke.map(s => ({ lijekId: s.lijekId, doziranje: s.doziranje }))
      : [{ lijekId: '', doziranje: '' }]
  }
  showModal.value = true
}

const viewPrescription = (prescription) => {
  viewingPrescription.value = prescription
  showViewModal.value = true
}

const addStavka = () => {
  currentPrescription.value.stavke.push({ lijekId: '', doziranje: '' })
}

const removeStavka = (index) => {
  if (currentPrescription.value.stavke.length > 1) {
    currentPrescription.value.stavke.splice(index, 1)
  }
}

const closeModal = () => {
  showModal.value = false
  isEditing.value = false
  currentPrescription.value = {
    receptId: null,
    pacijentId: '',
    datumIzdavanja: new Date().toISOString().split('T')[0],
    stavke: [{ lijekId: '', doziranje: '' }]
  }
}

const savePrescription = async () => {
  try {
    const prescriptionData = {
      pacijentId: parseInt(currentPrescription.value.pacijentId),
      datumIzdavanja: currentPrescription.value.datumIzdavanja,
      stavke: currentPrescription.value.stavke
        .filter(s => s.lijekId && s.doziranje.trim())
        .map(s => ({
          lijekId: parseInt(s.lijekId),
          doziranje: s.doziranje.trim()
        }))
    }

    if (prescriptionData.stavke.length === 0) {
      alert('Dodajte barem jedan lijek na recept')
      return
    }

    if (isEditing.value) {
      await prescriptionService.updatePrescription(currentPrescription.value.receptId, prescriptionData)
    } else {
      await prescriptionService.createPrescription(prescriptionData)
    }

    await loadData()
    closeModal()
  } catch (error) {
    console.error('Greška pri spremanju recepta:', error)
    alert('Greška pri spremanju recepta')
  }
}

const deletePrescription = async (id) => {
  if (confirm('Jeste li sigurni da želite obrisati ovaj recept?')) {
    try {
      await prescriptionService.deletePrescription(id)
      await loadData()
    } catch (error) {
      console.error('Greška pri brisanju recepta:', error)
      alert('Greška pri brisanju recepta')
    }
  }
}

// Filtering methods
const filterPrescriptions = () => {
  let filtered = [...prescriptions.value]

  // Filter by search term (patient name or OIB)
  if (searchTerm.value.trim()) {
    const searchLower = searchTerm.value.toLowerCase()
    filtered = filtered.filter(prescription => {
      const patient = patients.value.find(p => p.pacijentId === prescription.pacijentId)
      if (patient) {
        const fullName = `${patient.ime} ${patient.prezime}`.toLowerCase()
        const oib = patient.oib || ''
        return fullName.includes(searchLower) || 
               oib.includes(searchLower) ||
               patient.ime.toLowerCase().includes(searchLower) ||
               patient.prezime.toLowerCase().includes(searchLower)
      }
      return false
    })
  }


  filteredPrescriptions.value = filtered
}

const clearFilters = () => {
  searchTerm.value = ''
  filteredPrescriptions.value = [...prescriptions.value]
}

const formatDisplayDate = (dateStr) => {
  if (!dateStr) return '-';
  
  try {
    let date;
    if (dateStr.includes('T') || dateStr.includes('-')) {
      // ISO format - parse directly
      date = new Date(dateStr);
    } else if (dateStr.includes('.')) {
      // Already in Croatian format - return as is (remove trailing dot if present)
      return dateStr.replace(/\.$/, '');
    } else {
      date = new Date(dateStr);
    }
    
    if (date && !isNaN(date.getTime())) {
      return date.toLocaleDateString('hr-HR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
    }
  } catch (error) {
    console.warn('Could not format date:', dateStr);
  }
  
  return dateStr; // Return original if parsing fails
}

onMounted(async () => {
  await loadData()
})
</script>

<style scoped>
.prescriptions {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.filters-section {
  background-color: #f8f9fa;
  padding: 20px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.search-filters {
  display: grid;
  grid-template-columns: 1fr auto;
  gap: 15px;
  align-items: end;
}

.filter-group {
  display: flex;
  flex-direction: column;
}

.filter-group label {
  font-size: 12px;
  font-weight: bold;
  margin-bottom: 5px;
  color: #555;
}

.search-input,
.filter-select,
.date-input {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 14px;
}

.search-input {
  min-width: 250px;
}

@media (max-width: 768px) {
  .search-filters {
    grid-template-columns: 1fr;
    gap: 10px;
  }
  
  .search-input {
    min-width: auto;
  }
}

.prescriptions-list {
  margin-top: 20px;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 10px;
}

.data-table th,
.data-table td {
  padding: 10px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

.data-table th {
  background-color: #f5f5f5;
  font-weight: bold;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-right: 5px;
}

.btn-primary {
  background: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-small {
  padding: 5px 10px;
  font-size: 12px;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000;
}

.modal {
  background-color: white;
  border-radius: 8px;
  width: 90%;
  max-width: 800px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  border-bottom: 1px solid #ddd;
}

.close-btn {
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
}

.modal-body {
  padding: 20px;
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
.form-group select,
.form-group textarea {
  width: 100%;
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.medicine-item {
  margin-bottom: 10px;
  padding: 10px;
  border: 1px solid #eee;
  border-radius: 4px;
}

.medicine-row {
  display: flex;
  gap: 10px;
  align-items: center;
}

.medicine-select {
  flex: 2;
}

.dosage-input {
  flex: 1;
}

.form-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  margin-top: 20px;
}

.loading, .no-data {
  text-align: center;
  padding: 20px;
  color: #666;
}

.prescription-details p {
  margin-bottom: 10px;
}

.prescription-details ul {
  margin-left: 20px;
}

.prescription-details li {
  margin-bottom: 5px;
}
</style>