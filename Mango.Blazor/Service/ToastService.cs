using Mango.Blazor.Models.Enums;
using Mango.Blazor.Service.IService;

namespace Mango.Blazor.Service
{
    public class ToastService : IToastService
    {
        public event Action<ToastLevel, string, string?>? OnToastAdded;

        public void ShowToast(ToastLevel level, string message, string? title = null)
        {
            OnToastAdded?.Invoke(level, message, title);
        }
    }
}
