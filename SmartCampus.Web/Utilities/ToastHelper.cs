using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartCampus.Web.Utilities
{



    public static class ToastHelper
    {
        public enum ToastType
        {
            Success,
            Error,
            Warning,
            Info
        }

        public static void ShowToast(PageModel pageModel, string message, ToastType type = ToastType.Info)
        {
            if (pageModel?.TempData == null) return;

            string typeString = type switch
            {
                ToastType.Success => "success",
                ToastType.Error => "danger",
                ToastType.Warning => "warning",
                ToastType.Info => "info",
                _ => "info"
            };

            pageModel.TempData["ToastMessage"] = message;
            pageModel.TempData["ToastType"] = typeString;
        }

        public static void ShowSuccess(PageModel pageModel, string message)
            => ShowToast(pageModel, message, ToastType.Success);

        public static void ShowError(PageModel pageModel, string message)
            => ShowToast(pageModel, message, ToastType.Error);

        public static void ShowWarning(PageModel pageModel, string message)
            => ShowToast(pageModel, message, ToastType.Warning);

        public static void ShowInfo(PageModel pageModel, string message)
            => ShowToast(pageModel, message, ToastType.Info);
    }
}