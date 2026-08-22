using EduPath.Avalonia.Data;
using EduPath.Avalonia.Models;

namespace EduPath.Avalonia.Services
{
    /// <summary>Nghiệp vụ cho màn 14 · Quản lý đợt đăng ký, và cấp thông tin đợt hiện hành cho dashboard SV.</summary>
    public class RegistrationPeriodService
    {
        private readonly InMemoryStore _store = InMemoryStore.Instance;

        public IReadOnlyList<RegistrationPeriod> GetAll() => _store.Periods;

        public RegistrationPeriod? GetCurrent() =>
            _store.Periods.FirstOrDefault(p => p.IsOpen) ?? _store.Periods.FirstOrDefault();

        public (bool ok, string? error) Open(string name)
        {
            var period = _store.Periods.FirstOrDefault(p => p.Name == name);
            if (period is null) return (false, "Không tìm thấy đợt đăng ký.");

            bool anotherOpen = _store.Periods.Any(p => p.Name != name && p.IsOpen);
            if (anotherOpen)
                return (false, "Đã có một đợt đăng ký khác đang mở. Vui lòng đóng đợt đó trước.");

            period.IsOpen = true;
            return (true, null);
        }

        public (bool ok, string? error) Close(string name)
        {
            var period = _store.Periods.FirstOrDefault(p => p.Name == name);
            if (period is null) return (false, "Không tìm thấy đợt đăng ký.");
            period.IsOpen = false;
            return (true, null);
        }
    }
}
