using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.TimingUnits
{
    public class ECU1 : TimingUnit
    {
        public ECU1(string comPort)
            : base(comPort, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One)
        {
        }

        protected override void _serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Read all bytes
            while (_serialPort.IsOpen && _serialPort.BytesToRead > 0)
            {
                // Add the next byte in buffer to the list
                _last_recieved_byte = Convert.ToByte(_serialPort.ReadByte());
                _recievedBytes.Add(_last_recieved_byte);

                switch (_last_recieved_byte)
                {
                    case 2:
                        // Start of text (ASCII STX)
                        // Clear the list, since we are recieving a new message
                        _recievedBytes.Clear();
                        break;
                    case 3:
                        // End of text (ASCII ETX)
                        // Convert all the bytes to a string using ASCII encoding
                        string recieved_string = System.Text.Encoding.ASCII.GetString(_recievedBytes.ToArray());

                        // Process the converted data
                        ProcessData(recieved_string);
                        break;
                }
            }
        }

        private void ProcessData(string dataLine)
        {
            if (dataLine.Length == 0)
            {
                return;
            }

            // Split data on tab character
            string[] tabSplit = dataLine.Split('\t');

            // Since data can come in arbitary order, we use a function to safely check
            // message type first.
            MessageType msgType = FindMessageType(tabSplit);
            switch (msgType)
            {
                case MessageType.EmiTagMessage:
                    ProcessEmiTagMessage(tabSplit);
                    break;
                default:
                    break;
            }
        }
        private MessageType FindMessageType(string[] data)
        {
            /* Loop trough data to find identifier.
             * These are valid identifiers (I think):
             * F == Start or finish gate
             * N == EmiTag passing
             * I == Hardware status message
             * K == Keypad message
             */

            foreach (string field in data)
            {
                // Loop trough fields, until a valid identifier is found.
                if (field.Length < 1)
                {
                    // Check field length
                    continue;
                }

                switch (field[0])
                {
                    case 'F':
                        if (field.Length > 2)
                        {
                            // Check field length
                            if (field[1] == '0')
                            {
                                return MessageType.StartGate;
                            }
                            else if (field[1] == '1')
                            {
                                return MessageType.FinishGate;
                            }
                        }
                        break;
                    case 'N':
                        return MessageType.EmiTagMessage;
                    case 'I':
                        return MessageType.StatusMessage;
                    case 'K':
                        return MessageType.KeyPad;
                }
            }

            // If none of the identifiers is found, return Unknown message type.
            return MessageType.Unknown;
        }
        private void ProcessEmiTagMessage(string[] data)
        {
            List<DataStore.Classes.ControlPunch> punches = new List<DataStore.Classes.ControlPunch>();

            uint? tagNumber = null;
            uint? originalTagNumber = null;
         
            byte controlCode, controlNumber;
            byte prevCode = 0;
            uint uintValue;
            TimeSpan usedTime;
            DateTime? timeOfIncident = null;

            bool lastControlRead = false;

            // Loop trough each field, to find information
            foreach (string field in data)
            {
                if (field.Length < 1)
                {
                    // Check field length
                    continue;
                }
                
                switch (field[0])
                {
                    case 'V':
                        // Battery info
                        break;
                    case 'N':
                        // Tag number
                        if (uint.TryParse(field.Remove(0, 1), out uintValue))
                        {
                            tagNumber = uintValue;
                        }
                        break;
                    case 'Y':
                        // Unit serial number
                        break;
                    case 'W':
                        // Time of incident
                        TimeSpan tempValue;
                        if (!TimeSpan.TryParse(field.Remove(0, 1), out tempValue))
                        {
                            continue;
                        }
                        else
                        {
                            timeOfIncident = DateTime.Now.Date.Add(tempValue);
                        }
                        break;
                    case 'S':
                        // Custom emitag number
                        if (uint.TryParse(field.Remove(0, 1), out uintValue))
                        {
                            originalTagNumber = uintValue;
                        }
                        break;
                    case 'R':
                        // Custom emitag string
                        break;
                    case 'L':
                        // Race number...?
                        break;
                    case 'Q':
                        // Control reading
                        string[] fieldSplit = field.Split('-');

                        // Index overview
                        // 0: Control number
                        // 1: Control code
                        // 2: Milliseconds...? Or ticks?
                        // 3: Time since card passed code 0
                        // 4: Time of passing (clock)
                        // 5: Unkown...
                        if (lastControlRead)
                        {
                            break;
                        }
                        if (!byte.TryParse(fieldSplit[0].Remove(0, 1), out controlNumber))
                        {
                            break;
                        }
                        else if (controlNumber == 0)
                        {
                            break;
                        }
                        else if (!byte.TryParse(fieldSplit[1], out controlCode))
                        {
                            break;
                        }
                        else if (controlCode == prevCode)
                        {
                            break;
                        }
                        else if (!TimeSpan.TryParse(fieldSplit[3], out usedTime))
                        {
                            break;
                        }

                        punches.Add(new DataStore.Classes.ControlPunch(controlCode, controlNumber, usedTime));
                        
                        prevCode = controlCode;

                        if (controlCode == 250)
                        {
                            lastControlRead = true;
                        }
                        break;
                    default:
                        break;
                }
            }

            // Check if all values has been set correctly
            if (tagNumber.HasValue && originalTagNumber.HasValue && timeOfIncident.HasValue)
            {
               
                TimingPackage package = new TimingPackage(tagNumber.Value, RunnerIdentifier.EmiTag, null);
                //foreach (byte control in controls.OrderBy(r => r.Key).Select(r => r.Value))
                //{
                //    package.AddControl(
                //}

                RaiseDataReadEvent(package);
            }
        }
        public override void SyncClock(DateTime time)
        {
            // Set up string (add one second!)
            string messageString = "/SC" + time.AddSeconds(1).ToString("HH:mm:ss") + '\r' + '\n';

            // Sleep for higher accuracy
            TimeSpan sleepTime = TimeSpan.FromMilliseconds(1000 - time.Millisecond);
            System.Threading.Thread.Sleep(sleepTime);

            // Send message to device
            byte[] message = System.Text.Encoding.ASCII.GetBytes(messageString);
            sendMessage(message);
        }
    }

    public enum MessageType
    {
        StatusMessage, EmiTagMessage, StartGate, FinishGate, Unknown, KeyPad
    }
}
