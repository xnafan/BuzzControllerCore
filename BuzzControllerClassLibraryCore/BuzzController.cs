using System.Collections;

namespace BuzzControllerClassLibraryCore
{
    //Invaluable reference article by Ashley Deakin:
    //http://www.developerfusion.com/article/84338/making-usb-c-friendly/
    public class BuzzController : IDisposable
    {
        #region Enumerations
        public enum BuzzButtonAction { Unchanged, Pressed, Released };
        public enum BuzzControllerNumber { First, Second, Third, Fourth };
        public enum BuzzControllerButton { Red, Blue, Orange, Green, Yellow }; 
        #endregion

        #region variables and properties
        public event EventHandler<BuzzControllerEventArgs>? ButtonStateChanged;
        byte[] _lastReadData = new byte[6] { 0, 0, 0, 0, 0, 0 };
        byte[] _lastWriteData = new byte[6] { 0, 0, 0, 0, 0, 0 };
        HIDInterface.HIDDevice _device;
        #endregion

        #region Constructor
        public BuzzController()
        {
            //Get the details of all connected USB HID devices
            HIDInterface.HIDDevice.interfaceDetails[] devices = HIDInterface.HIDDevice.getConnectedDevices();

            //Select a device from the available devices (uses the Vendor ID and Product ID of the PS2 Buzz! controller).
            var dev = devices.Where(dev => dev.VID == 0x054C && dev.PID == 0x0002).FirstOrDefault();
            if (dev.VID == 0) { throw new Exception("No 'Playstation 2 BUZZ!' controller detected"); }

            //register device, and set it up for publishing events when new data comes in
            _device = new HIDInterface.HIDDevice(dev.devicePath, true);

            //subscribe to data received event
            _device.dataReceived += _device_dataReceived;

            //initialize all lights off, in case they were left on from a previous program running.
            TurnAllLightsOff();
        } 
        #endregion

        #region Functions to control lights in controllers
        public void TurnAllLightsOff()
        {
            Enum.GetValues<BuzzControllerNumber>().ToList().ForEach(x => SetBuzzerLight(x, false));
        }

        public void SetBuzzerLight(BuzzControllerNumber controllerNumber, bool on)
        {
            _lastWriteData[1 + (int)controllerNumber] = (byte)(on ? 1 : 0);
            WriteData(_lastWriteData);
        }
        public bool GetBuzzerLight(BuzzControllerNumber controllerNumber)
        {
            return _lastWriteData[1 + (int)controllerNumber] > 0;

        }

        public void ToggleBuzzerLight(BuzzControllerNumber controllerNumber)
        {
            int dataByte = 1 + (int)controllerNumber;
            _lastWriteData[dataByte] = (byte)(_lastWriteData[dataByte] == 0 ? 1 : 0);
            WriteData(_lastWriteData);
        }

        #endregion

        #region Internal functionality
        private void _device_dataReceived(byte[] message)
        {
            try
            {
                //read through all 20 bits related to 4 controllers with 5 buttons each
                BitArray lastBitArray = new(_lastReadData);
                BitArray currentBitArray = new(message);
                int bitOffset = 24;
                for (int bitCounter = 0; bitCounter < 20; bitCounter++)
                {
                    var action = GetActionFromBitChange(lastBitArray[bitOffset + bitCounter], currentBitArray[bitOffset + bitCounter]);
                    if (action != BuzzButtonAction.Unchanged)
                    {
                        OnButtonStateChanged(bitCounter / 5, bitCounter % 5, action);
                    }
                }
                _lastReadData = message.ToArray();

            }
            catch (Exception ex) { throw new Exception($"Error while receiving data from controller. Maybe it was disconnected?. Error was: '{ex.Message}'", ex); }
        }
       
        public void WriteData(byte[] data) => _device.write(data);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) => _device.close();

        protected void OnButtonStateChanged(int controllerNumber, int buttonNumber, BuzzButtonAction action)
        {
            ButtonStateChanged?.Invoke(this, new BuzzControllerEventArgs(controllerNumber, buttonNumber, action));
        }

        private BuzzButtonAction GetActionFromBitChange(bool previous, bool current)
        {
            if (previous == current) { return BuzzButtonAction.Unchanged; }
            if (previous) { return BuzzButtonAction.Released; }
            else { return BuzzButtonAction.Pressed; }
        } 
        #endregion

    }
}