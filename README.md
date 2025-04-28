# SmartWallet
## Диаграмма базы данных
```mermaid
erDiagram
    CashVault }|--|| User : is
    CashVault {
        guid id PK
        guid userId FK
        string name
        double value
        Date deletedAt
    }

    SpendingArea }|--|| User : is
    SpendingArea{
        guid id PK
        guid userId FK
        string name
        double value
        Date deletedAt
    }

    Transaction }o--|| User : is
    Transaction }o--|| CashVault : is
    Transaction }o--|| SpendingArea : is
    Transaction {
        guid id PK
        guid userId FK
        guid fromCashVaultId FK
        guid toSpendingAreaId FK
        double value
        Date madeAt
        Date deletedAt
    }

    User {
        guid id PK
        string email PK
        string firstName
        string lastName
        string patronymic
        string hashedPassword
    }
```
## Диаграмма последовательности взаимодействия клиента и сервера
```mermaid
sequenceDiagram
  
```
## Возможные улучшения
 - Добать под области трат, которые находятся в областях трат, а также могут иметь в себе свои под области. Возможно для хранения под областей стоит использовать графовую БД 
 - Добавить интеграцию с банками