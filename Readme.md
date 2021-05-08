# ICPC Revolver Alternative

ICPC's resolver provides scoreboard rolling function for XCPC contest. However, there're full of *weird* bugs and it is not open-sourced. So we will reimplement its function with C# and WPF.

## Finished

To be added

## Data schema

### Status change

Description: Description of the problems status change, listed by teams

Schema ref: [./config/Schemas/StatusChange.json](./config/Schemas/StatusChange.json)

Example:

```json
[
    {
        "TeamId": 1,
        "TeamName": "SampleName",
        "StatusFrom": [
            {
                "Label": "A",
                "Try": 5,
                "Time": 120,
                "Status": "UnAccept"
            }
        ],
        "StatusTo": [
            {
                "Label": "A",
                "Try": 6,
                "Time": 150,
                "Status": "Accept"
            }
        ]
    }
]
```

### Animation Config

see [ResolverConfig.cs](./src/IcpcResolver.Net/Window/ResolverConfig.cs)

## TODO

- [ ] Data schema definition (Ongoing)
- [ ] Dataloader support
  - [ ] Import directly from Domjudge's event feed
  - [ ] Import from event feed file
  - [ ] Import from domjudge database
- [ ] Award utilities
- [ ] Add images support
