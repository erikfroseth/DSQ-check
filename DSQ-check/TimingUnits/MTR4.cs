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

        private void ProcessEcardReading(List<byte> byteList)
        {
            // Get ecard number and update last ecard numer variable
            uint ecardNo = GetEcardNo(byteList, 20);

            // Get all readings from ecard
            List<byte> ecardData = GetEcardData(byteList, 26);

            TimingPackage package = new TimingPackage(ecardNo, RunnerIdentifier.Ecard);
            foreach (byte control in ecardData)
            {
                package.AddControl(control);
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
        private List<byte> GetEcardData(List<byte> dataList, int offset)
        {
            // Initialize a table to hold all data
            List<byte> ecardData = new List<byte>();

            // Loop through the bytelist
            for (int i = offset; i + 2 < dataList.Count; i += 3)
            {
                if (dataList[i] == 0)
                {
                    continue;
                }


                ecardData.Add(dataList[i]);
            }

            return ecardData;
        }

    }
}
