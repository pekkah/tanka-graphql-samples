export default {
    "scalars": [
        0,
        2,
        3,
        4,
        9
    ],
    "types": {
        "Boolean": {},
        "Channel": {
            "description": [
                9
            ],
            "id": [
                4
            ],
            "messages": [
                5
            ],
            "name": [
                9
            ],
            "__typename": [
                9
            ]
        },
        "Float": {},
        "ID": {},
        "Int": {},
        "Message": {
            "channelId": [
                4
            ],
            "id": [
                4
            ],
            "text": [
                9
            ],
            "timestampMs": [
                9
            ],
            "__typename": [
                9
            ]
        },
        "Mutation": {
            "addChannel": [
                7,
                {
                    "name": [
                        9,
                        "String!"
                    ],
                    "description": [
                        9,
                        "String!"
                    ]
                }
            ],
            "channel": [
                7,
                {
                    "id": [
                        4,
                        "Int!"
                    ]
                }
            ],
            "__typename": [
                9
            ]
        },
        "MutationChannel": {
            "addMessage": [
                5,
                {
                    "text": [
                        9,
                        "String!"
                    ]
                }
            ],
            "description": [
                9
            ],
            "id": [
                4
            ],
            "name": [
                9
            ],
            "__typename": [
                9
            ]
        },
        "Query": {
            "channel": [
                1,
                {
                    "id": [
                        4,
                        "Int!"
                    ]
                }
            ],
            "channels": [
                1
            ],
            "__typename": [
                9
            ]
        },
        "String": {}
    }
}