# Sudoku Solver API 🧩

A robust, concurrency-oriented backend API developed in **.NET 10** to manage, validate, and process Sudoku boards asynchronously. The project employs modern architectural patterns, internal messaging via Channels, NoSQL persistence, and traffic protection mechanisms.

---

## 🚀 Key Features

* **Asynchronous Processing:** Uses `System.Threading.Channels` combined with `BackgroundServices` for background board validation and processing without blocking HTTP requests.
* **NoSQL Persistence (MongoDB):** Stores Sudoku run states using a custom BSON serializer for native support of multidimensional arrays (`int[,]`).
* **Native Rate Limiting:** Request abuse protection based on client IP (10 requests per minute), with support for reverse proxies (such as Railway via `X-Forwarded-For`).
* **Cloud-Ready (Docker / Railway):** Optimized container via multi-stage Dockerfile and support for continuous deployment.

---

## 🏗️ Project Architecture

The solution is organized within the `SudokuSolverAPI` folder:
* **`Controllers/`**: HTTP entry points for querying and submitting moves.
* **`Channels/`**: Thread-safe communication channels for the task flow (`ValidationChannel` and `ProcessingChannel`).
* **`BackgroundServices/`**: Background workers that consume the channels asynchronously.
* **`Services/`**: Business logic, validation, and persistence (`BoardPersisterService`, etc.).
* **`Utils/`**: JSON and BSON serialization converters for two-dimensional arrays.

---

## 🛠️ Technologies Used

* **.NET 10** (C#)
* **ASP.NET Core Web API**
* **MongoDB & MongoDB Driver**
* **Docker & Docker Compose**

---

## ⚙️ How to Run Locally

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Docker and Docker Compose](https://www.docker.com/)

### Running with Docker Compose
In the project folder containing the `compose.yaml` file, run:

```bash
docker compose up --build

```

This will start both the API and the MongoDB instance locally. The API will be accessible at `http://localhost:8080`.

```