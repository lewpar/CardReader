#include <SPI.h>
#include <MFRC522.h>

#define RST_PIN 9
#define SS_PIN 10

enum OpCodes : uint8_t
{
  OPCODE_RFID_READ = 1,
  OPCODE_RFID_LOCK_STATE = 2
};

bool locked = false;
unsigned long lockTimeMs = 0; // How many milliseconds have passed since the reader has been locked.
unsigned long timeoutMs = 10000; // Number of milliseconds to unlock the reader to prevent indefinite lock.

MFRC522 mfrc522(SS_PIN, RST_PIN);

void setup() {
	Serial.begin(9600);
	SPI.begin();
	mfrc522.PCD_Init();
}

void loop() {
  // If the card reader is locked waiting for response.
  if(isLocked())
  {
    // Check the serial buffer for unlock signal.
    checkForUnlock();
    return;
  }

	// Check if there is a card present to read.
	if (!mfrc522.PICC_IsNewCardPresent()) {
		return;
	}

	// Check to see if the card if readable.
	if (!mfrc522.PICC_ReadCardSerial()) {
		return;
	}

  Serial.write(OPCODE_RFID_READ); // OpCode
  Serial.write(mfrc522.uid.size); // UidLen
  for (byte i = 0; i < mfrc522.uid.size; i++) // Uid
  {
    Serial.write(mfrc522.uid.uidByte[i]);
  }

  // Lock the card reader.
  setLock(true);
}

bool isLocked()
{
  return locked;
}

void setLock(bool state)
{
  locked = state;

  if(state)
  {
    // Set the time when the reader is locked.
    lockTimeMs = millis();
  }

  // Send lock state through Serial.
  Serial.write(OPCODE_RFID_LOCK_STATE); // OpCode
  Serial.write(locked); // Lock State
}

void checkForUnlock()
{
  // Check if the reader has timed out.
  if((millis() - lockTimeMs) > timeoutMs)
  {
    setLock(false);

    // Clear the serial buffer.
    if(Serial.available())
    {
      int b = 0;
      while(b != -1)
      {
        b = Serial.read();
      }
    }

    return;
  }

  if(!Serial.available())
  {
    return;
  }

  // Read 1 byte from serial buffer.
  int data = Serial.read();

  // Treat data as a bool, if the value is true then unlock the reader.
  if(data)
  {
    setLock(false);
  }
}