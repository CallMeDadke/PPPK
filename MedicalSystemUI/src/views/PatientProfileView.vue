<template>
  <div class="patient-profile">
    <div v-if="loading">Učitavanje...</div>
    <div v-else-if="!patient">Pacijent nije pronađen</div>
    <div v-else>
      <div class="header">
        <button @click="$router.go(-1)" class="btn btn-secondary">
          Natrag
        </button>
        <h1>{{ patient.ime }} {{ patient.prezime }}</h1>
      </div>

      <!-- Patient Info -->
      <div class="patient-info">
        <h3>Osnovne informacije</h3>
        <div class="info-grid">
          <div class="info-item"><strong>Ime:</strong> {{ patient.ime }}</div>
          <div class="info-item">
            <strong>Prezime:</strong> {{ patient.prezime }}
          </div>
          <div class="info-item"><strong>OIB:</strong> {{ patient.oib }}</div>
          <div class="info-item">
            <strong>Datum rođenja:</strong> {{ patient.datumRodenja }}
          </div>
          <div class="info-item"><strong>Godine:</strong> {{ patientAge }}</div>
          <div class="info-item">
            <strong>Spol:</strong>
            {{ patient.spol }}
          </div>
        </div>
      </div>


      <!-- Examinations -->
      <div class="examinations">
        <div class="section-header">
          <h3>Povјesnica pregleda</h3>
          <button
            @click="showAddExaminationForm = true"
            class="btn btn-primary"
          >
            Dodaj pregled
          </button>
        </div>

        <!-- Add/Edit Examination Form -->
        <div
          v-if="showAddExaminationForm"
          class="modal-overlay"
          @click="closeExaminationForm"
        >
          <div class="modal" @click.stop>
            <div class="modal-header">
              <h3>
                {{ editingExamination ? "Uredi pregled" : "Dodaj pregled" }}
              </h3>
              <button @click="closeExaminationForm" class="btn-close">×</button>
            </div>

            <form @submit.prevent="saveExamination" class="examination-form">
              <div class="form-group">
                <label>Doktor:</label>
                <select v-model="examinationForm.doktorId" required>
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
                <select v-model="examinationForm.vrstaPregleda" required>
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
                  v-model="examinationForm.datumPregleda"
                  required
                />
              </div>

              <div class="form-actions">
                <button
                  type="button"
                  @click="closeExaminationForm"
                  class="btn btn-secondary"
                >
                  Odustani
                </button>
                <button type="submit" class="btn btn-primary">
                  {{ editingExamination ? "Spremi" : "Dodaj" }}
                </button>
              </div>
            </form>
          </div>
        </div>

        <div v-if="examinationsLoading" class="loading">
          Učitavanje pregleda...
        </div>
        <div v-else-if="examinations.length === 0" class="no-data">
          Nema zabilježenih pregleda
        </div>
        <div v-else>
          <table class="examinations-table">
            <thead>
              <tr>
                <th>Datum</th>
                <th>Vrsta pregleda</th>
                <th>Doktor</th>
                <th>Akcije</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="examination in examinations"
                :key="examination.pregledId"
              >
                <td>{{ examination.datumPregleda }}</td>
                <td>{{ getExaminationTypeName(examination.vrstaPregleda) }}</td>
                <td>{{ getDoctorName(examination.doktorId) }}</td>
                <td>
                  <button
                    @click="editExamination(examination)"
                    class="btn-edit"
                  >
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
        </div>
      </div>

      <!-- Prescriptions -->
      <div class="prescriptions">
        <div class="section-header">
          <h3>Recepti</h3>
          <button
            @click="showAddPrescriptionForm = true"
            class="btn btn-primary"
          >
            Dodaj recept
          </button>
        </div>

        <!-- Add/Edit Prescription Form -->
        <div
          v-if="showAddPrescriptionForm"
          class="modal-overlay"
          @click="closePrescriptionForm"
        >
          <div class="modal" @click.stop>
            <div class="modal-header">
              <h3>
                {{ editingPrescription ? "Uredi recept" : "Dodaj recept" }}
              </h3>
              <button @click="closePrescriptionForm" class="btn-close">
                ×
              </button>
            </div>

            <form @submit.prevent="savePrescription" class="prescription-form">
              <div class="form-group">
                <label>Datum izdavanja:</label>
                <input
                  type="date"
                  v-model="prescriptionForm.datumIzdavanja"
                  required
                />
              </div>

              <div class="form-group">
                <h4>Lijekovi</h4>
                <div
                  v-for="(stavka, index) in prescriptionForm.stavke"
                  :key="index"
                  class="medicine-item"
                >
                  <div class="medicine-row">
                    <select
                      v-model="stavka.lijekId"
                      required
                      class="medicine-select"
                    >
                      <option value="">Odaberite lijek</option>
                      <option
                        v-for="medicine in medicines"
                        :key="medicine.lijekId"
                        :value="medicine.lijekId"
                      >
                        {{ medicine.naziv }}
                      </option>
                    </select>
                    <input
                      type="text"
                      v-model="stavka.doziranje"
                      placeholder="Doziranje (npr. 2x dnevno)"
                      required
                      class="dosage-input"
                    />
                    <button
                      type="button"
                      @click="removePrescriptionItem(index)"
                      class="btn btn-small btn-danger"
                    >
                      Ukloni
                    </button>
                  </div>
                </div>
                <button
                  type="button"
                  @click="addPrescriptionItem"
                  class="btn btn-small"
                >
                  Dodaj lijek
                </button>
              </div>

              <div class="form-actions">
                <button
                  type="button"
                  @click="closePrescriptionForm"
                  class="btn btn-secondary"
                >
                  Odustani
                </button>
                <button type="submit" class="btn btn-primary">
                  {{ editingPrescription ? "Spremi" : "Dodaj" }}
                </button>
              </div>
            </form>
          </div>
        </div>

        <div v-if="prescriptionsLoading" class="loading">
          Učitavanje recepata...
        </div>
        <div v-else-if="prescriptions.length === 0" class="no-data">
          Nema zabilježenih recepata
        </div>
        <div v-else>
          <div
            v-for="prescription in prescriptions"
            :key="prescription.receptId"
            class="prescription-item"
          >
            <div class="prescription-header">
              <div>
                <strong>Datum:</strong> {{ prescription.datumIzdavanja }}
              </div>
              <div class="prescription-actions">
                <button
                  @click="editPrescription(prescription)"
                  class="btn-edit"
                >
                  Uredi
                </button>
                <button
                  @click="deletePrescription(prescription.receptId)"
                  class="btn-delete"
                >
                  Obriši
                </button>
              </div>
            </div>
            <div class="medications">
              <div
                v-for="item in prescription.stavke"
                :key="item.stavkaReceptaId"
                class="medication"
              >
                {{ getMedicineName(item.lijekId) }} - {{ item.doziranje }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useRoute } from "vue-router";
import { usePatientsStore } from "@/stores/patients";
import { patientService } from "@/services/patientService";
import { examinationService } from "@/services/examinationService";
import { prescriptionService } from "@/services/prescriptionService";
import { doctorService } from "@/services/doctorService";
import { medicineService } from "@/services/medicineService";

const route = useRoute();
const patientsStore = usePatientsStore();
const loading = ref(false);
const examinationsLoading = ref(false);
const prescriptionsLoading = ref(false);
const examinations = ref([]);
const prescriptions = ref([]);
const doctors = ref([]);
const medicines = ref([]);
const examinationTypes = ref([]);


// Examinations
const showAddExaminationForm = ref(false);
const editingExamination = ref(null);
const examinationForm = ref({
  doktorId: "",
  vrstaPregleda: "",
  datumPregleda: "",
});

// Prescriptions
const showAddPrescriptionForm = ref(false);
const editingPrescription = ref(null);
const prescriptionForm = ref({
  datumIzdavanja: new Date().toISOString().split("T")[0],
  stavke: [{ lijekId: "", doziranje: "" }],
});

const patientId = route.params.id;
const patient = computed(() => patientsStore.currentPatient);

// Age calculation
const patientAge = computed(() => {
  if (!patient.value?.datumRodenja) return "N/A";
  const today = new Date();
  const birth = new Date(patient.value.datumRodenja);
  let age = today.getFullYear() - birth.getFullYear();
  const monthDiff = today.getMonth() - birth.getMonth();
  if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birth.getDate())) {
    age--;
  }
  return age;
});

const loadPatientData = async () => {
  try {
    loading.value = true;
    const patientData = await patientService.getPatientById(patientId);
    patientsStore.setCurrentPatient(patientData.data);

    // Load supporting data
    await Promise.all([loadDoctors(), loadMedicines(), loadExaminationTypes()]);

    // Load patient-specific data
    await Promise.all([loadExaminations(), loadPrescriptions()]);
  } catch (error) {
    console.error("Error loading patient:", error);
    alert("Greška pri učitavanju podataka pacijenta");
  } finally {
    loading.value = false;
  }
};

const loadDoctors = async () => {
  try {
    const doctorsData = await doctorService.getAllDoctors();
    doctors.value = doctorsData || [];
  } catch (error) {
    console.error("Error loading doctors:", error);
  }
};

const loadMedicines = async () => {
  try {
    const medicinesData = await medicineService.getAllMedicines();
    medicines.value = medicinesData || [];
  } catch (error) {
    console.error("Error loading medicines:", error);
  }
};

const loadExaminationTypes = async () => {
  try {
    const typesData = await examinationService.getExaminationTypes();
    examinationTypes.value = typesData || [];
  } catch (error) {
    console.error("Error loading examination types:", error);
  }
};

const loadExaminations = async () => {
  try {
    examinationsLoading.value = true;
    const examinationsData =
      await examinationService.getExaminationsByPatientId(patientId);
    examinations.value = examinationsData || [];
  } catch (error) {
    console.error("Error loading examinations:", error);
  } finally {
    examinationsLoading.value = false;
  }
};

const loadPrescriptions = async () => {
  try {
    prescriptionsLoading.value = true;
    const prescriptionsData =
      await prescriptionService.getPrescriptionsByPatient(patientId);
    prescriptions.value = prescriptionsData || [];
  } catch (error) {
    console.error("Error loading prescriptions:", error);
  } finally {
    prescriptionsLoading.value = false;
  }
};


// Examination Functions
const closeExaminationForm = () => {
  showAddExaminationForm.value = false;
  editingExamination.value = null;
  resetExaminationForm();
};

const resetExaminationForm = () => {
  examinationForm.value = {
    doktorId: "",
    vrstaPregleda: "",
    datumPregleda: "",
  };
};

const saveExamination = async () => {
  try {
    const examinationData = {
      ...examinationForm.value,
      pacijentId: parseInt(patientId),
      doktorId: parseInt(examinationForm.value.doktorId),
      datumPregleda: new Date(
        examinationForm.value.datumPregleda
      ).toISOString(),
    };

    if (editingExamination.value) {
      await examinationService.updateExamination(
        editingExamination.value.pregledId,
        examinationData
      );
    } else {
      await examinationService.createExamination(examinationData);
    }

    await loadExaminations();
    closeExaminationForm();
  } catch (error) {
    console.error("Error saving examination:", error);
    alert("Greška pri spremanju pregleda");
  }
};

const editExamination = (examination) => {
  editingExamination.value = examination;
  examinationForm.value = {
    doktorId: examination.doktorId?.toString() || "",
    vrstaPregleda: examination.vrstaPregleda || "",
    datumPregleda: new Date(examination.datumPregleda)
      .toISOString()
      .slice(0, 16),
  };
  showAddExaminationForm.value = true;
};

const deleteExamination = async (id) => {
  if (confirm("Jeste li sigurni da želite obrisati ovaj pregled?")) {
    try {
      await examinationService.deleteExamination(id);
      await loadExaminations();
    } catch (error) {
      console.error("Error deleting examination:", error);
      alert("Greška pri brisanju pregleda");
    }
  }
};

const getExaminationTypeName = (code) => {
  const type = examinationTypes.value.find((t) => t.kod === code);
  return type ? type.naziv : code;
};

const getDoctorName = (doctorId) => {
  const doctor = doctors.value.find((d) => d.doktorId === doctorId);
  return doctor ? `${doctor.ime} ${doctor.prezime}` : "N/A";
};

// Prescription Functions
const closePrescriptionForm = () => {
  showAddPrescriptionForm.value = false;
  editingPrescription.value = null;
  resetPrescriptionForm();
};

const resetPrescriptionForm = () => {
  prescriptionForm.value = {
    datumIzdavanja: new Date().toISOString().split("T")[0],
    stavke: [{ lijekId: "", doziranje: "" }],
  };
};

const addPrescriptionItem = () => {
  prescriptionForm.value.stavke.push({ lijekId: "", doziranje: "" });
};

const removePrescriptionItem = (index) => {
  if (prescriptionForm.value.stavke.length > 1) {
    prescriptionForm.value.stavke.splice(index, 1);
  }
};

const savePrescription = async () => {
  try {
    const prescriptionData = {
      pacijentId: parseInt(patientId),
      datumIzdavanja: prescriptionForm.value.datumIzdavanja,
      stavke: prescriptionForm.value.stavke
        .filter((s) => s.lijekId && s.doziranje.trim())
        .map((s) => ({
          lijekId: parseInt(s.lijekId),
          doziranje: s.doziranje.trim(),
        })),
    };

    if (prescriptionData.stavke.length === 0) {
      alert("Dodajte barem jedan lijek na recept");
      return;
    }

    if (editingPrescription.value) {
      await prescriptionService.updatePrescription(
        editingPrescription.value.receptId,
        prescriptionData
      );
    } else {
      await prescriptionService.createPrescription(prescriptionData);
    }

    await loadPrescriptions();
    closePrescriptionForm();
  } catch (error) {
    console.error("Error saving prescription:", error);
    alert("Greška pri spremanju recepta");
  }
};

const editPrescription = (prescription) => {
  editingPrescription.value = prescription;
  prescriptionForm.value = {
    datumIzdavanja: prescription.datumIzdavanja,
    stavke:
      prescription.stavke && prescription.stavke.length > 0
        ? prescription.stavke.map((s) => ({
            lijekId: s.lijekId?.toString() || "",
            doziranje: s.doziranje,
          }))
        : [{ lijekId: "", doziranje: "" }],
  };
  showAddPrescriptionForm.value = true;
};

const deletePrescription = async (id) => {
  if (confirm("Jeste li sigurni da želite obrisati ovaj recept?")) {
    try {
      await prescriptionService.deletePrescription(id);
      await loadPrescriptions();
    } catch (error) {
      console.error("Error deleting prescription:", error);
      alert("Greška pri brisanju recepta");
    }
  }
};

const getMedicineName = (lijekId) => {
  const medicine = medicines.value.find((m) => m.lijekId === lijekId);
  return medicine ? medicine.naziv : "N/A";
};

onMounted(async () => {
  await loadPatientData();
});
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

.loading {
  text-align: center;
  color: #666;
  padding: 20px;
}

.examination-form,
.prescription-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.form-group select {
  padding: 8px;
  border: 1px solid #ddd;
  border-radius: 4px;
  width: 100%;
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

.btn-small {
  padding: 5px 10px;
  font-size: 0.9em;
}

.btn-danger {
  background-color: #dc3545;
  color: white;
}

.prescription-item {
  margin-bottom: 15px;
}

.prescription-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.prescription-actions {
  display: flex;
  gap: 5px;
}
</style>
