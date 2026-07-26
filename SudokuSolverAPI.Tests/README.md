# Sudoku Solver API - Test Suite 🧪

This project contains the complete suite of automated tests for the **Sudoku Solver API**, covering everything from isolated unit tests to integration tests with a real database via containers and end-to-end (E2E) tests of the controllers.

---

## 📂 Test Structure

The tests are divided into three main layers:

* **`Unit/`**: Tests focused on isolated business rules, validators, and unit components, without external dependencies.

* **`Integration/`**: Integration tests of the persistence layer (`BoardPersisterService`) using real MongoDB instances running in ephemeral containers through **Testcontainers**.

* **`E2E/` (End-to-End):** End-to-end tests that run an in-memory test server (`TestServer`), sending real HTTP requests to the controllers, testing pipeline flow, capacity limits (*TooManyRequests / 429*) and API responses.

---

## 🧰 Testing Technologies and Tools

* **xUnit**: Main testing framework in C#.

* **Testcontainers.MongoDb**: Automated provisioning of Docker containers with isolated MongoDB for each integration/E2E suite.

* **Microsoft.AspNetCore.TestHost**: Simulation of the ASP.NET Core HTTP pipeline for controller testing without the need for open physical ports.

---

## 🚀 How to Run the Tests

To run all the solution tests (Unit, Integration, and E2E):

```bash
dotnet test

```

> **Note:** Integration and E2E tests use **Testcontainers**, so make sure **Docker** is running on your machine so that the MongoDB temporary containers can be initialized correctly during execution.

```