using System;
using System.Collections.Generic;
using System.Linq;
using Mighty.HID;
using Serilog.Core;

namespace SteelSeries.Arctis7.Gear
{
    internal class Arctis7Reader
    {
        public const byte muteStatus = 0x30;
        private const byte batteryAdress = 0x18;
        private readonly Logger _logger;

        //->Mute status(0-Not muted, 1-Muted)

        private HIDDev _dev = null;

        public Arctis7Reader(Logger logger)
        {
            _logger = logger;
        }

        public bool TryInitHIDDev()
        {
            byte devNumber = 0;

            List<HIDInfo> devs = HIDBrowse.Browse().FindAll(x => (x.Pid == 4781 && x.Vid == 4152));

            if (devs.Count == 0)
            {
                _logger.Warning("Arctis 7 HID Device not found!");
                return false;
            }

            do
            {
                _dev = new HIDDev();
                _dev.Open(devs.ElementAt(devNumber));
                devNumber++;
            }
            while (devNumber < devs.Count && !TryReadBattery(out int batCharge));

            if (devNumber >= devs.Count)
            {
                _logger.Warning("None of the Arctis 7 HID Devices responded!");
                return false;
            }

            _logger.Information("Device initialized successfully");

            return true;
        }

        public bool TryReadBattery(out int batteryCharge)
        {
            batteryCharge = 0;
            try
            {
                if (_dev == null)
                {
                    return false;
                }

                // Set message to send
                byte[] report = new byte[32];
                report[0] = 0x06;
                report[1] = batteryAdress;

                _dev.Write(report);

                //neeed 31 (by testing)
                byte[] reportIn = new byte[31];

                _dev.Read(reportIn);

                if (reportIn[0] == 0x06 && reportIn[1] == report[1])
                {
                    batteryCharge = reportIn[2];
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Device read failed: {ex.Message}");

                return false;
            }
        }
    }
}