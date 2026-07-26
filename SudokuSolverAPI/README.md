# Distributed Sudoku Solver - Backend API

This repository contains the central server for the **Distributed Sudoku Solver**, a distributed volunteer computing system designed to solve complex Sudoku puzzles collaboratively.

The API was developed using **.NET 10** and employs an **eventual consistency** model, prioritizing low-latency responses and asynchronous background processing.

---

## 🚀 Project Structure

The project ecosystem is divided into four development stages:

1. **Web Client (Frontend):** A lightweight interface for visualizing current Sudoku states (Dashboard) and allowing manual or assisted solving by users.
2. **API Server (This repository):** The system orchestrator, responsible for distributing sub-states and consolidating received solutions.
3. **Docker Clients (Heavy Clients):** Heavy-duty processing nodes that consume states from the API to compute solutions using greater hardware power.

---

## 🏗️ System Architecture

The API is internally divided into four well-defined layers to ensure separation of concerns, performance, and resilience:

```
[ Client ] ---> ( Presentation Layer: POST / GET )
                        │               ▲
                        ▼               │ (Returns current tree)
                 ( Filter Layer ) ──┘
                        │ (If valid)
                        ▼
           ( Processing Layer )
                        │ (Updates Topological Tree)
                        ▼
           ( Persistence Layer ) ---> [ Memory / Database ]

```

### 1. Presentation Layer

Exposes the system's HTTP entry points (endpoints). It ensures immediate responses to the user (for both submission and querying) by delegating heavy lifting to internal queues.

### 2. Filter Layer (Validation)

Before a new solution proceeds, it undergoes three rigorous validation criteria:

* **Duplication Filter:** Uses hashing algorithms structured within an in-memory `HashMap`/`HashSet` to check if that exact board state has already been processed, preventing wasted computational power.
* **Rule Validity Filter:** Verifies that the submitted Sudoku adheres to traditional game rules (no repeated numbers in rows, columns, or $3 \times 3$ quadrants).
* **Lineage (Origin) Filter:** Mathematically ensures that the submitted board is a direct evolution of the original base board the system is attempting to solve, rejecting external or corrupted boards.

### 3. Processing Layer

Following validation, this layer integrates the new state into the Sudoku's **topological resolution tree**, mapping the path taken from the original seed to the current resolution state.

### 4. Persistence Layer

To ensure the extreme performance required for distributed processing, the application operates **100% in-memory**. However, this layer acts as a disaster recovery mechanism (asynchronous I/O), persisting the tree state to a database so that the system can be instantly rehydrated in the event of a crash.

---

## 🌐 API Endpoints

The API exposes two main routes, designed based on the principle of eventual consistency:

| Method | Route | Description | Technical Behavior |
| --- | --- | --- | --- |
| **POST** | `/api/sudoku/state` | Submits a more advanced or solved Sudoku state. | Instant return (asynchronous). Processing occurs in the background. |
| **GET** | `/api/sudoku/tree` | Returns the complete topological tree of all states. | Instant return containing the latest state consolidated in memory. |

---

## 🔐 Identification and Security (Proof of Contribution)

To track and reward the volunteer nodes assisting with the solution, every `POST` request must include an electronically **signed message**.

* **How ​​it works:** The client generates an asymmetric key pair. When submitting a solution, it digitally signs its identifier using its **Private Key**.
* **Verification:** The client sends the signed message and its **Public Key** in the payload. The API uses the public key to verify the signature's authenticity, ensuring authorship integrity without exposing the volunteer's sensitive data.

---

## 🛠️ Technologies Used

* **Runtime:** .NET 10
* **Language:** C#
* **Primary Data Structures:** `ConcurrentDictionary` (for concurrent validation hashes) and N-ary tree structures for the Sudoku topology.

---

## 🛠️ How to Run the API (Development)

### Prerequisites

* .NET 10 SDK installed.

### Steps

1. Clone the repository:
```bash
git clone https://github.com/your-username/sudoku-solver-backend.git
```

2. Navigate to the project directory:
```bash
cd sudoku-solver-backend
```

3. Run the application:
```bash
dotnet run
```

---

## 🛠️ How to Run the API (Docker)

### Prerequisites

* Docker & Docker Compose installed;

### Steps (Building for source)

1. Clone the repository:
```bash
git clone https://github.com/your-username/sudoku-solver-backend.git
```

2. Navigate to the project directory:
```bash
cd sudoku-solver-backend
```

3. Run the application:
```bash
docker compose up --build
```

### Steps (Building from ghcr.io)

1. Login in ghcr.io:
```bash
docker login ghcr.io
```

2. Pull the image:
```bash
docker pull ghcr.io/sd-ufcg-2026/sudoku-solver-api:latest
```

3. Run the application:
```bash
docker run ghcr.io/sd-ufcg-2026/sudoku-solver-api:latest -p 8080:8080 
```