# ToxiScan – Chemical Safety Analysis Tool

**ToxiScan** is an intelligent chemical safety checker that uses real-time data from PubChem to help users identify hazardous chemicals and assess their safety information quickly. Designed for researchers, students, and lab professionals, this tool simplifies how chemical data is explored.

---

## 🔬 How It Works

ToxiScan fetches hazard data such as **GHS classification**, **signal words (e.g., Danger, Warning)**, and **synonyms** from the PubChem PUG View API using the compound CID or chemical name. It processes this data in a user-friendly interface powered by .NET Core (backend) and React with TailwindCSS (frontend).

The backend is built entirely in C# using HttpClient to communicate with the API, ensuring it is fast, reliable, and suitable for enterprise-level integration. The frontend shows hazard icons, signal words, and hazard ratings at a glance.

---

## 🚀 Features

- Real-time hazard analysis from PubChem
- Displays GHS classifications and signal words
- Easy-to-use chemical search with synonym matching
- Extensible for toxicity scoring and graphical hazard indicators

---
-- I am not attached video beacause this will break privacy of our application..
---

## ⚙️ Build & Run Instructions

### 🔧 Backend (.NET Core)
```bash
cd src/backend
dotnet restore
dotnet build
dotnet run
