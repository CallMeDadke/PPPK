<template>
  <div class="examinations">
    <div class="header">
      <h1>Pregledi</h1>
      <button @click="showAddForm = true" class="btn-primary">
        Dodaj Pregled
      </button>
    </div>

    <div v-if="loading" class="loading">Učitavanje...</div>

    <div v-if="showAddForm" class="modal-overlay" @click="closeAddForm">
      <div class="modal" @click.stop>
        <div class="modal-header">
          <h2>{{ editingExamination ? "Uredi Pregled" : "Dodaj Pregled" }}</h2>
          <button @click="closeAddForm" class="btn-close">×</button>
        </div>

        <form @submit.prevent="saveExamination" class="examination-form">
          <div class="form-group">
            <label>Pacijent:</label>
            <select v-model="form.pacijentId" required>
              <option value="">Odaberite pacijenta</option>
              <option
                v-for="patient in patients"
                :key="patient.id"
                :value="patient.id"
              >
                {{ patient.ime }} {{ patient.prezime }} ({{ patient.oib }})
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Doktor:</label>
            <select v-model="form.doktorId" required>
              <option value="">Odaberite doktora</option>
              <option
                v-for="doctor in doctors"
                :key="doctor.doktorId"
                :value="doctor.doktorId"
              >
                {{ doctor.ime }} {{ doctor.prezime }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Vrsta pregleda:</label>
            <select v-model="form.vrstaPregleda" required>
              <option value="">Odaberite vrstu</option>
              <option
                v-for="type in examinationTypes"
                :key="type.kod"
                :value="type.kod"
              >
                {{ type.naziv }}
              </option>
            </select>
          </div>

          <div class="form-group">
            <label>Datum i vrijeme:</label>
            <input
              type="datetime-local"
              v-model="form.datumPregleda"
              required
            />
          </div>

          <div v-if="isImagingExamination" class="form-group">
            <label>Medicinske slike:</label>
            <input
              type="file"
              @change="handleFileUpload"
              multiple
              accept="image/*,.pdf"
              class="file-input"
            />
            <small>Podržani formati: slike (JPG, PNG) i PDF dokumente</small>
            <div v-if="uploadedFiles.length > 0" class="uploaded-files">
              <h4>Učitane datoteke:</h4>
              <div
                v-for="(file, index) in uploadedFiles"
                :key="index"
                class="file-item"
              >
                <span>{{ file.name }}</span>
                <button
                  type="button"
                  @click="removeFile(index)"
                  class="btn-remove-file"
                >
                  ×
                </button>
              </div>
            </div>
          </div>

          <div class="form-actions">
            <button type="button" @click="closeAddForm" class="btn-secondary">
              Odustani
            </button>
            <button type="submit" class="btn-primary">
              {{ editingExamination ? "Spremi" : "Dodaj" }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <div class="examinations-list">
      <table v-if="examinations.length > 0">
        <thead>
          <tr>
            <th>Datum</th>
            <th>Pacijent</th>
            <th>Doktor</th>
            <th>Vrsta</th>
            <th>Akcije</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="examination in examinations" :key="examination.pregledId">
            <td>{{ examination.datumPregleda }}</td>
            <td>{{ getPatientName(examination.pacijentId) }}</td>
            <td>{{ examination.doktor || "-" }}</td>
            <td>{{ getExaminationType(examination.vrstaPregleda) }}</td>
            <td class="actions">
              <button @click="editExamination(examination)" class="btn-edit">
                Uredi
              </button>
              <button
                @click="deleteExamination(examination.pregledId)"
                class="btn-delete"
              >
                Obriši
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <div v-else class="no-data">
        <p>Nema unesenih pregleda.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { examinationService } from "../services/examinationService";
import { patientService } from "../services/patientService";
import { doctorService } from "../services/doctorService";

const examinations = ref([]);
const patients = ref([]);
const doctors = ref([]);
const examinationTypes = ref([]);
const loading = ref(false);
const showAddForm = ref(false);
const editingExamination = ref(null);
const uploadedFiles = ref([]);
const form = ref({
  pacijentId: "",
  doktorId: "",
  vrstaPregleda: "",
  datumPregleda: "",
});

const isImagingExamination = computed(() => {
  const imagingTypes = ["X-RAY", "CT", "MR", "ULTRA", "MAMMO"];
  return imagingTypes.includes(form.value.vrstaPregleda);
});

const loadData = async () => {
  loading.value = true;
  try {
    const [examinationsData, patientsData, typesData, doctorsData] =
      await Promise.all([
        examinationService.getAllExaminations(),
        patientService.getAllPatients(),
        examinationService.getExaminationTypes(),
        doctorService.getAllDoctors(),
      ]);

    examinations.value = examinationsData ?? [];
    patients.value = patientsData ?? [];
    examinationTypes.value = typesData ?? [];
    doctors.value = doctorsData ?? [];
    console.log(
      examinations.value,
      patients.value,
      examinationTypes.value,
      doctors.value
    );
  } catch (error) {
    console.error("Greška pri učitavanju podataka:", error);
    alert("Greška pri učitavanju podataka");
  } finally {
    loading.value = false;
  }
};

const closeAddForm = () => {
  showAddForm.value = false;
  editingExamination.value = null;
  resetForm();
};

const resetForm = () => {
  form.value = {
    pacijentId: "",
    doktorId: "",
    vrstaPregleda: "",
    datumPregleda: "",
  };
  uploadedFiles.value = [];
};

const handleFileUpload = (event) => {
  const files = Array.from(event.target.files);
  uploadedFiles.value.push(...files);
};

const removeFile = (index) => {
  uploadedFiles.value.splice(index, 1);
};

const saveExamination = async () => {
  try {
    const examinationData = {
      ...form.value,
      datumPregleda: new Date(form.value.datumPregleda).toISOString(),
    };

    if (editingExamination.value) {
      await examinationService.updateExamination(
        editingExamination.value.pregledId,
        examinationData
      );
    } else {
      await examinationService.createExamination(examinationData);
    }

    // File upload placeholder - in real implementation, files would be uploaded to the server
    if (uploadedFiles.value.length > 0) {
      console.log(
        "Datoteke za učitavanje:",
        uploadedFiles.value.map((f) => f.name)
      );
      // Here you would normally upload files to the server
      // const formData = new FormData()
      // uploadedFiles.value.forEach(file => formData.append('files', file))
      // await documentService.uploadFiles(examination.id, formData)
    }

    await loadData();
    closeAddForm();
  } catch (error) {
    console.error("Greška pri spremanju:", error);
    alert("Greška pri spremanju pregleda");
  }
};

const editExamination = (examination) => {
  editingExamination.value = examination;
  form.value = {
    pacijentId: examination.pacijentId,
    doktorId: examination.doktorId,
    vrstaPregleda: examination.vrstaPregleda,
    datumPregleda: new Date(examination.datumPregleda)
      .toISOString()
      .slice(0, 16),
  };
  showAddForm.value = true;
};

const deleteExamination = async (id) => {
  if (confirm("Jeste li sigurni da želite obrisati ovaj pregled?")) {
    try {
      await examinationService.deleteExamination(id);
      await loadData();
    } catch (error) {
      console.error("Greška pri brisanju:", error);
      alert("Greška pri brisanju pregleda");
    }
  }
};

const getPatientName = (patientId) => {
  if (patients.value.length === 0) {
    return "Loading...";
  }
  const patient = patients.value.find((p) => p.pacijentId === patientId);
  return patient ? `${patient.ime} ${patient.prezime}` : "N/A";
};

const getExaminationType = (typeCode) => {
  const type = examinationTypes.value.find((t) => t.kod === typeCode);
  return type ? type.naziv : typeCode;
};

onMounted(async () => {
  await loadData();
});
</script>

<style scoped>
.examinations {
  padding: 20px;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.loading {
  text-align: center;
  padding: 20px;
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
  max-width: 600px;
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

.examination-form {
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

.form-group input,
.form-group select,
.form-group textarea {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.form-actions {
  display: flex;
  gap: 10px;
  justify-content: flex-end;
  margin-top: 20px;
}

.btn-primary {
  background: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-secondary {
  background: #6c757d;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

.btn-edit {
  background: #28a745;
  color: white;
  border: none;
  padding: 5px 10px;
  border-radius: 3px;
  cursor: pointer;
  margin-right: 5px;
}

.btn-delete {
  background: #dc3545;
  color: white;
  border: none;
  padding: 5px 10px;
  border-radius: 3px;
  cursor: pointer;
}

.examinations-list {
  margin-top: 20px;
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

th,
td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #ddd;
}

th {
  background: #f8f9fa;
  font-weight: bold;
}

.actions {
  white-space: nowrap;
}

.no-data {
  text-align: center;
  padding: 40px;
  color: #666;
}

.file-input {
  margin-bottom: 10px;
}

.form-group small {
  color: #666;
  font-size: 0.9em;
  margin-top: 5px;
}

.uploaded-files {
  margin-top: 10px;
  padding: 10px;
  border: 1px solid #ddd;
  border-radius: 4px;
  background: #f9f9f9;
}

.uploaded-files h4 {
  margin: 0 0 10px 0;
  font-size: 1em;
}

.file-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 5px;
  margin: 5px 0;
  background: white;
  border-radius: 3px;
}

.btn-remove-file {
  background: #dc3545;
  color: white;
  border: none;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  cursor: pointer;
  font-size: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}
</style>
