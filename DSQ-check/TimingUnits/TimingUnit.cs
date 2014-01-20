using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
namespace DSQ_check.TimingUnits
{
    public abstract class TimingUnit
    {
        protected SerialPort _serialPort;
        private object _sendLock = new object();
        private const int WRITE_SLEEP = 5;

        protected byte _last_recieved_byte;
        protected List<byte> _recievedBytes = new List<byte>();

        public delegate void TimingDataReadDelegatte(TimingPackage package);
        public event TimingDataReadDelegatte TimingDataReadEvent;

        public TimingUnit(string comPort, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = new SerialPort(comPort, baudRate, parity, dataBits, stopBits);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
        }

        public void StartCommunication()
        {
            _serialPort.Open();
        }
        public void StopCommunication()
        {
            try
            {
                _serialPort.Close();
            }
            catch (Exception) { }
        }

        protected void RaiseDataReadEvent(TimingPackage package)
        {
            if (TimingDataReadEvent != null)
            {
                TimingDataReadEvent(package);
            }
        }
        public bool IsStarted
        {
            get
            {
                return _serialPort.IsOpen;
            }
        }

        protected abstract void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e);
        public abstract void SyncClock(DateTime time);
        public void sendMessage(byte[] messageBytes)
        {
            if (!_serialPort.IsOpen)
            {
                return;
            }

            // Lock on object to avoid to threads from sending message at once
            lock (_sendLock)
            {
                for (int i = 0; i < messageBytes.Length; i++)
                {
                    // Write data to serialport, with specified sleep between each byte
                    _serialPort.Write(messageBytes, i, 1);
                    System.Threading.Thread.Sleep(WRITE_SLEEP);
                }
            }
        }
    }

    public class TimingPackage
    {
        private List<DataStore.Classes.ControlPunch> _controlPunches;
        private uint _cardNumber;
        private RunnerIdentifier _identifierType;
        private DateTime? _intime;

        public TimingPackage(uint cardNumber, RunnerIdentifier identifierType, DateTime? intime)
        {
            _cardNumber = cardNumber;
            _identifierType = identifierType;
            _controlPunches = new List<DataStore.Classes.ControlPunch>();
            _intime = intime;
        }

        public void AddControl(byte controlCode, byte controlNumber, TimeSpan usedTime)
        {
            _controlPunches.Add(new DataStore.Classes.ControlPunch(controlCode, controlNumber, usedTime));
        }
        public void AddControl(DataStore.Classes.ControlPunch punch)
        {
            _controlPunches.Add(punch);
        }

        public IOrderedEnumerable<DataStore.Classes.ControlPunch> ControlPunches
        {
            get
            {
                return _controlPunches.OrderBy(r => r.ControlCode);
            }
        }
        public uint CardNumber
        {
            get
            {
                return _cardNumber;
            }
        }
        public RunnerIdentifier IdentifierType
        {
            get
            {
                return _identifierType;
            }
        }
        public DateTime? InTime
        {
            get
            {
                return _intime;
            }
        }
    }
}
