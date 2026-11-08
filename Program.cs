using System;
using System.Collections.Generic;

namespace HomeApplianceRepairCenter
{
    // تمثل حالة الجهاز 
    public enum RepairStatus
    {
        Received,
        UnderRepair,
        Repaired
    }

    // ===================== 1. التجريد (Abstraction) =====================

    // تعريف الانترفيس تمثل العلميات المشتركة 
    public interface IDevice
    {
        string SerialNumber { get; }
        string Name { get; }
        RepairStatus Status { get; }

        decimal CalculateRepairCost();
        void ShowInfo();
    }

    public abstract class Device : IDevice
    {
        private readonly string _serialNumber;
        private string _name;
        private RepairStatus _status;

        protected Device(string serialNumber, string name)
        {
            _serialNumber = serialNumber;
            _name = name;
            _status = RepairStatus.Received;
        }

        public string SerialNumber => _serialNumber;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public RepairStatus Status
        {
            get => _status;
            set => _status = value;
        }

        // دالة حساب التكلفة 
        public abstract decimal CalculateRepairCost();

        public virtual void ShowInfo()
        {
            Console.WriteLine($"[{GetType().Name}] {Name} | Serial: {SerialNumber} | Status: {Status} | Cost: {CalculateRepairCost():N0}");
        }
    }

    // ============== 2. الوراثة وتعدد الأشكال (Inheritance & Polymorphism) ==============

    // الثلاجة 
    public class Refrigerator : Device
    {
        private readonly bool _needsGasRefill;
        private readonly bool _compressorFailure;

        public Refrigerator(string serialNumber, string name, bool needsGasRefill, bool compressorFailure)
            : base(serialNumber, name)
        {
            _needsGasRefill = needsGasRefill;
            _compressorFailure = compressorFailure;
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = 20000;
            if (_needsGasRefill) cost += 35000;
            if (_compressorFailure) cost += 120000;
            return cost;
        }
    }

    // الغسالة 
    public class WashingMachine : Device
    {
        private readonly bool _motorReplacement;
        private readonly int _laborHours;

        public WashingMachine(string serialNumber, string name, bool motorReplacement, int laborHours)
            : base(serialNumber, name)
        {
            _motorReplacement = motorReplacement;
            _laborHours = laborHours;
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = _laborHours * 15000m; 
            if (_motorReplacement) cost += 90000;
            return cost;
        }
    }

    // المكيف 
    public class AirConditioner : Device
    {
        private readonly int _pipeLengthMeters;
        private readonly bool _needsCleaning;

        public AirConditioner(string serialNumber, string name, int pipeLengthMeters, bool needsCleaning)
            : base(serialNumber, name)
        {
            _pipeLengthMeters = pipeLengthMeters;
            _needsCleaning = needsCleaning;
        }

        public override decimal CalculateRepairCost()
        {
            decimal cost = 30000 + (_pipeLengthMeters * 8000m);
            if (_needsCleaning) cost += 25000;
            return cost;
        }
    }

    
    public class RepairCenter
    {
        private readonly List<Device> _devices = new List<Device>();

        public void AddDevice(Device device)
        {
            _devices.Add(device);
        }

        // 
        public void ShowAllDevices()
        {
            foreach (Device device in _devices)
            {
                device.ShowInfo();
            }
        }

        // حساب التكلفة بغض النطر عن الجهاز 
        public decimal CalculateTotalCost()
        {
            decimal total = 0;
            foreach (Device device in _devices)
            {
                total += device.CalculateRepairCost();
            }
            return total;
        }
    }

    internal class Program
    {
        private static void Main()
        {
            var center = new RepairCenter();

            center.AddDevice(new Refrigerator("FR-1001", "Samsung Fridge", true, false));
            center.AddDevice(new WashingMachine("WM-2002", "LG Washer", true, 2));
            center.AddDevice(new AirConditioner("AC-3003", "Gree Split AC", 5, true));

            Console.WriteLine("===== Devices in Repair Center =====");
            center.ShowAllDevices();

            Console.WriteLine();
            Console.WriteLine($"Total repair cost: {center.CalculateTotalCost():N0}");

            Console.ReadKey();
        }
    }
}
