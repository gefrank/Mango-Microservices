using Mango.Blazor.Models.Enums;

namespace Mango.Blazor.Service.IService
{
    public interface IToastService
    {
        event Action<ToastLevel, string, string?> OnToastAdded;
        void ShowToast(ToastLevel level, string message, string? title = null);
    }
}
