<template>
  <div class="dashboard">
    <h1>Nadzorna ploča</h1>
    
    <div class="stats-grid">
      <div class="stat-card">
        <h3>{{ patientCount }}</h3>
        <p>Pacijenti</p>
      </div>
      
      <div class="stat-card">
        <h3>{{ examinationCount }}</h3>
        <p>Pregledi</p>
      </div>
      
      <div class="stat-card">
        <h3>13</h3>
        <p>Vrste pregleda</p>
      </div>
    </div>
    
    <div class="quick-actions">
      <h2>Brze akcije</h2>
      <div class="action-buttons">
        <router-link to="/patients" class="btn btn-primary">
          Dodaj pacijenta
        </router-link>
        <router-link to="/examinations" class="btn btn-secondary">
          Dodaj pregled
        </router-link>
        <router-link to="/prescriptions" class="btn btn-secondary">
          Dodaj recept
        </router-link>
      </div>
    </div>

    <div class="export-section">
      <h2>Izvoz podataka (CSV)</h2>
      <div class="export-buttons">
        <button @click="exportData('patients')" class="btn btn-export" :disabled="exporting">
          {{ exporting === 'patients' ? 'Izvozi...' : 'Izvozi pacijente' }}
        </button>
        <button @click="exportData('examinations')" class="btn btn-export" :disabled="exporting">
          {{ exporting === 'examinations' ? 'Izvozi...' : 'Izvozi preglede' }}
        </button>
        <button @click="exportData('prescriptions')" class="btn btn-export" :disabled="exporting">
          {{ exporting === 'prescriptions' ? 'Izvozi...' : 'Izvozi recepte' }}
        </button>
        <button @click="exportData('history')" class="btn btn-export" :disabled="exporting">
          {{ exporting === 'history' ? 'Izvozi...' : 'Izvozi povijest bolesti' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script>
import { computed, ref } from 'vue'
import { usePatientsStore } from '@/stores/patients'
import { useExaminationsStore } from '@/stores/examinations'
import { exportService } from '@/services/exportService'

export default {
  name: 'DashboardView',
  setup() {
    const patientsStore = usePatientsStore()
    const examinationsStore = useExaminationsStore()
    const exporting = ref(null)
    
    const patientCount = computed(() => patientsStore.patientCount)
    const examinationCount = computed(() => examinationsStore.examinationCount)
    
    const exportData = async (type) => {
      exporting.value = type
      try {
        switch (type) {
          case 'patients':
            await exportService.exportPatients()
            break
          case 'examinations':
            await exportService.exportExaminations()
            break
          case 'prescriptions':
            await exportService.exportPrescriptions()
            break
          case 'history':
            await exportService.exportMedicalHistory()
            break
          default:
            throw new Error('Nepoznata vrsta izvoza')
        }
        // Optional: show success message
      } catch (error) {
        console.error('Greška pri izvozu:', error)
        alert('Greška pri izvozu podataka')
      } finally {
        exporting.value = null
      }
    }
    
    return {
      patientCount,
      examinationCount,
      exporting,
      exportData
    }
  }
}
</script>

<style scoped>
.dashboard {
  padding: 20px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
  margin: 20px 0;
}

.stat-card {
  background: #f5f5f5;
  padding: 20px;
  border-radius: 8px;
  text-align: center;
}

.stat-card h3 {
  font-size: 2em;
  margin: 0;
  color: #007bff;
}

.quick-actions {
  margin-top: 40px;
}

.action-buttons {
  display: flex;
  gap: 10px;
  margin-top: 10px;
}

.export-section {
  margin-top: 40px;
  padding: 20px;
  background-color: #f8f9fa;
  border-radius: 8px;
}

.export-buttons {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 10px;
  margin-top: 10px;
}

.btn {
  padding: 10px 20px;
  text-decoration: none;
  border-radius: 4px;
  display: inline-block;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-export {
  background-color: #28a745;
  color: white;
  border: none;
  cursor: pointer;
}

.btn-export:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

.btn:hover:not(:disabled) {
  opacity: 0.8;
}
</style>