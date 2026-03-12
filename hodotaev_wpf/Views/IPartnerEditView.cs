using hodotaev_library.Models;

namespace hodotaev_wpf.Views;

/// <summary>
/// Интерфейс окна добавления/редактирования партнера
/// </summary>
public interface IPartnerEditView : IView
{
    /// <summary>
    /// Партнер для редактирования (null для нового)
    /// </summary>
    HodotaevPartner? Partner { get; set; }

    /// <summary>
    /// Список типов партнеров для выпадающего списка
    /// </summary>
    IEnumerable<HodotaevPartnerType> PartnerTypes { get; set; }

    /// <summary>
    /// Заголовок окна
    /// </summary>
    string WindowTitle { get; set; }

    /// <summary>
    /// Событие сохранения
    /// </summary>
    event Action SaveRequested;

    /// <summary>
    /// Событие отмены
    /// </summary>
    event Action CancelRequested;

    /// <summary>
    /// Результат редактирования
    /// </summary>
    bool DialogResult { get; set; }

    /// <summary>
    /// Показать модальное окно
    /// </summary>
    void ShowDialog();
}
