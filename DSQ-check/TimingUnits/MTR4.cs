using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DSQ_check.TimingUnits
{
    public class MTR4 : TimingUnit
    {
        private int sum_incoming = 0, count_255 = 0;

        public MTR4(string comPort)
            : base(comPort, 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One)
        {
        }

        protected override void _serialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Read all bytes
            while (_serialPort.BytesToRead > 0)
            {
                _last_recieved_byte = Convert.ToByte(_serialPort.ReadByte());
                _recievedBytes.Add(_last_recieved_byte);

                // Add byte to sum
                sum_incoming += _last_recieved_byte;

                if (_last_recieved_byte == 255)
                {
                    count_255++;
                }
                else
                {
                    count_255 = 0;
                }

                if (count_255 >= 4)
                {
                    // Clear byte list and add four byte values of 255
                    _recievedBytes.Clear();
                    _recievedBytes.AddRange(Enumerable.Repeat((byte)255, 4));

                    // Add up the bytes
                    sum_incoming = (255 * 4);
                }
                else if (_recievedBytes.Count == 59 || _recievedBytes.Count == 234)
                {
                    byte checkByte = _recievedBytes[_recievedBytes.Count - 2];

                    if ((sum_incoming - checkByte) % 256 == checkByte)
                    {
                        // Process data (fetch time, ecardNo and so on...)
                        AnalyzeData(_recievedBytes);
                    }
                }
            }
        }

        private void AnalyzeData(List<byte> byteList)
        {
            // First, find out what type of message it is
            switch (byteList[5])
            {
                case 83:
                    // Statusmessage (83 is ASCII for 'S')
                    break;
                case 77:
                    // Ecard reading (77 is ASCII for 'M' (MTR Datamessage))
                    ProcessEcardReading(byteList);
                    break;
            }
        }
        public override void SyncClock(DateTime time)
        {
            time = time.AddSeconds(1);

            // Set up message
            byte[] message = new byte[9];

            message[0] = 47;
            message[1] = 83;
            message[2] = 67;
            message[3] = Convert.ToByte(time.ToString("yy"));
            message[4] = Convert.ToByte(time.ToString("MM"));
            message[5] = Convert.ToByte(time.ToString("dd"));
            message[6] = Convert.ToByte(time.ToString("HH"));
            message[7] = Convert.ToByte(time.ToString("mm"));
            message[8] = Convert.ToByte(time.ToString("ss"));

            // Sleep for higher accuracy
            TimeSpan sleepTime = TimeSpan.FromMilliseconds(1000 - time.Millisecond);
            System.Threading.Thread.Sleep(sleepTime);

            sendMessage(message);
        }
        private void ProcessEcardReading(List<byte> byteList)
        {
            // Get ecard number and update last ecard numer variable
            uint ecardNo = GetEcardNo(byteList, 20);

            // Get all readings from ecard
            List<DataStore.Classes.ControlPunch> ecardData = GetEcardData(byteList, 29);

            TimingPackage package = new TimingPackage(ecardNo, RunnerIdentifier.Ecard);
            foreach (DataStore.Classes.ControlPunch punch in ecardData)
            {
                package.AddControl(punch);
            }

            RaiseDataReadEvent(package);
        }

        private uint GetEcardNo(List<byte> dataList, int offset)
        {
            uint ecardNo = 0;

            ecardNo += (uint)Math.Pow(256, 0) * dataList[offset];
            ecardNo += (uint)Math.Pow(256, 1) * dataList[offset + 1];
            ecardNo += (uint)Math.Pow(256, 2) * dataList[offset + 2];

            return ecardNo;
        }
        private List<DataStore.Classes.ControlPunch> GetEcardData(List<byte> dataList, int offset)
        {
            // Initialize a table to hold all data
            List<DataStore.Classes.ControlPunch> ecardData = new List<DataStore.Classes.ControlPunch>();

            // Loop through the bytelist
            byte controlCounter = 1;

            for (int i = offset; i + 2 < dataList.Count; i += 3)
            {
                if (dataList[i] == 0)
                {
                    break;
                }
                else
                {
                    ushort seconds = BitConverter.ToUInt16(dataList.ToArray(), i + 1);
                    TimeSpan usedTime = TimeSpan.FromSeconds(seconds);

                    ecardData.Add(new DataStore.Classes.ControlPunch(dataList[i], controlCounter++, usedTime));
                }
            }

            return ecardData;
        }

    }
}
