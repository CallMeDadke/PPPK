<template>
  <div class="reports">
    <div class="header">
      <h1>Nadzorna Ploča</h1>
      <p class="subtitle">Pregled statistika i export podataka</p>
    </div>

    <!-- Statistics Overview -->
    <div class="stats-section">
      <h2>Osnovne statistike</h2>
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-icon">👥</div>
          <div class="stat-info">
            <div class="stat-number">{{ stats.totalPatients }}</div>
            <div class="stat-label">Ukupno pacijenata</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon">🏥</div>
          <div class="stat-info">
            <div class="stat-number">{{ stats.totalExaminations }}</div>
            <div class="stat-label">Ukupno pregleda</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon">💊</div>
          <div class="stat-info">
            <div class="stat-number">{{ stats.totalPrescriptions }}</div>
            <div class="stat-label">Ukupno recepta</div>
          </div>
        </div>

        <div class="stat-card">
          <div class="stat-icon">⚕️</div>
          <div class="stat-info">
            <div class="stat-number">{{ stats.totalDoctors }}</div>
            <div class="stat-label">Ukupno doktora</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Gender Distribution -->
    <div class="analysis-section">
      <h2>Razdioba po spolu</h2>
      <div class="gender-stats">
        <div class="gender-item">
          <span class="gender-label">Muški:</span>
          <span class="gender-count"
            >{{ stats.maleCount }} ({{ stats.malePercentage }}%)</span
          >
        </div>
        <div class="gender-item">
          <span class="gender-label">Ženski:</span>
          <span class="gender-count"
            >{{ stats.femaleCount }} ({{ stats.femalePercentage }}%)</span
          >
        </div>
      </div>
    </div>

    <div class="export-section">
      <h2>Izvoz podataka</h2>
      <div class="export-grid">
        <div class="export-card">
          <h3>📋 Pacijenti</h3>
          <p>Izvoz svih pacijenata s osnovnim podacima</p>
          <button
            @click="exportPatients"
            :disabled="exporting.patients"
            class="btn btn-primary"
          >
            {{ exporting.patients ? "Izvoz u tijeku..." : "Izvezi (CSV)" }}
          </button>
        </div>

        <div class="export-card">
          <h3>🏥 Pregledi</h3>
          <p>Izvoz svih pregleda s detaljima</p>
          <button
            @click="exportExaminations"
            :disabled="exporting.examinations"
            class="btn btn-primary"
          >
            {{ exporting.examinations ? "Izvoz u tijeku..." : "Izvezi (CSV)" }}
          </button>
        </div>
      </div>
    </div>

    <!-- Loading and Error States -->
    <div v-if="loading" class="loading">Učitavanje statistika...</div>

    <div v-if="error" class="error-message">
      {{ error }}
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { patientService } from "@/services/patientService";
import { prescriptionService } from "@/services/prescriptionService";
import { examinationService } from "@/services/examinationService";
import { doctorService } from "@/services/doctorService";
import { exportService } from "@/services/exportService";

const loading = ref(false);
const error = ref("");
const stats = ref({
  totalPatients: 0,
  totalExaminations: 0,
  totalPrescriptions: 0,
  totalDoctors: 0,
  maleCount: 0,
  femaleCount: 0,
  malePercentage: 0,
  femalePercentage: 0,
});

const exporting = ref({
  patients: false,
  examinations: false,
  prescriptions: false,
  history: false,
});

onMounted(() => {
  loadStatistics();
});

async function loadStatistics() {
  try {
    loading.value = true;
    error.value = "";

    // Load basic data
    const [patients, examinations, prescriptions, doctors] = await Promise.all([
      patientService.getAllPatients(),
      examinationService.getAllExaminations(),
      prescriptionService.getAllPrescriptions(),
      doctorService.getAllDoctors(),
    ]);

    // Calculate basic stats
    stats.value.totalPatients = patients.length;
    stats.value.totalExaminations = examinations.length;
    stats.value.totalPrescriptions = prescriptions.length;
    stats.value.totalDoctors = doctors.length;

    // Gender statistics
    const maleCount = patients.filter((p) => p.spol === "Muško").length;
    const femaleCount = patients.filter((p) => p.spol === "Žensko").length;
    const total = patients.length;

    stats.value.maleCount = maleCount;
    stats.value.femaleCount = femaleCount;
    stats.value.malePercentage =
      total > 0 ? Math.round((maleCount / total) * 100) : 0;
    stats.value.femalePercentage =
      total > 0 ? Math.round((femaleCount / total) * 100) : 0;
  } catch (err) {
    console.error("Error loading statistics:", err);
    error.value = "Greška pri učitavanju statistika";
  } finally {
    loading.value = false;
  }
}

const exportPatients = async () => {
  try {
    exporting.value.patients = true;
    await exportService.exportPatients();
  } catch (err) {
    console.error("Export error:", err);
    alert("Greška pri exportu pacijenata");
  } finally {
    exporting.value.patients = false;
  }
};

const exportExaminations = async () => {
  try {
    exporting.value.examinations = true;
    await exportService.exportExaminations();
  } catch (err) {
    console.error("Export error:", err);
    alert("Greška pri exportu pregleda");
  } finally {
    exporting.value.examinations = false;
  }
};
</script>

<style scoped>
.reports {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.header {
  text-align: center;
  margin-bottom: 30px;
}

.header h1 {
  color: #2c3e50;
  margin-bottom: 8px;
}

.subtitle {
  color: #7f8c8d;
  font-size: 16px;
}

.stats-section {
  margin-bottom: 40px;
}

.stats-section h2 {
  color: #2c3e50;
  margin-bottom: 20px;
  border-bottom: 2px solid #3498db;
  padding-bottom: 8px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 20px;
  margin-bottom: 30px;
}

.stat-card {
  background: #007bff;
  color: white;
  padding: 20px;
  border-radius: 8px;
  display: flex;
  align-items: center;
}

.stat-icon {
  font-size: 40px;
  margin-right: 20px;
}

.stat-number {
  font-size: 32px;
  font-weight: bold;
  margin-bottom: 5px;
}

.stat-label {
  font-size: 14px;
  opacity: 0.9;
}

.analysis-section {
  margin-bottom: 30px;
  background: white;
  padding: 25px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
}

.analysis-section h2 {
  color: #2c3e50;
  margin-bottom: 20px;
  border-bottom: 2px solid #e74c3c;
  padding-bottom: 8px;
}

.gender-stats {
  display: flex;
  gap: 30px;
}

.gender-item {
  display: flex;
  align-items: center;
  gap: 10px;
}

.gender-label {
  font-weight: bold;
  color: #2c3e50;
}

.gender-count {
  color: #27ae60;
  font-weight: bold;
}

.export-section {
  margin-bottom: 40px;
}

.export-section h2 {
  color: #2c3e50;
  margin-bottom: 25px;
  border-bottom: 2px solid #27ae60;
  padding-bottom: 8px;
}

.export-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
}

.export-card {
  background: white;
  padding: 25px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  text-align: center;
  border: 1px solid #ecf0f1;
}

.export-card h3 {
  color: #2c3e50;
  margin-bottom: 12px;
  font-size: 18px;
}

.export-card p {
  color: #7f8c8d;
  margin-bottom: 20px;
  font-size: 14px;
}

.btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

.btn:hover {
  background: #0056b3;
}

.btn:disabled {
  background: #ccc;
  cursor: not-allowed;
}

.btn-primary {
  background: #3498db;
}

.btn-primary:hover {
  background: #5dade2;
}

.loading {
  text-align: center;
  padding: 40px;
  color: #7f8c8d;
  font-size: 18px;
}

.error-message {
  text-align: center;
  padding: 20px;
  background: #e74c3c;
  color: white;
  border-radius: 8px;
  margin-top: 20px;
}

@media (max-width: 768px) {
  .reports {
    padding: 15px;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }

  .export-grid {
    grid-template-columns: 1fr;
  }

  .gender-stats {
    flex-direction: column;
    gap: 15px;
  }

  .activity-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }
}
</style>
