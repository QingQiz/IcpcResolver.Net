# ICPC Revolver Alternative

ICPC's resolver provides scoreboard rolling function for XCPC contest. However, there're full of *weird* bugs and it is not open-sourced. So we will reimplement its function with C# and WPF.

## Finished

To be added

## TODO

* Data schema definition (Ongoing)
* Dataloader support
  * Import directly from Domjudge's event feed
  * Import from event feed file
  * Import from domjudge database
* Award utilities
* Add images support

## Data schema

* Status change

Description: Description of the problems status change, listed by teams

Schema ref: [./IcpcResolver.Net/Schemas/StatusChange.json](./IcpcResolver.Net/Schemas/StatusChange.json)

Example:

```json
[
    {
        "TeamId": 1,
        "Teamname": "SampleName",
        "StatusFrom": [
            {
                "Lable": "A",
                "Try": 5,
                "Time": 120,
                "Status": "UnAccept"
            }
        ],
        "StatusFrom": [
            {
                "Lable": "A",
                "Try": 6,
                "Time": 150,
                "Status": "Accept"
            }
        ]
    }
]
```
