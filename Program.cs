using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
namespace HomeApplianceRepairCenter
{
    // تمثل حالة الجهاز 
    public enum RepairStatus
    {
        Received,
        UnderRepair,
        Repaired
    }

    // كلاس التحقق من صحة المداخلات   
    public static class InputValidator
    {
        public static int MinSerialNumberLength = 6;
        public static int MaxSerialNumberLength = 20;

        private static readonly Regex _serialNumberPattern =
            new Regex(@"^[A-Z0-9]+(-[A-Z0-9]+)*$", RegexOptions.CultureInvariant);

        public static bool IsValidSerialNumber(string serialNumber)
        {
            return serialNumber != null
                && serialNumber.Length >= MinSerialNumberLength
                && serialNumber.Length <= MaxSerialNumberLength
                && _serialNumberPattern.IsMatch(serialNumber);
        }
    }

    //  تعريف التفويض الخاص بتغيير الحالة
    public delegate void StatusChangedHandler(Device device, RepairStatus oldStatus, RepairStatus newStatus);

                         //          1   -التجريد-

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
            if (!InputValidator.IsValidSerialNumber(serialNumber))
                throw new ArgumentException("الرقم التسلسلي غير صالح.");

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

       //    خاصية الحالة تطلق الحدث لما تتغير
        public RepairStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return; // لا تغيير، لا إشعار

                RepairStatus oldStatus = _status;
                _status = value;
                OnStatusChanged(oldStatus, _status);
            }
        }

        //      حدث تغيير الحالة 
        public event StatusChangedHandler StatusChanged;

        //     دالة إطلاق الحدث      
        protected virtual void OnStatusChanged(RepairStatus oldStatus, RepairStatus newStatus)
        {
            StatusChanged?.Invoke(this, oldStatus, newStatus);
        }

        // دالة حساب التكلفة 
        public abstract decimal CalculateRepairCost();

        public virtual void ShowInfo()
        {
            Console.WriteLine($"[{GetType().Name}] {Name} | Serial: {SerialNumber} | Status: {Status} | Cost: {CalculateRepairCost():N0}");
        }
    }

    //                         - 2. الوراثة وتعدد الأشكال (Inheritance & Polymorphism) -

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

        // قائمة ثابتة بتجمع كل الأجهزة بكل أفرع المركز
        private static readonly List<Device> _allInstitutionDevices = new List<Device>();

        // خصائص ثابتة للإحصائيات العامة
        public static int TotalRegistered => _allInstitutionDevices.Count;
        public static int UnderRepairCount => _allInstitutionDevices.Count(d => d.Status == RepairStatus.UnderRepair);

        public void AddDevice(Device device)
        {
            _devices.Add(device);
            _allInstitutionDevices.Add(device); // نضيف الجهاز للقائمة العامة كمان
            SubscribeToDevice(device);
        }

        // عرض كل الأجهزة الموجودة بالمركز
        public void ShowAllDevices()
        {
            foreach (Device device in _devices)
            {
                device.ShowInfo();
            }
        }

        // حساب التكلفة الإجمالية لكل الأجهزة
        public decimal CalculateTotalCost()
        {
            decimal total = 0;
            foreach (Device device in _devices)
            {
                total += device.CalculateRepairCost();
            }
            return total;
        }

        // ربط الحدث مع الدالة اللي رح تستقبل الإشعار
        private void SubscribeToDevice(Device device)
        {
            device.StatusChanged += OnDeviceStatusChanged;
        }

        // هي الدالة بتشتغل لحالها بس تتغير حالة أي جهاز
        private void OnDeviceStatusChanged(Device device, RepairStatus oldStatus, RepairStatus newStatus)
        {
            Console.WriteLine($"[Notification] {device.Name} ({device.SerialNumber}): {oldStatus} -> {newStatus}");

            if (newStatus == RepairStatus.Repaired)
            {
                Console.WriteLine($"   >> {device.Name} is ready for pickup. Repair cost: {device.CalculateRepairCost():N0}");
            }
        }
    }

    internal class Program
    {
        private static void Main()
        {
            var center = new RepairCenter();

            var fridge = new Refrigerator("FR-1001", "Samsung Fridge", true, false);
            var washer = new WashingMachine("WM-2002", "LG Washer", true, 2);
            var ac = new AirConditioner("AC-3003", "Gree Split AC", 5, true);

            center.AddDevice(fridge);
            center.AddDevice(washer);
            center.AddDevice(ac);

            Console.WriteLine("===== Devices in Repair Center =====");
            center.ShowAllDevices();

            Console.WriteLine();
            Console.WriteLine($"Total repair cost: {center.CalculateTotalCost():N0}");

            //                 4 تجربة تغير حالات الأجهزة  
            Console.WriteLine();
            Console.WriteLine("===== Updating statuses =====");
            fridge.Status = RepairStatus.UnderRepair;
            fridge.Status = RepairStatus.Repaired;
            washer.Status = RepairStatus.UnderRepair;
            // طباعة الإحصائيات العامة للمركز
            Console.WriteLine();
            Console.WriteLine("===== Static Statistics =====");
            Console.WriteLine($"Total registered devices: {RepairCenter.TotalRegistered}");
            Console.WriteLine($"Devices currently under repair: {RepairCenter.UnderRepairCount}");
            Console.ReadKey();
        }
    }
}
