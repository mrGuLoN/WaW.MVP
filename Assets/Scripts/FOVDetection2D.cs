using System.Collections;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

public class FOVDetection2D : MonoBehaviour
{
    public List<Transform> viewPoints = new(); // Массив точек зрения
    [SerializeField] public float viewRadius; // Радиус области зрения
    public LayerMask obstacleMask; // Маска для препятствий
    public List<EnemyController> enemies = new(); // Список противников

    private void Start()
    {
        // Запуск корутины для обновления области зрения
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    private IEnumerator FindTargetsWithDelay(float delay)
    {
        // Поиск целей с задержкой
        while (true)
        {
            yield return new WaitForSeconds(delay);
            foreach (Transform viewPoint in viewPoints)
            {
                NativeArray<Vector2> enemyPositions = new NativeArray<Vector2>(GetEnemyPositions(), Allocator.TempJob);
                NativeArray<bool> results = new NativeArray<bool>(enemies.Count, Allocator.TempJob);

                CheckEnemiesVisibilityJob job = new CheckEnemiesVisibilityJob
                {
                    viewPointPosition = viewPoint.position,
                    viewRadius = viewRadius,
                    enemies = enemyPositions,
                    results = results,
                };

                JobHandle handle = job.Schedule(enemies.Count, 1);
                handle.Complete();
              
                for (int i = 0; i < enemies.Count; i++)
                {
                    if (results[i])
                    {
                        float distanceToEnemy = Vector2.Distance(viewPoint.position, enemyPositions[i]);

                        // Проверка наличия препятствий
                        if (!Physics2D.Raycast(viewPoint.position, viewPoint.up, distanceToEnemy, obstacleMask))
                        {
                            enemies[i].visual.enabled=true; // Враг виден
                        }
                        else
                        {
                            enemies[i].visual.enabled=false;// Враг не виден из-за препятствия
                        }
                    }
                    else
                    {
                        enemies[i].visual.enabled=false; // Враг не виден в радиусе
                    }
                }

                // Освобождение ресурсов
                enemyPositions.Dispose();
                results.Dispose();
            }
        }
    }

    private Vector2[] GetEnemyPositions()
    {
        Vector2[] positions = new Vector2[enemies.Count];
        for (int i = 0; i < enemies.Count; i++)
        {
            positions[i] = enemies[i].thisTR.position;
        }
        return positions;
    }
   

    // Job для проверки видимости врагов без Raycast
    private struct CheckEnemiesVisibilityJob : IJobParallelFor
    {
        public Vector2 viewPointPosition;
        public float viewRadius;
        [ReadOnly] public NativeArray<Vector2> enemies; // Позиции врагов
        public NativeArray<bool> results; // Результаты видимости
       

        public void Execute(int index)
        {
            Vector2 enemyPosition = enemies[index];

            // Проверка расстояния до врага
            float distanceToEnemy = Vector2.Distance(viewPointPosition, enemyPosition);
            if (distanceToEnemy <= viewRadius)
            {
               results[index] = true; // Враг потенциально виден
            }
            else
            {
                results[index] = false; // Враг вне радиуса видимости
            }
        }
    }
}