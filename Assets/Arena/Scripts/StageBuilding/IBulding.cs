using UnityEngine;

namespace Arena.Scripts.StageBuilding
{
    public interface IBuilding
    {
        // Основные свойства здания
        int Cost { get; }               // Базовая стоимость постройки
        Sprite Sprite { get; }            // Иконка для UI
        string Name { get; }               // Название здания
        string Description { get; }        // Описание
        int CurrentLevel { get; }          // Текущий уровень улучшения
        int UpgradeCost { get; }
        float UpdateUpCost { get; } // Стоимость улучшения
        Collider2D Collider { get; }       // Коллайдер для проверки пересечений
        
        // Методы взаимодействия
        void Place(Vector2 position);      // Установить здание на карту
        void Upgrade();                    // Улучшить здание
        void Demolish(float refundPercent);// Снести здание (возвращает % стоимости)
    }
}