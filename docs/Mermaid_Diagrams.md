# Диаграммы Mermaid для сопроводительной записки hodotaev_praktika

---

## ДИАГРАММА 1: Архитектура системы

```mermaid
flowchart TB
    subgraph Presentation["Уровень представления (Presentation Layer)"]
        WPF[["hodotaev_wpf (WPF Приложение)"]]
        MW[MainWindow]
        PEW[PartnerEditWindow]
        SHW[SalesHistoryWindow]
    end
    
    subgraph Business["Уровень бизнес-логики (Business Logic Layer)"]
        LIB[["hodotaev_library (Библиотека)"]]
        PS[PartnerService]
        IPS[IPartnerService]
    end
    
    subgraph Data["Уровень данных (Data Layer)"]
        EF[Entity Framework Core]
        CTX[HodotaevPraktikaContext]
        MOD[Модели данных]
        PG[(PostgreSQL<br/>База данных)]
    end
    
    WPF --> MW
    WPF --> PEW
    WPF --> SHW
    
    MW --> PS
    PEW --> PS
    SHW --> PS
    
    PS --> IPS
    PS --> CTX
    CTX --> EF
    EF --> MOD
    MOD --> PG
    
    style Presentation fill:#e1f5ff
    style Business fill:#fff4e1
    style Data fill:#e8f5e9
```

---

## ДИАГРАММА 2: Диаграмма классов моделей данных

```mermaid
classDiagram
    class HodotaevPartner {
        +int PartnerId
        +int PartnerTypeId
        +string CompanyName
        +string LegalAddress
        +string Inn
        +string DirectorFullName
        +string Phone
        +string Email
        +int Rating
        +DateTime CreatedAt
        +DateTime UpdatedAt
        +HodotaevPartnerType PartnerType
        +ICollection~HodotaevSale~ Sales
        +ICollection~HodotaevPartnerRatingHistory~ RatingHistory
        +decimal Discount
        +decimal TotalSalesAmount
    }
    
    class HodotaevPartnerType {
        +int PartnerTypeId
        +string TypeName
        +string Description
        +ICollection~HodotaevPartner~ Partners
    }
    
    class HodotaevProduct {
        +int ProductId
        +string ProductName
        +string Article
        +string ProductType
        +string Description
        +decimal MinPrice
        +string Unit
        +DateTime CreatedAt
        +ICollection~HodotaevSale~ Sales
    }
    
    class HodotaevSale {
        +int SaleId
        +int PartnerId
        +int ProductId
        +int Quantity
        +decimal SalePrice
        +DateTime SaleDate
        +DateTime CreatedAt
        +HodotaevPartner Partner
        +HodotaevProduct Product
    }
    
    class HodotaevPartnerRatingHistory {
        +int HistoryId
        +int PartnerId
        +int? OldRating
        +int NewRating
        +string ChangeReason
        +DateTime ChangedAt
        +string ChangedBy
        +HodotaevPartner Partner
    }
    
    HodotaevPartner "1" *-- "1" HodotaevPartnerType : тип партнера
    HodotaevPartner "1" *-- "0..*" HodotaevSale : продажи
    HodotaevPartner "1" *-- "0..*" HodotaevPartnerRatingHistory : история рейтинга
    HodotaevSale "0..*" --> "1" HodotaevProduct : продукт
```

---

## ДИАГРАММА 3: Диаграмма классов сервисов

```mermaid
classDiagram
    class IPartnerService {
        <<interface>>
        +Task~List~HodotaevPartner~~ GetAllPartnersAsync()
        +Task~HodotaevPartner?~ GetPartnerByIdAsync(int id)
        +Task~HodotaevPartner~ AddPartnerAsync(HodotaevPartner partner)
        +Task~HodotaevPartner~ UpdatePartnerAsync(HodotaevPartner partner)
        +Task DeletePartnerAsync(int id)
        +Task~List~HodotaevPartnerType~~ GetPartnerTypesAsync()
        +Task~decimal~ CalculateDiscountAsync(int partnerId)
        +Task~List~HodotaevSale~~ GetPartnerSalesHistoryAsync(int partnerId)
        +Task LogRatingChangeAsync(int partnerId, int oldRating, int newRating, string reason, string changedBy)
    }
    
    class PartnerService {
        -HodotaevPraktikaContext _context
        +PartnerService(HodotaevPraktikaContext context)
        +Task~List~HodotaevPartner~~ GetAllPartnersAsync()
        +Task~HodotaevPartner?~ GetPartnerByIdAsync(int id)
        +Task~HodotaevPartner~ AddPartnerAsync(HodotaevPartner partner)
        +Task~HodotaevPartner~ UpdatePartnerAsync(HodotaevPartner partner)
        +Task DeletePartnerAsync(int id)
        +Task~List~HodotaevPartnerType~~ GetPartnerTypesAsync()
        +decimal CalculateDiscount(decimal totalSalesAmount)
        +Task~decimal~ CalculateDiscountAsync(int partnerId)
        +Task~List~HodotaevSale~~ GetPartnerSalesHistoryAsync(int partnerId)
        +Task LogRatingChangeAsync(...)
        -void ValidatePartner(HodotaevPartner partner)
        -bool IsValidEmail(string email)
    }
    
    class ValidationException {
        +ValidationException(string message)
    }
    
    IPartnerService <|.. PartnerService : реализует
    PartnerService ..> ValidationException : выбрасывает
```

---

## ДИАГРАММА 4: ER-диаграмма базы данных

```mermaid
erDiagram
    hodotaev_partner_types {
        int partner_type_id PK
        varchar type_name UK
        varchar description
    }
    
    hodotaev_partners {
        int partner_id PK
        int partner_type_id FK
        varchar company_name
        varchar legal_address
        varchar inn
        varchar director_full_name
        varchar phone
        varchar email
        int rating
        timestamp created_at
        timestamp updated_at
    }
    
    hodotaev_products {
        int product_id PK
        varchar article UK
        varchar product_name
        varchar product_type
        varchar description
        decimal min_price
        varchar unit
        timestamp created_at
    }
    
    hodotaev_sales {
        int sale_id PK
        int partner_id FK
        int product_id FK
        int quantity
        decimal sale_price
        date sale_date
        timestamp created_at
    }
    
    hodotaev_partner_rating_history {
        int history_id PK
        int partner_id FK
        int old_rating
        int new_rating
        varchar change_reason
        timestamp changed_at
        varchar changed_by
    }
    
    hodotaev_partner_types ||--o{ hodotaev_partners : "has"
    hodotaev_partners ||--o{ hodotaev_sales : "makes"
    hodotaev_products ||--o{ hodotaev_sales : "included_in"
    hodotaev_partners ||--o{ hodotaev_partner_rating_history : "has_history"
```

---

## ДИАГРАММА 5: Паттерн MVP

```mermaid
flowchart LR
    subgraph View["View (Представление)"]
        IView[IMainView<br/>IPartnerEditView<br/>ISalesHistoryView]
        WPF[MainWindow<br/>PartnerEditWindow<br/>SalesHistoryWindow]
    end
    
    subgraph Presenter["Presenter (Презентер)"]
        MP[MainPresenter]
        PEP[PartnerEditPresenter]
        SHP[SalesHistoryPresenter]
    end
    
    subgraph Model["Model (Модель)"]
        PS[PartnerService]
        MOD[Модели данных]
    end
    
    WPF --> IView : реализует
    IView --> MP : события
    IView --> PEP : события
    IView --> SHP : события
    
    MP --> PS : вызов методов
    PEP --> PS : вызов методов
    SHP --> PS : вызов методов
    
    MP --> WPF : обновление UI
    PEP --> WPF : обновление UI
    SHP --> WPF : обновление UI
    
    PS --> MOD : работа с данными
    
    style View fill:#e1f5ff
    style Presenter fill:#fff4e1
    style Model fill:#e8f5e9
```

---

## ДИАГРАММА 6: Схема главного окна

```mermaid
flowchart TB
    subgraph MainWindow["MainWindow - Главное окно"]
        Title[Заголовок: "Управление партнерами"]
        
        subgraph Toolbar["Панель инструментов"]
            BtnAdd[Кнопка "Добавить"]
            BtnEdit[Кнопка "Редактировать"]
            BtnDelete[Кнопка "Удалить"]
            BtnHistory[Кнопка "История продаж"]
            BtnRefresh[Кнопка "Обновить"]
            BtnExit[Кнопка "Выход"]
        end
        
        subgraph Content["Основная область"]
            subgraph LeftPanel["Левая панель"]
                ListBox[ListBox: Список партнеров]
            end
            
            subgraph RightPanel["Правая панель"]
                CardTitle[Заголовок: "Информация о партнере"]
                Company[Компания]
                PartnerType[Тип партнера]
                Director[Директор]
                Phone[Телефон]
                Email[Email]
                Rating[Рейтинг]
                Address[Адрес]
                Inn[ИНН]
                Stats[Блок статистики<br/>Объем продаж<br/>Скидка]
            end
        end
        
        subgraph StatusBar["Строка состояния"]
            Status[Статус]
            Count[Количество партнеров]
        end
    end
    
    Title --> Toolbar
    Toolbar --> Content
    Content --> StatusBar
    
    LeftPanel --> RightPanel : выделение партнера
```

---

## ДИАГРАММА 7: Последовательность добавления партнера

```mermaid
sequenceDiagram
    participant U as Пользователь
    participant V as PartnerEditWindow
    participant P as PartnerEditPresenter
    participant S as PartnerService
    participant DB as База данных
    
    U->>V: Нажимает "Добавить"
    V->>P: Создает презентер
    P->>S: GetPartnerTypesAsync()
    S->>DB: SELECT * FROM partner_types
    DB-->>S: Список типов
    S-->>P: Типы партнеров
    P->>V: Заполняет ComboBox
    
    U->>V: Вводит данные партнера
    U->>V: Нажимает "Сохранить"
    V->>P: SaveRequested
    
    P->>P: Валидация данных
    alt Данные некорректны
        P->>V: ShowError()
        V->>U: Сообщение об ошибке
    else Данные корректны
        P->>S: AddPartnerAsync(partner)
        S->>DB: INSERT INTO partners
        DB-->>S: ID нового партнера
        S-->>P: Сохраненный партнер
        P->>V: DialogResult = true
        V->>U: Сообщение об успехе
        V-->>MainWindow: Обновить список
    end
```

---

## ДИАГРАММА 8: Последовательность расчета скидки

```mermaid
sequenceDiagram
    participant S as PartnerService
    participant DB as База данных
    participant F as Функция БД
    
    S->>DB: SELECT SUM(quantity * sale_price)<br/>FROM sales WHERE partner_id = X
    DB-->>S: totalSales = 150000
    
    S->>S: CalculateDiscount(totalSales)
    
    alt totalSales < 10000
        S->>S: discount = 0
    else 10000 <= totalSales < 50000
        S->>S: discount = 5
    else 50000 <= totalSales < 300000
        S->>S: discount = 10
    else totalSales >= 300000
        S->>S: discount = 15
    end
    
    S-->>UI: Возврат discount (10%)
```

---

## ДИАГРАММА 9: Активность удаления партнера

```mermaid
flowchart TD
    Start([Начало]) --> Select{Партнер<br/>выбран?}
    
    Select -->|Нет| Warn1[Показать предупреждение]
    Warn1 --> End([Конец])
    
    Select -->|Да| Confirm{Подтверждение<br/>удаления?}
    
    Confirm -->|Нет| End
    
    Confirm -->|Да| Check{Есть<br/>продажи?}
    
    Check -->|Да| Error1[Показать ошибку<br/>"Существуют записи о продажах"]
    Error1 --> End
    
    Check -->|Нет| Delete[DELETE FROM partners<br/>WHERE partner_id = X]
    
    Delete --> Success[Показать сообщение<br/>"Партнер успешно удален"]
    
    Success --> Refresh[Обновить список]
    
    Refresh --> End
```

---

## ДИАГРАММА 10: Состояния приложения

```mermaid
stateDiagram-v2
    [*] --> Initializing: Запуск приложения
    
    Initializing --> LoadingData: Инициализация завершена
    LoadingData --> Ready: Данные загружены
    
    Ready --> ViewingList: Просмотр списка
    Ready --> EditingPartner: Редактирование партнера
    Ready --> ViewingHistory: Просмотр истории
    
    ViewingList --> Ready: Выбор партнера
    ViewingList --> Ready: Обновление
    
    EditingPartner --> Ready: Сохранено
    EditingPartner --> Ready: Отменено
    
    ViewingHistory --> Ready: Закрыто
    
    Ready --> Error: Ошибка БД
    Error --> Ready: Повторная попытка
    Error --> [*]: Закрытие
    
    Ready --> [*]: Выход
```

---

## ДИАГРАММА 11: Компонентная диаграмма

```mermaid
flowchart TB
    subgraph App["hodotaev_praktika"]
        subgraph WPF["hodotaev_wpf"]
            MW[MainWindow]
            PEW[PartnerEditWindow]
            SHW[SalesHistoryWindow]
            App[App.xaml.cs]
        end
        
        subgraph Lib["hodotaev_library"]
            subgraph Models["Models"]
                Partner[HodotaevPartner]
                PartnerType[HodotaevPartnerType]
                Product[HodotaevProduct]
                Sale[HodotaevSale]
                RatingHist[HodotaevPartnerRatingHistory]
            end
            
            subgraph Data["Data"]
                Context[HodotaevPraktikaContext]
            end
            
            subgraph Services["Services"]
                IPS[IPartnerService]
                PS[PartnerService]
            end
        end
        
        subgraph Tests["hodotaev_library.Tests"]
            DiscountTests[DiscountCalculationTests]
            ValidationTests[PartnerValidationTests]
        end
    end
    
    MW --> PS
    PEW --> PS
    SHW --> PS
    
    PS --> Context
    PS --> IPS
    
    Context --> Partner
    Context --> PartnerType
    Context --> Product
    Context --> Sale
    Context --> RatingHist
    
    DiscountTests --> PS
    ValidationTests --> PS
```

---

## ДИАГРАММА 12: Диаграмма развертывания

```mermaid
flowchart TB
    subgraph Client["Клиентский компьютер"]
        App[["hodotaev_wpf.exe<br/>(.NET 8.0 App)"]]
        Runtime[".NET 8.0 Runtime"]
    end
    
    subgraph Server["Сервер БД"]
        PG[["PostgreSQL 17<br/>(hodotaev_praktika)"]]
        Schema[Схема: app]
        Tables[Таблицы:<br/>- hodotaev_partner_types<br/>- hodotaev_partners<br/>- hodotaev_products<br/>- hodotaev_sales<br/>- hodotaev_partner_rating_history]
    end
    
    App --> Runtime : использует
    App -->|Npgsql 8.0<br/>TCP/IP:5432| PG
    PG --> Schema : содержит
    Schema --> Tables : содержит
    
    style Client fill:#e1f5ff
    style Server fill:#e8f5e9
```

---
