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

    // ===== 4. التفويض (Delegate): شكل الدالة اللي بتستقبل إشعار تغيّر الحالة =====
    public delegate void StatusChangedHandler(Device device, RepairStatus oldStatus, RepairStatus newStatus);

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

        // ===== 4. تعديل: الخاصية Status صارت تطلق الحدث عند التغيير =====
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

        // ===== 4. الحدث (Event) =====
        public event StatusChangedHandler StatusChanged;

        // ===== 4. إطلاق الحدث للمشتركين =====
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
            SubscribeToDevice(device); // ===== 4. الاشتراك بحدث الجهاز =====
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

        // ===== 4. الاشتراك بالحدث =====
        private void SubscribeToDevice(Device device)
        {
            device.StatusChanged += OnDeviceStatusChanged;
        }

        // ===== 4. تُنفذ تلقائياً عند تغيّر حالة أي جهاز =====
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

            // ===== 4. تجربة الأحداث: تغيير الحالات =====
            Console.WriteLine();
            Console.WriteLine("===== Updating statuses =====");
            fridge.Status = RepairStatus.UnderRepair;
            fridge.Status = RepairStatus.Repaired;
            washer.Status = RepairStatus.UnderRepair;

            Console.ReadKey();
        }
    }
}
namespace ApplianceRepairCenter
{
    public enum ApplianceStatus
    {
        Registered,
        Received,
        UnderRepair,
        Repaired,
        Delivered
    }

    public static class InputValidator
    {
        public static int MinSerialNumberLength = 6;
        public static int MaxSerialNumberLength = 20;

        private static readonly Regex s_serialNumberPattern =
            new Regex(@"^[A-Z0-9]+(-[A-Z0-9]+)*$", RegexOptions.CultureInvariant);

        public static bool IsValidSerialNumber(string? serialNumber)
        {
            return serialNumber!= null
                && serialNumber.Length >= MinSerialNumberLength
                && serialNumber.Length <= MaxSerialNumberLength
                && s_serialNumberPattern.IsMatch(serialNumber);
        }
    }

    public class RepairCenter
    {
        public string CenterName { get; private set; }

        private static readonly Dictionary<ApplianceStatus, int> s_statusCounts =
            Enum.GetValues<ApplianceStatus>().ToDictionary(status => status, _ => 0);

        public RepairCenter(string name)
        {
            CenterName = name;
        }

        public static int TotalRegistered
        {
            get { return s_statusCounts.Values.Sum(); }
        }

        public static int UnderRepairCount
        {
            get { return CountByStatus(ApplianceStatus.UnderRepair); }
        }

        public static int CountByStatus(ApplianceStatus status)
        {
            return s_statusCounts[status];
        }

        public void Register(string serialNumber)
        {
            if (!InputValidator.IsValidSerialNumber(serialNumber))
            {
                Console.WriteLine($"خطأ: الرقم التسلسلي '{serialNumber}' غير صالح.");
                return;
            }

            s_statusCounts[ApplianceStatus.Registered]++;
            Console.WriteLine($"تم تسجيل الجهاز {serialNumber} في مركز {CenterName}.");

            HandleApplianceStatusChanged();
        }

        public void ChangeStatus(ApplianceStatus oldStatus, ApplianceStatus newStatus)
        {
            if (s_statusCounts[oldStatus] > 0)
            {
                s_statusCounts[oldStatus]--;
                s_statusCounts[newStatus]++;
                Console.WriteLine($"تغيرت حالة جهاز من {oldStatus} إلى {newStatus} في مركز {CenterName}.");

                HandleApplianceStatusChanged();
            }
        }

        private void HandleApplianceStatusChanged()
        {
            Console.WriteLine("====================================================================");
            Console.WriteLine($"{CenterName}");
            Console.WriteLine($"Registered: {CountByStatus(ApplianceStatus.Registered)} | " +
                              $"Received: {CountByStatus(ApplianceStatus.Received)} | " +
                              $"UnderRepair: {CountByStatus(ApplianceStatus.UnderRepair)} | " +
                              $"Repaired: {CountByStatus(ApplianceStatus.Repaired)} | " +
                              $"Delivered: {CountByStatus(ApplianceStatus.Delivered)}");
            Console.WriteLine("====================================================================\n");
        }

        public static double CalculateTotalCost()
        {
            return 0.0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- بدء سيناريو التجربة (Demo Scenario) ---\n");

            RepairCenter center1 = new RepairCenter("Damascus Home Appliance Repair Center");
            RepairCenter center2 = new RepairCenter("Aleppo Home Appliance Repair Center"); center1.Register("RF-2024-0001");
            center1.Register("TV-2024-0002");
            center2.Register("WM-2024-0003");
            center1.Register("123");

            center1.ChangeStatus(ApplianceStatus.Registered, ApplianceStatus.Received);
            center2.ChangeStatus(ApplianceStatus.Registered, ApplianceStatus.UnderRepair);

            Console.WriteLine($"إجمالي الأجهزة المسجلة في المؤسسة كاملة: {RepairCenter.TotalRegistered}");
            Console.WriteLine($"إجمالي الأجهزة تحت الصيانة حالياً: {RepairCenter.UnderRepairCount}");

            Console.ReadLine();
        }
    }
}