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
        
        <div class="filter-group">
          <label>Filtriraj po pacijentu:</label>
          <select v-model="selectedPatient" @change="filterPrescriptions" class="filter-select">
            <option value="">Svi pacijenti</option>
            <option v-for="patient in patients" :key="patient.id" :value="patient.id">
              {{ patient.ime }} {{ patient.prezime }}
            </option>
          </select>
        </div>

        <div class="filter-group">
          <label>Datum od:</label>
          <input type="date" v-model="dateFrom" @change="filterPrescriptions" class="date-input">
        </div>

        <div class="filter-group">
          <label>Datum do:</label>
          <input type="date" v-model="dateTo" @change="filterPrescriptions" class="date-input">
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
            <th>ID</th>
            <th>Pacijent</th>
            <th>Datum izdavanja</th>
            <th>Broj lijekova</th>
            <th>Akcije</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="prescription in filteredPrescriptions" :key="prescription.receptId">
            <td>{{ prescription.receptId }}</td>
            <td>{{ getPatientName(prescription.pacijentId) }}</td>
            <td>{{ prescription.datumIzdavanja }}</td>
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
                <option v-for="patient in patients" :key="patient.id" :value="patient.id">
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
            <p><strong>Datum izdavanja:</strong> {{ viewingPrescription.datumIzdavanja }}</p>
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

<script>
import { prescriptionService } from '../services/prescriptionService'
import { medicineService } from '../services/medicineService'
import { patientService } from '../services/patientService'

export default {
  name: 'PrescriptionsView',
  data() {
    return {
      prescriptions: [],
      filteredPrescriptions: [],
      patients: [],
      medicines: [],
      loading: false,
      showModal: false,
      showViewModal: false,
      isEditing: false,
      currentPrescription: {
        receptId: null,
        pacijentId: '',
        datumIzdavanja: new Date().toISOString().split('T')[0],
        stavke: [{ lijekId: '', doziranje: '' }]
      },
      viewingPrescription: {},
      // Filter properties
      searchTerm: '',
      selectedPatient: '',
      dateFrom: '',
      dateTo: ''
    }
  },
  async created() {
    await this.loadData()
  },
  methods: {
    async loadData() {
      this.loading = true
      try {
        const [prescriptionsData, patientsData, medicinesData] = await Promise.all([
          prescriptionService.getAllPrescriptions(),
          patientService.getAllPatients(),
          medicineService.getAllMedicines()
        ])
        this.prescriptions = prescriptionsData || []
        this.filteredPrescriptions = this.prescriptions
        this.patients = patientsData || []
        this.medicines = medicinesData || []
      } catch (error) {
        console.error('Greška pri učitavanju podataka:', error)
        alert('Greška pri učitavanju podataka')
      } finally {
        this.loading = false
      }
    },

    getPatientName(pacijentId) {
      const patient = this.patients.find(p => p.id === pacijentId)
      return patient ? `${patient.ime} ${patient.prezime}` : 'Nepoznat pacijent'
    },

    editPrescription(prescription) {
      this.isEditing = true
      this.currentPrescription = {
        receptId: prescription.receptId,
        pacijentId: prescription.pacijentId,
        datumIzdavanja: prescription.datumIzdavanja,
        stavke: prescription.stavke && prescription.stavke.length > 0 
          ? prescription.stavke.map(s => ({ lijekId: s.lijekId, doziranje: s.doziranje }))
          : [{ lijekId: '', doziranje: '' }]
      }
      this.showModal = true
    },

    viewPrescription(prescription) {
      this.viewingPrescription = prescription
      this.showViewModal = true
    },

    addStavka() {
      this.currentPrescription.stavke.push({ lijekId: '', doziranje: '' })
    },

    removeStavka(index) {
      if (this.currentPrescription.stavke.length > 1) {
        this.currentPrescription.stavke.splice(index, 1)
      }
    },

    closeModal() {
      this.showModal = false
      this.isEditing = false
      this.currentPrescription = {
        receptId: null,
        pacijentId: '',
        datumIzdavanja: new Date().toISOString().split('T')[0],
        stavke: [{ lijekId: '', doziranje: '' }]
      }
    },

    async savePrescription() {
      try {
        const prescriptionData = {
          pacijentId: parseInt(this.currentPrescription.pacijentId),
          datumIzdavanja: this.currentPrescription.datumIzdavanja,
          stavke: this.currentPrescription.stavke
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

        if (this.isEditing) {
          await prescriptionService.updatePrescription(this.currentPrescription.receptId, prescriptionData)
        } else {
          await prescriptionService.createPrescription(prescriptionData)
        }

        await this.loadData()
        this.closeModal()
      } catch (error) {
        console.error('Greška pri spremanju recepta:', error)
        alert('Greška pri spremanju recepta')
      }
    },

    async deletePrescription(id) {
      if (confirm('Jeste li sigurni da želite obrisati ovaj recept?')) {
        try {
          await prescriptionService.deletePrescription(id)
          await this.loadData()
        } catch (error) {
          console.error('Greška pri brisanju recepta:', error)
          alert('Greška pri brisanju recepta')
        }
      }
    },

    // Filtering methods
    filterPrescriptions() {
      let filtered = [...this.prescriptions]

      // Filter by search term (patient name or OIB)
      if (this.searchTerm.trim()) {
        const searchLower = this.searchTerm.toLowerCase()
        filtered = filtered.filter(prescription => {
          const patient = this.patients.find(p => p.id === prescription.pacijentId)
          if (patient) {
            const patientName = `${patient.ime} ${patient.prezime}`.toLowerCase()
            const oib = patient.oib || ''
            return patientName.includes(searchLower) || oib.includes(searchLower)
          }
          return false
        })
      }

      // Filter by selected patient
      if (this.selectedPatient) {
        filtered = filtered.filter(prescription => 
          prescription.pacijentId === parseInt(this.selectedPatient)
        )
      }

      // Filter by date range
      if (this.dateFrom) {
        filtered = filtered.filter(prescription => 
          prescription.datumIzdavanja >= this.dateFrom
        )
      }

      if (this.dateTo) {
        filtered = filtered.filter(prescription => 
          prescription.datumIzdavanja <= this.dateTo
        )
      }

      this.filteredPrescriptions = filtered
    },

    clearFilters() {
      this.searchTerm = ''
      this.selectedPatient = ''
      this.dateFrom = ''
      this.dateTo = ''
      this.filteredPrescriptions = [...this.prescriptions]
    }
  }
}
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
  grid-template-columns: 1fr 1fr 150px 150px auto;
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
  background-color: #007bff;
  color: white;
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
.form-group select {
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
  margin-top: 20px;
  display: flex;
  gap: 10px;
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