db = db.getSiblingDB('ABC');

db.runs.insertMany([
    {
        "_id": 1,
        "Root": {
            "Value": {
                "SudokuBoard": [
                    [1, 0, 0, 0],
                    [0, 0, 0, 0],
                    [0, 0, 0, 0],
                    [0, 0, 0, 0]
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
        "Boards": {}
    },
    {
        "_id": 2,
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
        "Boards": {}
    }
]);