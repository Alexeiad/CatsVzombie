using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class ZombiePath : MonoBehaviour
{
    [SerializeField] private SplineContainer _splineContainer;

    // Новая зона ограничения для точек (прямоугольник)
    [SerializeField] private Vector2 _boundsMin = new Vector2(-10f, -10f); // Нижний левый угол зоны
    [SerializeField] private Vector2 _boundsMax = new Vector2(10f, 10f);   // Верхний правый угол зоны

    [SerializeField] private int _knotCount = 10; // Количество точек (можно менять)

    // Параметры хаотичного движения точек
    [SerializeField] private float _knotDriftSpeed = 1f;      // Скорость дрейфа точек (единиц/сек)
    [SerializeField] private float _driftFrequency = 0.2f;   // Частота шума Perlin (меньше = медленнее/плавнее)

    [SerializeField] private float _normalizedSpeed = 0.15f; // Скорость движения зомби по параметру t (0..1 в секунду, ≈ обороты пути)

    private Spline _spline;
    private List<float> _perlinOffsets; // Уникальные оффсеты для Perlin-шума каждой точки

    private float _t = 0f; // Нормализованный параметр положения зомби (0..1)

    private void Start()
    {
        _spline = new Spline { Closed = true };
        _perlinOffsets = new List<float>();

        // Генерация начальных точек внутри заданной зоны
        for (int i = 0; i < _knotCount; i++)
        {
            float randomX = UnityEngine.Random.Range(_boundsMin.x, _boundsMax.x);
            float randomY = UnityEngine.Random.Range(_boundsMin.y, _boundsMax.y);

            _spline.Add(new BezierKnot(new Vector3(randomX, randomY, 0)));
            _perlinOffsets.Add (UnityEngine.Random.Range(0f, 1000f)); // Уникальный seed для каждой точки
        }

        UpdateTangents(); // Делаем сплайн гладким (Catmull-Rom стиль)
        _splineContainer.AddSpline(_spline);

        // Стартовая позиция зомби — начало сплайна
        _spline.Evaluate(0f, out float3 startPos, out _, out _);
        transform.position = startPos;
    }

    private void Update()
    {
        // === 1. Хаотичное движение точек ===
        float time = Time.time;
        int count = _spline.Count;

        for (int i = 0; i < count; i++)
        {
            float offset = _perlinOffsets[i];

            // Perlin-шум даёт плавное, органичное движение (значения от -1 до 1)
            float noiseX = (Mathf.PerlinNoise(time * _driftFrequency + offset, offset + 100f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(time * _driftFrequency + offset + 200f, offset + 300f) - 0.5f) * 2f;

            Vector3 drift = new Vector3(noiseX, noiseY, 0f) * _knotDriftSpeed * Time.deltaTime;

            BezierKnot knot = _spline[i];
            Vector3 newPos = (Vector3)knot.Position + drift;

            // Ограничиваем точку внутри заданной зоны
            newPos.x = Mathf.Clamp(newPos.x, _boundsMin.x, _boundsMax.x);
            newPos.y = Mathf.Clamp(newPos.y, _boundsMin.y, _boundsMax.y);
            newPos.z = 0f;

            knot.Position = newPos;
            _spline[i] = knot;
        }

        // Пересчитываем тангенсы, чтобы сплайн оставался гладким после перемещения точек
        UpdateTangents();

        // === 2. Движение зомби по сплайну ===
        _t += _normalizedSpeed * Time.deltaTime;
        _t = _t % 1f;
        if (_t < 0f) _t += 1f;

        _spline.Evaluate(_t, out float3 position, out _, out _);
        transform.position = position;
    }

    // Метод для поддержания Catmull-Rom стиля (плавные скругления без углов)
    private void UpdateTangents()
    {
        int count = _spline.Count;
        for (int i = 0; i < count; i++)
        {
            int prevIndex = (i - 1 + count) % count;
            int nextIndex = (i + 1) % count;

            Vector3 delta = _spline[nextIndex].Position - _spline[prevIndex].Position;
            Vector3 tangent = delta / 6f; // Стандартный коэффициент для Catmull-Rom

            BezierKnot knot = _spline[i];
            knot.TangentIn = -tangent;
            knot.TangentOut = tangent;
            _spline[i] = knot;
        }
    }
}