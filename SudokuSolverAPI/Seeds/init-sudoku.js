let dbName = 'SudokuSolverDB';
let collectionName = 'Runs';

print(`[Seed] Conectando ao banco de dados: ${dbName}`);
print(`[Seed] Inserindo na collection: ${collectionName}`);

const database = db.getSiblingDB(dbName);
const collection = database.getCollection(collectionName);

collection.insertMany([
    {
        "_id": 1,
        "Root": {
            "Value": {
                "SudokuBoard": [
                    [0, 0, 0, 4],
                    [0, 0, 0, 0],
                    [2, 0, 0, 3],
                    [4, 0, 1, 2]
                ],
                "Signature": {
                    "Identifier": "System",
                    "Key": "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEmApSRE2KjXcWJVNV4jekQtlf+dQdS2cbl6ObvMKpDPG006seSuXEO+9AdYXep1ZO4q5s7U5IhCpPPmSrXNrtEQ=="
                }
            },
            "Nodes": []
        },
        "IsResolved": false,
        "Final": null,
        "Boards": []
    },
    {
        "_id": 2,
        "Root": {
            "Value": {
                "SudokuBoard": [
                    [9, 0, 0, 5, 0, 8, 0, 0, 7],
                    [0, 8, 0, 3, 0, 2, 9, 0, 5],
                    [0, 5, 4, 0, 0, 0, 0, 8, 0],
                    [0, 7, 0, 6, 8, 0, 0, 3, 2],
                    [1, 0, 0, 0, 0, 4, 0, 0, 8],
                    [5, 0, 0, 2, 1, 9, 0, 6, 0],
                    [0, 0, 0, 9, 0, 6, 0, 0, 1],
                    [7, 2, 6, 0, 0, 1, 0, 4, 0],
                    [0, 0, 1, 4, 7, 0, 0, 5, 6]
                ],
                "Signature": {
                    "Identifier": "Sistema",
                    "Key": "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEmApSRE2KjXcWJVNV4jekQtlf+dQdS2cbl6ObvMKpDPG006seSuXEO+9AdYXep1ZO4q5s7U5IhCpPPmSrXNrtEQ=="
                }
            },
            "Nodes": []
        },
        "IsResolved": false,
        "Final": null,
        "Boards": []
    }
]);

print("[Seed] Dados inseridos com sucesso!");