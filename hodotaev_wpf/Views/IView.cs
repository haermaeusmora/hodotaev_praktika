namespace hodotaev_wpf.Views;

public interface IView
{
    void Show();
    void Close();
    void ShowError(string message, string title = "Ошибка");
    void ShowWarning(string message, string title = "Предупреждение");
    void ShowInfo(string message, string title = "Информация");
    bool ShowConfirmation(string message, string title = "Подтверждение");
}
