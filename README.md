# Card Reader
This is a personal project that utilizes:
- Arduino UNO
- RFID RC522

The Arduino Project file requires the MFRC522 community library to work.

## How to use
1. Wire the RC522 to the Arduino UNO based on the below table.

| RC522 | Arduino UNO |
| - | - |
| SDA | Pin 10 |
| SCK | Pin 13 |
| MOSI | Pin 11 |
| MISO | Pin 12 |
| GND | GND |
| RST | Pin 9 |
| 3.3V | 3.3V |

2. Plug in the Arduino UNO into the computer.
3. Compile and Upload the Arduino Code to your Arduino.
4. Compile and Run the C# project.
5. Scan your RFID (13.56 MHz) card on reader.
6. Observe the card UID printed in the C# project.