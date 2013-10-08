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

        protected byte _last_recieved_byte;
        protected List<byte> _recievedBytes;

        public delegate void TimingDataReadDelegatte(TimingPackage package);
        public event TimingDataReadDelegatte TimingDataReadEvent;

        public TimingUnit(string comPort, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            _serialPort = new SerialPort(comPort, baudRate, parity, dataBits, stopBits);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(_serialPort_DataReceived);
        }

        protected void RaiseDataReadEvent(TimingPackage package)
        {
            if (TimingDataReadEvent != null)
            {
                TimingDataReadEvent(package);
            }
        }

        protected abstract void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e);
    }

    public class TimingPackage
    {
        private List<byte> _controls;
        private uint _cardNumber;
        private RunnerIdentifier _identifierType;

        public TimingPackage(uint cardNumber, RunnerIdentifier identifierType)
        {
            _cardNumber = cardNumber;
            _identifierType = identifierType;
            _controls = new List<byte>();
        }

        public void AddControl(byte controlCode)
        {
            _controls.Add(controlCode);
        }

        public IEnumerable<byte> Controls
        {
            get
            {
                return _controls;
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
    }
}
