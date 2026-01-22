
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemiesData", menuName = "Game Data/Enemies Data")]
public class EntitiesDataSO : ScriptableObject
{
    [field: SerializeField] public EnemyTableRow[] EnemyRows { get; private set; }

    
}

[Serializable]
public class EnemyTableRow : IEntity, IEnemyAIConfig
{
    [field: SerializeField] public int ID { get; set; }
    [field: SerializeField] public string CharacterName { get; set; }
    [field: SerializeField] public GameObject EntityPrefab { get; set; }
    [field: SerializeField] public EntityType EntityType { get; set; }
    [field: SerializeField] public Vector2 MovementSpeed { get; set; }
    [field: SerializeField] public Vector2 AttackSpeed { get; set; }
    [field: SerializeField] public Vector2 ShootSpeed { get; set; }
    [field: SerializeField] public int ShootDamage { get; set; }
    [field: SerializeField] public int MeleeDamage { get; set; }
    [field: SerializeField] public int Health { get; set; }
    [field: SerializeField] public float DetectionRadius { get; set; } = 6f;
    [field: SerializeField] public float AttackRange { get; set; } = 2f;
    [field: SerializeField] public float ShootRange { get; set; } = 5f;
    [field: SerializeField][field: Range(0, 1)] public float Armor { get; set; }


    // === Разделение перед настройками ИИ ===
    [Space(15)]
    [Header("********************************************")]
    [Space(2)]
    [Header("Настройки ИИ поведения")]
    [Space(2)]
    
    // === Параметры ИИ ===
    [Header("Основные настройки")]
    [SerializeField] private float pursuitDistance = 10f;
    [SerializeField] private float speed = 4f;
    [SerializeField] private float avoidanceStrength = 15f;
    [SerializeField] private float avoidanceDistance = 3f;

    [Header("Параметры MeleeAttack")]
    [SerializeField] private float attackDistance = 1.5f;
    [SerializeField] private float retreatDistance = 4f;
    [SerializeField] private float minPauseTime = 1f;
    [SerializeField] private float maxPauseTime = 3f;

    [Header("Общие параметры патруля (используются в RangedPatrol и в Melee после отхода)")]
    [SerializeField] private float patrolVariation = 1.5f;
    [SerializeField] private float tangentialSpeed = 2f;
    [SerializeField] private float changeDirectionMin = 1.5f;
    [SerializeField] private float changeDirectionMax = 4f;

    [Header("Параметры RangedPatrol")]
    [SerializeField] private float safeDistance = 3f;

    [Header("Параметры Flee")]
    [SerializeField] private float fleeSpeedMultiplier = 1.5f;

    // Реализация IEnemyAIConfig (read-only доступ для скриптов ИИ)
    public float PursuitDistance { get => pursuitDistance; set => pursuitDistance = value; }
    public float Speed { get => speed; set => speed = value; }
    public float AvoidanceStrength { get => avoidanceStrength; set => avoidanceStrength = value; }
    public float AvoidanceDistance { get => avoidanceDistance; set => avoidanceDistance = value; }
    public float AttackDistance { get => attackDistance; set => attackDistance = value; }
    public float RetreatDistance { get => retreatDistance; set => retreatDistance = value; }
    public float MinPauseTime { get => minPauseTime; set => minPauseTime = value; }
    public float MaxPauseTime { get => maxPauseTime; set => maxPauseTime = value; }
    public float PatrolVariation { get => patrolVariation; set => patrolVariation = value; }
    public float TangentialSpeed { get => tangentialSpeed; set => tangentialSpeed = value; }
    public float ChangeDirectionMin { get => changeDirectionMin; set => changeDirectionMin = value; }
    public float ChangeDirectionMax { get => changeDirectionMax; set => changeDirectionMax = value; }
    public float SafeDistance { get => safeDistance; set => safeDistance = value; }
    public float FleeSpeedMultiplier { get => fleeSpeedMultiplier; set => fleeSpeedMultiplier = value; }


}
#if UNITY_EDITOR

[CustomEditor(typeof(EntitiesDataSO))]
public class EntitiesDataSOEditor : Editor
{
    private bool[] fieldSelected = new bool[27];
    private Vector2 movementSpeed = new Vector2(3, 5);
    private Vector2 attackSpeed = new Vector2(0.5f, 1.5f);
    private Vector2 shootSpeed = new Vector2(0.3f, 1f);
    private int shootDamage = 10;
    private int meleeDamage = 20;
    private int health = 100;
    private float detectionRadius = 6f;
    private float attackRange = 2f;
    private float shootRange = 5f;
    private float armor = 0.5f;
    private float pursuitDistance = 10f;
    private float speed = 4f;
    private float avoidanceStrength = 15f;
    private float avoidanceDistance = 3f;
    private float attackDistance = 1.5f;
    private float retreatDistance = 4f;
    private float minPauseTime = 1f;
    private float maxPauseTime = 3f;
    private float patrolVariation = 1.5f;
    private float tangentialSpeed = 2f;
    private float changeDirectionMin = 1.5f;
    private float changeDirectionMax = 4f;
    private float safeDistance = 3f;
    private float fleeSpeedMultiplier = 1.5f;

    public override void OnInspectorGUI()
    {
        EntitiesDataSO data = (EntitiesDataSO)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("=== Массовое копирование ===", EditorStyles.boldLabel);

        // Кнопка для копирования из первого элемента
        if (data.EnemyRows != null && data.EnemyRows.Length > 0)
        {
            if (GUILayout.Button("ЗАГРУЗИТЬ ИЗ ПЕРВОГО ВРАГА", GUILayout.Height(25)))
            {
                LoadFromFirstEnemy(data);
            }
        }

        EditorGUILayout.Space(10);

        // Управление галочками
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Выбрать все", GUILayout.Width(100)))
        {
            SelectAllFields(true);
        }
        if (GUILayout.Button("Снять все", GUILayout.Width(100)))
        {
            SelectAllFields(false);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Редактируем шаблон с галочками
        EditorGUILayout.LabelField("Выберите поля для копирования:", EditorStyles.boldLabel);

        // Боевые параметры
        EditorGUILayout.LabelField("Боевые характеристики", EditorStyles.boldLabel);
        fieldSelected[0] = DrawFieldWithToggle(fieldSelected[0], "Movement Speed", () => movementSpeed, v => movementSpeed = v);
        fieldSelected[1] = DrawFieldWithToggle(fieldSelected[1], "Attack Speed", () => attackSpeed, v => attackSpeed = v);
        fieldSelected[2] = DrawFieldWithToggle(fieldSelected[2], "Shoot Speed", () => shootSpeed, v => shootSpeed = v);
        fieldSelected[3] = DrawFieldWithToggle(fieldSelected[3], "Shoot Damage", () => shootDamage, v => shootDamage = v);
        fieldSelected[4] = DrawFieldWithToggle(fieldSelected[4], "Melee Damage", () => meleeDamage, v => meleeDamage = v);
        fieldSelected[5] = DrawFieldWithToggle(fieldSelected[5], "Health", () => health, v => health = v);
        fieldSelected[6] = DrawFieldWithToggle(fieldSelected[6], "Detection Radius", () => detectionRadius, v => detectionRadius = v);
        fieldSelected[7] = DrawFieldWithToggle(fieldSelected[7], "Attack Range", () => attackRange, v => attackRange = v);
        fieldSelected[8] = DrawFieldWithToggle(fieldSelected[8], "Shoot Range", () => shootRange, v => shootRange = v);
        fieldSelected[9] = DrawFieldWithToggleSlider(fieldSelected[9], "Armor", () => armor, v => armor = v, 0, 1);

        EditorGUILayout.Space(10);

        // Параметры ИИ
        EditorGUILayout.LabelField("Настройки ИИ", EditorStyles.boldLabel);
        fieldSelected[10] = DrawFieldWithToggle(fieldSelected[10], "Pursuit Distance", () => pursuitDistance, v => pursuitDistance = v);
        fieldSelected[11] = DrawFieldWithToggle(fieldSelected[11], "Speed", () => speed, v => speed = v);
        fieldSelected[12] = DrawFieldWithToggle(fieldSelected[12], "Avoidance Strength", () => avoidanceStrength, v => avoidanceStrength = v);
        fieldSelected[13] = DrawFieldWithToggle(fieldSelected[13], "Avoidance Distance", () => avoidanceDistance, v => avoidanceDistance = v);
        fieldSelected[14] = DrawFieldWithToggle(fieldSelected[14], "Attack Distance", () => attackDistance, v => attackDistance = v);
        fieldSelected[15] = DrawFieldWithToggle(fieldSelected[15], "Retreat Distance", () => retreatDistance, v => retreatDistance = v);
        fieldSelected[16] = DrawFieldWithToggle(fieldSelected[16], "Min Pause Time", () => minPauseTime, v => minPauseTime = v);
        fieldSelected[17] = DrawFieldWithToggle(fieldSelected[17], "Max Pause Time", () => maxPauseTime, v => maxPauseTime = v);
        fieldSelected[18] = DrawFieldWithToggle(fieldSelected[18], "Patrol Variation", () => patrolVariation, v => patrolVariation = v);
        fieldSelected[19] = DrawFieldWithToggle(fieldSelected[19], "Tangential Speed", () => tangentialSpeed, v => tangentialSpeed = v);
        fieldSelected[20] = DrawFieldWithToggle(fieldSelected[20], "Change Direction Min", () => changeDirectionMin, v => changeDirectionMin = v);
        fieldSelected[21] = DrawFieldWithToggle(fieldSelected[21], "Change Direction Max", () => changeDirectionMax, v => changeDirectionMax = v);
        fieldSelected[22] = DrawFieldWithToggle(fieldSelected[22], "Safe Distance", () => safeDistance, v => safeDistance = v);
        fieldSelected[23] = DrawFieldWithToggle(fieldSelected[23], "Flee Speed Multiplier", () => fleeSpeedMultiplier, v => fleeSpeedMultiplier = v);

        EditorGUILayout.Space(10);

        // Кнопка применения
        if (GUILayout.Button("СКОПИРОВАТЬ ВЫБРАННЫЕ ПОЛЯ ВО ВСЕХ ВРАГОВ", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Копирование",
                $"Скопировать выбранные поля во всех {data.EnemyRows?.Length ?? 0} врагов?",
                "Да", "Нет"))
            {
                ApplySelectedFieldsToAll(data);
            }
        }

        EditorGUILayout.Space(20);

        // Стандартный инспектор
        DrawDefaultInspector();
    }

    private bool DrawFieldWithToggle(bool selected, string label,
        System.Func<Vector2> getter, System.Action<Vector2> setter)
    {
        EditorGUILayout.BeginHorizontal();
        selected = EditorGUILayout.Toggle(selected, GUILayout.Width(20));

        EditorGUI.BeginDisabledGroup(!selected);
        Vector2 value = getter();
        value = EditorGUILayout.Vector2Field(label, value);
        setter(value);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        return selected;
    }

    private bool DrawFieldWithToggle(bool selected, string label,
        System.Func<int> getter, System.Action<int> setter)
    {
        EditorGUILayout.BeginHorizontal();
        selected = EditorGUILayout.Toggle(selected, GUILayout.Width(20));

        EditorGUI.BeginDisabledGroup(!selected);
        int value = getter();
        value = EditorGUILayout.IntField(label, value);
        setter(value);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        return selected;
    }

    private bool DrawFieldWithToggle(bool selected, string label,
        System.Func<float> getter, System.Action<float> setter)
    {
        EditorGUILayout.BeginHorizontal();
        selected = EditorGUILayout.Toggle(selected, GUILayout.Width(20));

        EditorGUI.BeginDisabledGroup(!selected);
        float value = getter();
        value = EditorGUILayout.FloatField(label, value);
        setter(value);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        return selected;
    }

    private bool DrawFieldWithToggleSlider(bool selected, string label,
        System.Func<float> getter, System.Action<float> setter, float min, float max)
    {
        EditorGUILayout.BeginHorizontal();
        selected = EditorGUILayout.Toggle(selected, GUILayout.Width(20));

        EditorGUI.BeginDisabledGroup(!selected);
        float value = getter();
        value = EditorGUILayout.Slider(label, value, min, max);
        setter(value);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        return selected;
    }

    private void LoadFromFirstEnemy(EntitiesDataSO data)
    {
        var firstEnemy = data.EnemyRows[0];

        movementSpeed = firstEnemy.MovementSpeed;
        attackSpeed = firstEnemy.AttackSpeed;
        shootSpeed = firstEnemy.ShootSpeed;
        shootDamage = firstEnemy.ShootDamage;
        meleeDamage = firstEnemy.MeleeDamage;
        health = firstEnemy.Health;
        detectionRadius = firstEnemy.DetectionRadius;
        attackRange = firstEnemy.AttackRange;
        shootRange = firstEnemy.ShootRange;
        armor = firstEnemy.Armor;

        // Поля ИИ
        pursuitDistance = firstEnemy.PursuitDistance;
        speed = firstEnemy.Speed;
        avoidanceStrength = firstEnemy.AvoidanceStrength;
        avoidanceDistance = firstEnemy.AvoidanceDistance;
        attackDistance = firstEnemy.AttackDistance;
        retreatDistance = firstEnemy.RetreatDistance;
        minPauseTime = firstEnemy.MinPauseTime;
        maxPauseTime = firstEnemy.MaxPauseTime;
        patrolVariation = firstEnemy.PatrolVariation;
        tangentialSpeed = firstEnemy.TangentialSpeed;
        changeDirectionMin = firstEnemy.ChangeDirectionMin;
        changeDirectionMax = firstEnemy.ChangeDirectionMax;
        safeDistance = firstEnemy.SafeDistance;
        fleeSpeedMultiplier = firstEnemy.FleeSpeedMultiplier;

        // Автоматически выбираем все поля при загрузке
        SelectAllFields(true);

        Debug.Log("Значения загружены из первого врага");
    }

    private void ApplySelectedFieldsToAll(EntitiesDataSO data)
    {
        if (data.EnemyRows == null || data.EnemyRows.Length == 0)
            return;

        Undo.RecordObject(data, "Copy selected fields to all enemies");

        int appliedCount = 0;

        for (int i = 0; i < data.EnemyRows.Length; i++)
        {
            if (data.EnemyRows[i] == null) continue;

            // Копируем только выбранные поля
            if (fieldSelected[0]) data.EnemyRows[i].MovementSpeed = movementSpeed;
            if (fieldSelected[1]) data.EnemyRows[i].AttackSpeed = attackSpeed;
            if (fieldSelected[2]) data.EnemyRows[i].ShootSpeed = shootSpeed;
            if (fieldSelected[3]) data.EnemyRows[i].ShootDamage = shootDamage;
            if (fieldSelected[4]) data.EnemyRows[i].MeleeDamage = meleeDamage;
            if (fieldSelected[5]) data.EnemyRows[i].Health = health;
            if (fieldSelected[6]) data.EnemyRows[i].DetectionRadius = detectionRadius;
            if (fieldSelected[7]) data.EnemyRows[i].AttackRange = attackRange;
            if (fieldSelected[8]) data.EnemyRows[i].ShootRange = shootRange;
            if (fieldSelected[9]) data.EnemyRows[i].Armor = armor;

            // Поля ИИ
            if (fieldSelected[10]) data.EnemyRows[i].PursuitDistance = pursuitDistance;
            if (fieldSelected[11]) data.EnemyRows[i].Speed = speed;
            if (fieldSelected[12]) data.EnemyRows[i].AvoidanceStrength = avoidanceStrength;
            if (fieldSelected[13]) data.EnemyRows[i].AvoidanceDistance = avoidanceDistance;
            if (fieldSelected[14]) data.EnemyRows[i].AttackDistance = attackDistance;
            if (fieldSelected[15]) data.EnemyRows[i].RetreatDistance = retreatDistance;
            if (fieldSelected[16]) data.EnemyRows[i].MinPauseTime = minPauseTime;
            if (fieldSelected[17]) data.EnemyRows[i].MaxPauseTime = maxPauseTime;
            if (fieldSelected[18]) data.EnemyRows[i].PatrolVariation = patrolVariation;
            if (fieldSelected[19]) data.EnemyRows[i].TangentialSpeed = tangentialSpeed;
            if (fieldSelected[20]) data.EnemyRows[i].ChangeDirectionMin = changeDirectionMin;
            if (fieldSelected[21]) data.EnemyRows[i].ChangeDirectionMax = changeDirectionMax;
            if (fieldSelected[22]) data.EnemyRows[i].SafeDistance = safeDistance;
            if (fieldSelected[23]) data.EnemyRows[i].FleeSpeedMultiplier = fleeSpeedMultiplier;
        }

        // Считаем сколько полей применили
        for (int i = 0; i < fieldSelected.Length; i++)
        {
            if (fieldSelected[i]) appliedCount++;
        }

        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();

        Debug.Log($"Применено {appliedCount} полей ко всем {data.EnemyRows.Length} врагам");
    }

    private void SelectAllFields(bool select)
    {
        for (int i = 0; i < fieldSelected.Length; i++)
        {
            fieldSelected[i] = select;
        }
    }
}
#endif