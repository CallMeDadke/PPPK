import { patientService } from './patientService'
import { prescriptionService } from './prescriptionService'
import { examinationService } from './examinationService'
import { medicalHistoryService } from './medicalHistoryService'

export const exportService = {
  // CSV helper functions
  escapeCSVField(field) {
    if (field === null || field === undefined) return ''
    const str = String(field)
    if (str.includes('"') || str.includes(',') || str.includes('\n') || str.includes('\r')) {
      return `"${str.replace(/"/g, '""')}"`
    }
    return str
  },

  arrayToCSV(data, headers) {
    if (!data || data.length === 0) return ''
    
    // Create CSV header
    const csvHeader = headers.map(h => this.escapeCSVField(h.label)).join(',')
    
    // Create CSV rows
    const csvRows = data.map(row => 
      headers.map(header => this.escapeCSVField(row[header.key])).join(',')
    )
    
    return [csvHeader, ...csvRows].join('\n')
  },

  downloadCSV(csvContent, filename) {
    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
    const link = document.createElement('a')
    
    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob)
      link.setAttribute('href', url)
      link.setAttribute('download', filename)
      link.style.visibility = 'hidden'
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
    }
  },

  // Export functions for different entities
  async exportPatients() {
    try {
      const patients = await patientService.getAllPatients()
      const headers = [
        { key: 'id', label: 'ID' },
        { key: 'ime', label: 'Ime' },
        { key: 'prezime', label: 'Prezime' },
        { key: 'oib', label: 'OIB' },
        { key: 'datumRodjenja', label: 'Datum rođenja' },
        { key: 'spol', label: 'Spol' }
      ]
      
      const csvContent = this.arrayToCSV(patients, headers)
      const filename = `pacijenti_${new Date().toISOString().split('T')[0]}.csv`
      this.downloadCSV(csvContent, filename)
      return true
    } catch (error) {
      console.error('Greška pri exportu pacijenata:', error)
      throw error
    }
  },

  async exportPrescriptions() {
    try {
      const [prescriptions, patients] = await Promise.all([
        prescriptionService.getAllPrescriptions(),
        patientService.getAllPatients()
      ])

      // Create a map for patient lookup
      const patientMap = new Map()
      patients.forEach(p => patientMap.set(p.id, `${p.ime} ${p.prezime}`))

      // Flatten prescriptions with their items
      const flatData = []
      prescriptions.forEach(prescription => {
        if (prescription.stavke && prescription.stavke.length > 0) {
          prescription.stavke.forEach(stavka => {
            flatData.push({
              receptId: prescription.receptId,
              pacijent: patientMap.get(prescription.pacijentId) || 'Nepoznat',
              datumIzdavanja: prescription.datumIzdavanja,
              lijek: stavka.lijek,
              doziranje: stavka.doziranje
            })
          })
        } else {
          flatData.push({
            receptId: prescription.receptId,
            pacijent: patientMap.get(prescription.pacijentId) || 'Nepoznat',
            datumIzdavanja: prescription.datumIzdavanja,
            lijek: 'Nema lijekova',
            doziranje: ''
          })
        }
      })

      const headers = [
        { key: 'receptId', label: 'ID recepta' },
        { key: 'pacijent', label: 'Pacijent' },
        { key: 'datumIzdavanja', label: 'Datum izdavanja' },
        { key: 'lijek', label: 'Lijek' },
        { key: 'doziranje', label: 'Doziranje' }
      ]
      
      const csvContent = this.arrayToCSV(flatData, headers)
      const filename = `recepti_${new Date().toISOString().split('T')[0]}.csv`
      this.downloadCSV(csvContent, filename)
      return true
    } catch (error) {
      console.error('Greška pri exportu recepti:', error)
      throw error
    }
  },

  async exportExaminations() {
    try {
      const [examinations, patients, doctors] = await Promise.all([
        examinationService.getAllExaminations(),
        patientService.getAllPatients(),
        examinationService.getAllDoctors()
      ])

      // Create maps for lookups
      const patientMap = new Map()
      patients.forEach(p => patientMap.set(p.id, `${p.ime} ${p.prezime}`))
      
      const doctorMap = new Map()
      doctors.forEach(d => doctorMap.set(d.doktorId, `${d.ime} ${d.prezime}`))

      const exportData = examinations.map(exam => ({
        pregledId: exam.pregledId,
        pacijent: patientMap.get(exam.pacijentId) || 'Nepoznat',
        doktor: doctorMap.get(exam.doktorId) || 'Nepoznat',
        vrstaPregleda: exam.vrstaPregleda || '',
        datumVrijeme: exam.datumVrijeme,
        datoteka: exam.datoteka || ''
      }))

      const headers = [
        { key: 'pregledId', label: 'ID pregleda' },
        { key: 'pacijent', label: 'Pacijent' },
        { key: 'doktor', label: 'Doktor' },
        { key: 'vrstaPregleda', label: 'Vrsta pregleda' },
        { key: 'datumVrijeme', label: 'Datum i vrijeme' },
        { key: 'datoteka', label: 'Datoteka' }
      ]
      
      const csvContent = this.arrayToCSV(exportData, headers)
      const filename = `pregledi_${new Date().toISOString().split('T')[0]}.csv`
      this.downloadCSV(csvContent, filename)
      return true
    } catch (error) {
      console.error('Greška pri exportu pregleda:', error)
      throw error
    }
  },

  async exportMedicalHistory(pacijentId = null) {
    try {
      let historyData = []
      let patients = []
      
      if (pacijentId) {
        // Export for specific patient
        historyData = await medicalHistoryService.getHistoryByPatient(pacijentId)
        const patient = await patientService.getPatientById(pacijentId)
        patients = [patient]
      } else {
        // Export all medical history would require getting all patients first
        patients = await patientService.getAllPatients()
        for (const patient of patients) {
          try {
            const history = await medicalHistoryService.getHistoryByPatient(patient.id)
            historyData.push(...history.map(h => ({ ...h, pacijentId: patient.id })))
          } catch (error) {
            // Skip patients without history
            console.warn(`No history for patient ${patient.id}`)
          }
        }
      }

      // Create patient map
      const patientMap = new Map()
      patients.forEach(p => patientMap.set(p.id, `${p.ime} ${p.prezime}`))

      const exportData = historyData.map(history => ({
        povijestBolestiId: history.povijestBolestiId || history.id,
        pacijent: patientMap.get(history.pacijentId) || 'Nepoznat',
        nazivBolesti: history.nazivBolesti,
        datumPocetka: history.datumPocetka,
        datumZavrsetka: history.datumZavrsetka || '',
        statusBolesti: history.datumZavrsetka ? 'Završeno' : 'U tijeku'
      }))

      const headers = [
        { key: 'povijestBolestiId', label: 'ID' },
        { key: 'pacijent', label: 'Pacijent' },
        { key: 'nazivBolesti', label: 'Naziv bolesti' },
        { key: 'datumPocetka', label: 'Datum početka' },
        { key: 'datumZavrsetka', label: 'Datum završetka' },
        { key: 'statusBolesti', label: 'Status' }
      ]
      
      const csvContent = this.arrayToCSV(exportData, headers)
      const suffix = pacijentId ? `_pacijent_${pacijentId}` : ''
      const filename = `povijest_bolesti${suffix}_${new Date().toISOString().split('T')[0]}.csv`
      this.downloadCSV(csvContent, filename)
      return true
    } catch (error) {
      console.error('Greška pri exportu povijesti bolesti:', error)
      throw error
    }
  }
}