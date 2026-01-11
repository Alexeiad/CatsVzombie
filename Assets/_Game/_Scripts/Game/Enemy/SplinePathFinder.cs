using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(SplineContainer))]
public class SplinePathfinder : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float pointReachThreshold = 0.1f;
    [SerializeField] private float minPointDistance = 3f;
    [SerializeField] private float maxPointDistance = 10f;

    [Header("Настройки препятствий")]
    [SerializeField] private List<Transform> obstacles = new List<Transform>();
    [SerializeField] private float obstacleAvoidanceRadius = 2f;
    [SerializeField] private float avoidanceStrength = 3f;
    [SerializeField] private int maxAvoidanceIterations = 10;
    [SerializeField] private float obstacleClearance = 1.5f;

    [Header("Плавность пути")]
    [SerializeField] private float maxAngleThreshold = 45f;
    [SerializeField] private float curveSmoothness = 2f;
    [SerializeField] private float tangentLengthMultiplier = 0.5f;

    [Header("Настройки поиска пути")]
    [SerializeField] private int pathFindingRays = 16;
    [SerializeField] private float maxPathDeviation = 60f;
    [SerializeField] private float directionChangeProbability = 0.3f;

    [Header("Случайная генерация")]
    [SerializeField] private Vector2 boundsMin = new Vector2(-50, -50);
    [SerializeField] private Vector2 boundsMax = new Vector2(50, 50);
    [SerializeField] private int maxPoints = 10;
    [SerializeField] private float explorationBias = 0.5f;

    [Header("Дебаг")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color safePathColor = Color.green;
    [SerializeField] private Color obstaclePathColor = Color.red;
    [SerializeField] private Color directionColor = Color.cyan;

    private SplineContainer splineContainer;
    private Spline spline;
    private float currentSplinePosition = 0f;
    private bool isPathGenerated = false;
    private bool isMoving = false;
    private List<Vector3> smoothedPath = new List<Vector3>();
    private float splineLength = 0f;
    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private const float STUCK_THRESHOLD = 2f;
    private List<Vector3> debugPathPoints = new List<Vector3>();
    private Vector3 lastGeneratedDirection;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        spline = splineContainer.Spline;
        lastPosition = transform.position;
        lastGeneratedDirection = GetRandomDirection();
    }

    private void Start()
    {
        GenerateRandomPath();
        StartMoving();
    }

    private void Update()
    {
        if (isMoving && isPathGenerated)
        {
            MoveAlongSpline();
            CheckForStuck();
        }
    }

    private void CheckForStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);

        if (distanceMoved < 0.01f && isMoving)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= STUCK_THRESHOLD)
            {
                Debug.LogWarning("Объект застрял! Сбрасываю путь.");
                stuckTimer = 0f;
                currentSplinePosition += 0.1f;

                if (currentSplinePosition >= 0.95f)
                {
                    OnPathCompleted();
                }
                else
                {
                    float3 splinePos = spline.EvaluatePosition(Mathf.Clamp01(currentSplinePosition));
                    transform.position = new Vector3(splinePos.x, splinePos.y, splinePos.z);
                }
            }
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
    }

    public void GenerateRandomPath()
    {
        spline.Clear();
        smoothedPath.Clear();
        debugPathPoints.Clear();
        currentSplinePosition = 0f;
        stuckTimer = 0f;

        Vector3 startPos = transform.position;
        smoothedPath.Add(startPos);

        Vector3 currentPos = startPos;
        Vector3 previousDirection = lastGeneratedDirection;

        int pointsAdded = 1;
        int attempts = 0;
        const int maxAttempts = 100;

        // Сохраняем историю направлений для разнообразия
        List<Vector3> directionHistory = new List<Vector3>();
        directionHistory.Add(previousDirection);

        while (pointsAdded < maxPoints && attempts < maxAttempts)
        {
            attempts++;

            // Генерация следующей точки с учетом истории направлений
            Vector3 nextPos = GenerateDiverseNextPoint(currentPos, previousDirection, directionHistory);
            Vector3 adjustedPos = FindSafePathAroundObstacles(currentPos, nextPos, previousDirection);
            adjustedPos = ClampToBounds(adjustedPos);

            float distance = Vector3.Distance(currentPos, adjustedPos);
            if (distance < minPointDistance * 0.5f)
            {
                continue;
            }

            if (!IsPathCompletelySafe(currentPos, adjustedPos))
            {
                Vector3 alternativePos = FindAlternativeSafePoint(currentPos, nextPos, previousDirection, directionHistory);
                if (alternativePos != currentPos)
                {
                    adjustedPos = alternativePos;
                }
                else
                {
                    continue;
                }
            }

            Vector3 newDirection = (adjustedPos - currentPos).normalized;
            float angle = Vector3.Angle(previousDirection, newDirection);

            // Добавляем промежуточную точку если угол слишком большой
            if (angle > maxAngleThreshold)
            {
                Vector3 midPoint = currentPos + previousDirection * (distance * 0.3f);
                if (IsPointSafe(midPoint))
                {
                    smoothedPath.Add(midPoint);
                    pointsAdded++;
                    directionHistory.Add(previousDirection);
                }
            }

            smoothedPath.Add(adjustedPos);
            directionHistory.Add(newDirection);
            previousDirection = newDirection;
            currentPos = adjustedPos;
            pointsAdded++;

            // Периодически меняем общее направление для исследования
            if (pointsAdded % 3 == 0)
            {
                previousDirection = GetExplorationDirection(previousDirection, directionHistory);
            }
        }

        if (smoothedPath.Count < 2)
        {
            Vector3 randomDir = GetRandomDirection();
            Vector3 backupPoint = FindNearestSafePoint(transform.position + randomDir * 5f);
            smoothedPath.Add(backupPoint);
        }

        // Сохраняем последнее направление для следующего пути
        if (smoothedPath.Count >= 2)
        {
            lastGeneratedDirection = (smoothedPath[smoothedPath.Count - 1] - smoothedPath[smoothedPath.Count - 2]).normalized;
        }

        CheckPathDiversity(directionHistory);

        CreateSmoothSplineFromPath();
        splineLength = CalculateSplineLength();
        isPathGenerated = true;
    }

    private Vector3 GetRandomDirection()
    {
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        return new Vector3(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad),
            Mathf.Sin(randomAngle * Mathf.Deg2Rad),
            0
        );
    }

    private Vector3 GenerateDiverseNextPoint(Vector3 currentPosition, Vector3 previousDirection, List<Vector3> directionHistory)
    {
        float distance = UnityEngine.Random.Range(minPointDistance, maxPointDistance);

        // Определяем желаемое направление с учетом истории
        Vector3 desiredDirection = GetDesiredDirection(previousDirection, directionHistory);

        // Добавляем случайное отклонение
        float maxDeviation = maxPathDeviation * 0.6f;
        float angleChange = UnityEngine.Random.Range(-maxDeviation, maxDeviation);

        // С вероятностью directionChangeProbability делаем более значительное изменение
        if (UnityEngine.Random.value < directionChangeProbability)
        {
            angleChange = UnityEngine.Random.Range(-maxPathDeviation, maxPathDeviation * 1.5f);
        }

        // Применяем отклонение к желаемому направлению
        float desiredAngle = Mathf.Atan2(desiredDirection.y, desiredDirection.x) * Mathf.Rad2Deg;
        float newAngle = desiredAngle + angleChange;

        Vector3 offset = new Vector3(
            Mathf.Cos(newAngle * Mathf.Deg2Rad) * distance,
            Mathf.Sin(newAngle * Mathf.Deg2Rad) * distance,
            0
        );

        Vector3 nextPos = currentPosition + offset;
        return ClampToBounds(nextPos);
    }

    private Vector3 GetDesiredDirection(Vector3 currentDirection, List<Vector3> directionHistory)
    {
        if (directionHistory.Count < 5)
            return currentDirection;

        // Анализируем последние направления
        Vector3 avgRecentDirection = Vector3.zero;
        int lookBack = Mathf.Min(5, directionHistory.Count);

        for (int i = directionHistory.Count - lookBack; i < directionHistory.Count; i++)
        {
            avgRecentDirection += directionHistory[i];
        }
        avgRecentDirection.Normalize();

        // Вычисляем разнообразие направлений
        float diversity = CalculateDirectionDiversity(directionHistory);

        // Если недостаточно разнообразия, предлагаем противоположное направление
        if (diversity < 0.3f && UnityEngine.Random.value < explorationBias)
        {
            Vector3 oppositeDirection = -avgRecentDirection;

            // Добавляем небольшое случайное отклонение
            float randomAngle = UnityEngine.Random.Range(-30f, 30f);
            return Quaternion.Euler(0, 0, randomAngle) * oppositeDirection;
        }

        return currentDirection;
    }

    private float CalculateDirectionDiversity(List<Vector3> directions)
    {
        if (directions.Count < 2) return 1f;

        Vector3 sum = Vector3.zero;
        foreach (var dir in directions)
        {
            sum += dir;
        }

        float avgLength = sum.magnitude / directions.Count;
        return Mathf.Clamp01(1f - avgLength);
    }

    private Vector3 GetExplorationDirection(Vector3 currentDirection, List<Vector3> directionHistory)
    {
        // Иногда возвращаемся к менее исследованным направлениям
        if (UnityEngine.Random.value < explorationBias)
        {
            // Ищем направление, которое меньше всего использовалось
            Vector3 leastUsedDirection = GetLeastUsedDirection(directionHistory);

            // Интерполируем между текущим и наименее используемым направлением
            float blend = UnityEngine.Random.Range(0.3f, 0.7f);
            return Vector3.Slerp(currentDirection, leastUsedDirection, blend).normalized;
        }

        return currentDirection;
    }

    private Vector3 GetLeastUsedDirection(List<Vector3> directionHistory)
    {
        // Разделяем пространство на квадранты
        int[] quadrantCounts = new int[4];

        foreach (var dir in directionHistory)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            int quadrant = Mathf.FloorToInt(angle / 90f);
            quadrantCounts[quadrant % 4]++;
        }

        // Находим наименее используемый квадрант
        int minCount = int.MaxValue;
        int leastUsedQuadrant = 0;
        for (int i = 0; i < 4; i++)
        {
            if (quadrantCounts[i] < minCount)
            {
                minCount = quadrantCounts[i];
                leastUsedQuadrant = i;
            }
        }

        // Возвращаем направление из центра наименее используемого квадранта
        float quadrantAngle = (leastUsedQuadrant * 90f + 45f) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(quadrantAngle), Mathf.Sin(quadrantAngle), 0);
    }

    private Vector3 FindSafePathAroundObstacles(Vector3 from, Vector3 to, Vector3 previousDirection)
    {
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);

        if (IsPathCompletelySafe(from, to))
        {
            return to;
        }

        Vector3 bestDirection = direction;
        float bestScore = float.MaxValue;

        for (int i = 0; i < pathFindingRays; i++)
        {
            float angle = (i / (float)pathFindingRays) * 360f - 180f;
            if (Mathf.Abs(angle) > maxPathDeviation) continue;

            Vector3 testDirection = Quaternion.Euler(0, 0, angle) * direction;
            Vector3 testPoint = from + testDirection * distance;
            testPoint = ClampToBounds(testPoint);

            if (!IsPathCompletelySafe(from, testPoint))
                continue;

            float angleScore = Mathf.Abs(angle) * 0.5f;
            float smoothnessScore = Vector3.Angle(previousDirection, testDirection) * 0.3f;
            float distanceScore = Vector3.Distance(testPoint, to) * 0.2f;

            float totalScore = angleScore + smoothnessScore + distanceScore;

            if (totalScore < bestScore)
            {
                bestScore = totalScore;
                bestDirection = testDirection;
            }
        }

        if (bestScore < float.MaxValue)
        {
            Vector3 result = from + bestDirection * distance;
            debugPathPoints.Add(result);
            return result;
        }

        return FindStagedSafeAvoidance(from, to, previousDirection);
    }

    private Vector3 FindStagedSafeAvoidance(Vector3 from, Vector3 to, Vector3 previousDirection)
    {
        Vector3 currentPos = from;
        float segmentDistance = maxPointDistance * 0.4f;
        List<Transform> obstaclesOnPath = GetObstaclesOnPath(from, to);

        foreach (Transform obstacle in obstaclesOnPath)
        {
            if (obstacle == null) continue;

            Vector3 obstaclePos = obstacle.position;
            Vector3 toObstacle = obstaclePos - currentPos;
            Vector3 directionToTarget = (to - currentPos).normalized;

            Vector3 perpendicular = Vector3.Cross(directionToTarget, Vector3.forward).normalized;
            float rightSafety = CalculateSideSafety(currentPos, obstaclePos, perpendicular, 1);
            float leftSafety = CalculateSideSafety(currentPos, obstaclePos, perpendicular, -1);
            float side = (rightSafety >= leftSafety) ? 1f : -1f;

            float safeDistance = obstacleAvoidanceRadius + obstacleClearance;
            Vector3 avoidancePoint = obstaclePos + perpendicular * side * safeDistance;

            if (IsPathCompletelySafe(currentPos, avoidancePoint))
            {
                Vector3 approachPoint = currentPos + directionToTarget *
                    (Vector3.Distance(currentPos, obstaclePos) - safeDistance * 0.5f);

                if (IsPointSafe(approachPoint) && IsPathCompletelySafe(currentPos, approachPoint))
                {
                    smoothedPath.Add(approachPoint);
                }

                avoidancePoint = ClampToBounds(avoidancePoint);
                smoothedPath.Add(avoidancePoint);
                currentPos = avoidancePoint;

                Vector3 returnDirection = (to - avoidancePoint).normalized;
                Vector3 returnPoint = avoidancePoint + returnDirection * safeDistance;

                if (IsPointSafe(returnPoint) && IsPathCompletelySafe(avoidancePoint, returnPoint))
                {
                    smoothedPath.Add(returnPoint);
                    currentPos = returnPoint;
                }
            }
        }

        if (IsPathCompletelySafe(currentPos, to))
        {
            return to;
        }

        return FindNearestSafePoint(to);
    }

    private float CalculateSideSafety(Vector3 from, Vector3 obstaclePos, Vector3 perpendicular, float side)
    {
        Vector3 testPoint = obstaclePos + perpendicular * side * (obstacleAvoidanceRadius + obstacleClearance);

        if (!IsPathCompletelySafe(from, testPoint))
            return 0f;

        if (!IsPointSafe(testPoint))
            return 0f;

        float minDistance = float.MaxValue;
        foreach (Transform otherObstacle in obstacles)
        {
            if (otherObstacle == null) continue;
            if (otherObstacle.position == obstaclePos) continue;

            float distance = Vector3.Distance(testPoint, otherObstacle.position);
            minDistance = Mathf.Min(minDistance, distance);
        }

        return minDistance;
    }

    private Vector3 FindAlternativeSafePoint(Vector3 from, Vector3 target, Vector3 previousDirection, List<Vector3> directionHistory)
    {
        Vector3 bestPoint = from;
        float bestScore = float.MaxValue;

        int checkDirections = 12;

        for (int i = 0; i < checkDirections; i++)
        {
            float angle = i * (360f / checkDirections);

            float distanceMultiplier = (i % 3 == 0) ? 0.3f :
                             (i % 3 == 1) ? 0.6f : 1.0f;

            Vector3 direction = Quaternion.Euler(0, 0, angle) * previousDirection;
            Vector3 testPoint = from + direction * maxPointDistance * distanceMultiplier;
            testPoint = ClampToBounds(testPoint);

            if (!IsPathCompletelySafe(from, testPoint) || !IsPointSafe(testPoint))
                continue;

            float angleScore = Mathf.Abs(angle) * 0.3f;
            float distanceToTarget = Vector3.Distance(testPoint, target);

            // Поощряем направления, которые добавляют разнообразия
            float diversityBonus = CalculateDiversityBonus(direction, directionHistory);

            float totalScore = angleScore + distanceToTarget * 0.2f - diversityBonus * 10f;

            if (totalScore < bestScore)
            {
                bestScore = totalScore;
                bestPoint = testPoint;
            }
        }

        return bestPoint;
    }

    private float CalculateDiversityBonus(Vector3 direction, List<Vector3> directionHistory)
    {
        if (directionHistory.Count == 0) return 1f;

        float minSimilarity = 1f;
        foreach (var historicDir in directionHistory)
        {
            float similarity = Vector3.Dot(direction.normalized, historicDir.normalized);
            minSimilarity = Mathf.Min(minSimilarity, Mathf.Abs(similarity));
        }

        return 1f - minSimilarity; // Больше бонус за более разные направления
    }

    private Vector3 FindNearestSafePoint(Vector3 target)
    {
        Vector3 bestPoint = target;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < 16; i++)
        {
            float angle = i * 22.5f;
            float radius = obstacleAvoidanceRadius + obstacleClearance;

            for (int r = 1; r <= 3; r++)
            {
                Vector3 testPoint = target + new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius * r,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius * r,
                    0
                );

                testPoint = ClampToBounds(testPoint);

                if (IsPointSafe(testPoint))
                {
                    float distance = Vector3.Distance(target, testPoint);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPoint = testPoint;
                    }
                }
            }
        }

        return bestPoint;
    }

    private bool IsPathCompletelySafe(Vector3 from, Vector3 to)
    {
        int segments = Mathf.CeilToInt(Vector3.Distance(from, to) / (obstacleClearance * 0.5f));
        segments = Mathf.Max(segments, 3);

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = Vector3.Lerp(from, to, t);

            if (!IsPointSafe(point))
                return false;
        }

        return true;
    }

    private bool IsPointSafe(Vector3 point)
    {
        foreach (Transform obstacle in obstacles)
        {
            if (obstacle == null) continue;

            float distance = Vector3.Distance(point, obstacle.position);
            if (distance < obstacleAvoidanceRadius + obstacleClearance)
            {
                return false;
            }
        }
        return true;
    }

    private List<Transform> GetObstaclesOnPath(Vector3 from, Vector3 to)
    {
        List<Transform> result = new List<Transform>();

        foreach (Transform obstacle in obstacles)
        {
            if (obstacle == null) continue;

            Vector3 obstaclePos = obstacle.position;
            float distance = GetDistanceToLine(from, to, obstaclePos);

            if (distance < obstacleAvoidanceRadius + obstacleClearance)
            {
                result.Add(obstacle);
            }
        }

        return result.OrderBy(o => Vector3.Distance(from, o.position)).ToList();
    }

    private float GetDistanceToLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
    {
        Vector3 lineVector = lineEnd - lineStart;
        Vector3 toPoint = point - lineStart;

        float projectionLength = Vector3.Dot(toPoint, lineVector.normalized);
        float clampedProjection = Mathf.Clamp(projectionLength, 0f, lineVector.magnitude);

        Vector3 closestPoint = lineStart + lineVector.normalized * clampedProjection;
        return Vector3.Distance(point, closestPoint);
    }

    private void CheckPathDiversity(List<Vector3> directionHistory)
    {
        if (smoothedPath.Count < 3) return;

        Vector3 startToEnd = smoothedPath[smoothedPath.Count - 1] - smoothedPath[0];
        float pathLength = 0f;
        float straightDistance = startToEnd.magnitude;

        for (int i = 0; i < smoothedPath.Count - 1; i++)
        {
            pathLength += Vector3.Distance(smoothedPath[i], smoothedPath[i + 1]);
        }

        float straightnessRatio = straightDistance / pathLength;

        if (straightnessRatio > 0.8f)
        {
            AddMoreCurvesToPath();
        }

        float diversity = CalculateDirectionDiversity(directionHistory);
        if (diversity < 0.3f)
        {
            EnhancePathDiversity();
        }
    }

    private void AddMoreCurvesToPath()
    {
        if (smoothedPath.Count < 3) return;

        List<Vector3> newPath = new List<Vector3>();
        newPath.Add(smoothedPath[0]);

        for (int i = 1; i < smoothedPath.Count; i++)
        {
            Vector3 prev = smoothedPath[i - 1];
            Vector3 next = smoothedPath[i];
            float distance = Vector3.Distance(prev, next);

            if (distance > minPointDistance * 1.5f)
            {
                Vector3 midPoint = (prev + next) * 0.5f;
                Vector3 direction = (next - prev).normalized;
                Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);

                float side = UnityEngine.Random.Range(-1f, 1f);
                float offsetAmount = distance * 0.3f * side;

                Vector3 curvedPoint = midPoint + perpendicular * offsetAmount;

                if (IsPointSafe(curvedPoint) &&
                    IsPathCompletelySafe(prev, curvedPoint) &&
                    IsPathCompletelySafe(curvedPoint, next))
                {
                    newPath.Add(curvedPoint);
                }
            }

            newPath.Add(next);
        }

        smoothedPath = newPath;
    }

    private void EnhancePathDiversity()
    {
        if (smoothedPath.Count < 3) return;

        for (int i = 1; i < smoothedPath.Count - 1; i++)
        {
            Vector3 prev = smoothedPath[i - 1];
            Vector3 current = smoothedPath[i];
            Vector3 next = smoothedPath[i + 1];

            Vector3 dirToPrev = (current - prev).normalized;
            Vector3 dirToNext = (next - current).normalized;

            float dotProduct = Vector3.Dot(dirToPrev, dirToNext);

            // Если направление слишком похоже, добавляем изгиб
            if (dotProduct > 0.8f && UnityEngine.Random.value < 0.5f)
            {
                Vector3 perpendicular = new Vector3(-dirToPrev.y, dirToPrev.x, 0);
                float side = UnityEngine.Random.Range(-1f, 1f);
                Vector3 adjustedPoint = current + perpendicular * minPointDistance * 0.5f * side;

                adjustedPoint = ClampToBounds(adjustedPoint);

                if (IsPointSafe(adjustedPoint) &&
                    IsPathCompletelySafe(prev, adjustedPoint) &&
                    IsPathCompletelySafe(adjustedPoint, next))
                {
                    smoothedPath[i] = adjustedPoint;
                }
            }
        }
    }

    private void CreateSmoothSplineFromPath()
    {
        spline.Clear();

        if (smoothedPath.Count < 2) return;

        for (int i = 0; i < smoothedPath.Count; i++)
        {
            Vector3 position = smoothedPath[i];

            float3 tangentIn = float3.zero;
            float3 tangentOut = float3.zero;

            if (i > 0 && i < smoothedPath.Count - 1)
            {
                Vector3 prev = smoothedPath[i - 1];
                Vector3 next = smoothedPath[i + 1];

                Vector3 dirToPrev = (position - prev).normalized;
                Vector3 dirToNext = (next - position).normalized;

                Vector3 smoothDirection = (dirToPrev + dirToNext).normalized;

                float distanceToPrev = Vector3.Distance(position, prev);
                float distanceToNext = Vector3.Distance(next, position);
                float tangentLength = Mathf.Min(distanceToPrev, distanceToNext) * tangentLengthMultiplier;

                tangentIn = new float3(-smoothDirection.x * tangentLength,
                                     -smoothDirection.y * tangentLength,
                                     -smoothDirection.z * tangentLength);

                tangentOut = new float3(smoothDirection.x * tangentLength,
                                      smoothDirection.y * tangentLength,
                                      smoothDirection.z * tangentLength);
            }

            BezierKnot knot = new BezierKnot(new float3(position.x, position.y, position.z));
            knot.TangentIn = tangentIn;
            knot.TangentOut = tangentOut;

            spline.Add(knot);
        }

        ApplyCurveSmoothing();
    }

    private void ApplyCurveSmoothing()
    {
        if (spline.Count < 3) return;

        for (int i = 1; i < spline.Count - 1; i++)
        {
            var knot = spline[i];
            var prevPos = spline[i - 1].Position;
            var nextPos = spline[i + 1].Position;

            float3 toPrev = prevPos - knot.Position;
            float3 toNext = nextPos - knot.Position;

            float3 dirToPrev = toPrev;
            float lengthPrev = math.sqrt(dirToPrev.x * dirToPrev.x + dirToPrev.y * dirToPrev.y + dirToPrev.z * dirToPrev.z);
            if (lengthPrev > 0) dirToPrev = dirToPrev / lengthPrev;

            float3 dirToNext = toNext;
            float lengthNext = math.sqrt(dirToNext.x * dirToNext.x + dirToNext.y * dirToNext.y + dirToNext.z * dirToNext.z);
            if (lengthNext > 0) dirToNext = dirToNext / lengthNext;

            float3 smoothDirection = dirToPrev + dirToNext;
            float smoothLength = math.sqrt(smoothDirection.x * smoothDirection.x +
                                         smoothDirection.y * smoothDirection.y +
                                         smoothDirection.z * smoothDirection.z);

            if (smoothLength > 0)
            {
                smoothDirection = smoothDirection / smoothLength;
            }

            float currentInLength = math.sqrt(knot.TangentIn.x * knot.TangentIn.x +
                                            knot.TangentIn.y * knot.TangentIn.y +
                                            knot.TangentIn.z * knot.TangentIn.z);

            float currentOutLength = math.sqrt(knot.TangentOut.x * knot.TangentOut.x +
                                             knot.TangentOut.y * knot.TangentOut.y +
                                             knot.TangentOut.z * knot.TangentOut.z);

            knot.TangentIn = -smoothDirection * currentInLength * curveSmoothness;
            knot.TangentOut = smoothDirection * currentOutLength * curveSmoothness;

            spline[i] = knot;
        }
    }

    private float CalculateSplineLength()
    {
        float length = 0f;
        int segments = Mathf.Min(100, spline.Count * 10);

        Vector3 prevPoint = new Vector3(spline.EvaluatePosition(0).x, spline.EvaluatePosition(0).y, spline.EvaluatePosition(0).z);

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            float3 splinePos = spline.EvaluatePosition(t);
            Vector3 nextPoint = new Vector3(splinePos.x, splinePos.y, splinePos.z);

            length += Vector3.Distance(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        return length;
    }

    private void MoveAlongSpline()
    {
        if (splineLength <= 0 || spline.Count < 2)
        {
            Debug.LogWarning("Некорректный сплайн. Регенерирую путь...");
            GenerateRandomPath();
            return;
        }

        float distanceToMove = moveSpeed * Time.deltaTime;
        currentSplinePosition += distanceToMove / splineLength;
        currentSplinePosition = Mathf.Clamp01(currentSplinePosition);

        float3 splinePos = spline.EvaluatePosition(currentSplinePosition);
        Vector3 targetPosition = new Vector3(splinePos.x, splinePos.y, splinePos.z);

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (currentSplinePosition >= 0.99f)
        {
            OnPathCompleted();
        }
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        return new Vector3(
            Mathf.Clamp(position.x, boundsMin.x, boundsMax.x),
            Mathf.Clamp(position.y, boundsMin.y, boundsMax.y),
            0
        );
    }

    public void StartMoving()
    {
        if (spline.Count > 1 && splineLength > 0)
        {
            isMoving = true;
            currentSplinePosition = 0f;
            stuckTimer = 0f;
        }
        else
        {
            GenerateRandomPath();
        }
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    private void OnPathCompleted()
    {
        GenerateRandomPath();
        StartMoving();
    }

    public void AddObstacle(Transform obstacle)
    {
        if (!obstacles.Contains(obstacle))
        {
            obstacles.Add(obstacle);
        }
    }

    public void RemoveObstacle(Transform obstacle)
    {
        obstacles.Remove(obstacle);
    }

    public void ClearObstacles()
    {
        obstacles.Clear();
    }

    public bool IsMoving => isMoving;
    public bool IsPathGenerated => isPathGenerated;
    public float CurrentProgress => currentSplinePosition;
    public float SplineLength => splineLength;

    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        Gizmos.color = Color.gray;
        Vector3 center = new Vector3(
            (boundsMin.x + boundsMax.x) / 2,
            (boundsMin.y + boundsMax.y) / 2,
            0
        );
        Vector3 size = new Vector3(
            boundsMax.x - boundsMin.x,
            boundsMax.y - boundsMin.y,
            0.1f
        );
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = new Color(1, 0.5f, 0.5f, 0.3f);
        foreach (Transform obstacle in obstacles)
        {
            if (obstacle != null)
            {
                Gizmos.DrawSphere(obstacle.position, obstacleAvoidanceRadius + obstacleClearance);
            }
        }

        Gizmos.color = Color.blue;
        foreach (Vector3 point in debugPathPoints)
        {
            Gizmos.DrawSphere(point, 0.3f);
        }

        if (smoothedPath.Count > 1)
        {
            for (int i = 0; i < smoothedPath.Count - 1; i++)
            {
                bool safe = IsPathCompletelySafe(smoothedPath[i], smoothedPath[i + 1]);
                Gizmos.color = safe ? safePathColor : obstaclePathColor;
                Gizmos.DrawLine(smoothedPath[i], smoothedPath[i + 1]);
                Gizmos.DrawSphere(smoothedPath[i], 0.2f);
            }
            if (smoothedPath.Count > 0)
                Gizmos.DrawSphere(smoothedPath[smoothedPath.Count - 1], 0.2f);
        }

        if (smoothedPath.Count > 1)
        {
            Gizmos.color = directionColor;
            for (int i = 1; i < smoothedPath.Count; i++)
            {
                Vector3 start = smoothedPath[i - 1];
                Vector3 end = smoothedPath[i];
                Vector3 direction = (end - start).normalized;
                Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);

                Vector3 arrowStart = (start + end) * 0.5f;
                Vector3 arrowEnd = arrowStart + direction * 0.5f;
                Gizmos.DrawLine(arrowStart, arrowEnd);

                Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.2f + perpendicular * 0.1f);
                Gizmos.DrawLine(arrowEnd, arrowEnd - direction * 0.2f - perpendicular * 0.1f);
            }
        }

        if (Application.isPlaying && smoothedPath.Count > 1)
        {
            Gizmos.color = Color.magenta;
            int currentIndex = Mathf.FloorToInt(currentSplinePosition * (smoothedPath.Count - 1));
            currentIndex = Mathf.Clamp(currentIndex, 0, smoothedPath.Count - 2);

            Vector3 currentDir = (smoothedPath[currentIndex + 1] - smoothedPath[currentIndex]).normalized;
            Gizmos.DrawRay(transform.position, currentDir * 2f);
        }

        if (Application.isPlaying && spline.Count > 0)
        {
            Gizmos.color = Color.yellow;
            int segments = 50;
            float3 prevPoint = spline.EvaluatePosition(0);

            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                float3 nextPoint = spline.EvaluatePosition(t);
                Gizmos.DrawLine(
                    new Vector3(prevPoint.x, prevPoint.y, prevPoint.z),
                    new Vector3(nextPoint.x, nextPoint.y, nextPoint.z)
                );
                prevPoint = nextPoint;
            }

            Gizmos.color = Color.magenta;
            float3 currentPos = spline.EvaluatePosition(currentSplinePosition);
            Gizmos.DrawSphere(new Vector3(currentPos.x, currentPos.y, currentPos.z), 0.5f);
        }
    }
}