#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(EnemiesDataSO))]
public class EnemiesDataSOEdit : Editor
{
    private EnemiesDataSO _enemiesData;
    private Vector2 _scrollPosition;
    private string[] _columnHeaders = new string[]
    {
        "Персонаж",
        "Скорость движения, м/с",
        "Скорость удара в сек",
        "Урон ударом",
        "Скорость выстрела в сек",
        "Урон выстрелом",
        "Здоровье",
        "Броня (снижение урона,%)"
    };

    private float[] _columnWidths = new float[]
    {
        120,  // Персонаж
        160,  // Скорость движения
        140,  // Скорость удара
        100,  // Урон ударом
        150,  // Скорость выстрела
        110,  // Урон выстрелом
        90,   // Здоровье
        150   // Броня
    };

    private void OnEnable()
    {
        _enemiesData = target as EnemiesDataSO;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("ТАБЛИЦА ДАННЫХ ВРАГОВ", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox(
            "ПРИМ: В СОСТОЯНИИ IDLE У ВСЕХ ЗОМБИ СКОРОСТЬ 0,1-0,25\n" +
            "Для диапазонов скорости используйте Vector2 (x=мин, y=макс)",
            MessageType.Info
        );

        EditorGUILayout.Space(10);

        // Кнопки управления таблицей
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Добавить строку", GUILayout.Height(25)))
        {
            AddNewRow();
        }

        if (GUILayout.Button("▼ Добавить 5 пустых строк", GUILayout.Height(25)))
        {
            AddMultipleRows(5);
        }

        if (GUILayout.Button("✕ Очистить таблицу", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("Очистить таблицу",
                "Вы уверены, что хотите удалить все данные из таблицы?", "Да", "Нет"))
            {
                ClearTable();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(15);

        // Начало таблицы
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

        // Заголовки таблицы
        DrawTableHeader();

        // Строки таблицы
        DrawTableRows();

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);

        // Кнопки предустановленных данных
        EditorGUILayout.LabelField("Предустановленные данные:", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Загрузить пример из ТЗ", GUILayout.Height(30)))
        {
            LoadExampleData();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTableHeader()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        for (int i = 0; i < _columnHeaders.Length; i++)
        {
            EditorGUILayout.LabelField(_columnHeaders[i],
                EditorStyles.boldLabel,
                GUILayout.Width(_columnWidths[i]));
        }

        // Колонка для кнопки удаления
        EditorGUILayout.LabelField("Действия", EditorStyles.boldLabel, GUILayout.Width(80));

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
    }

    private void DrawTableRows()
    {
        SerializedProperty rowsProperty = serializedObject.FindProperty("_enemyRows");

        if (rowsProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("Таблица пуста. Нажмите '+ Добавить строку' для начала.",
                MessageType.Warning);
            return;
        }

        for (int i = 0; i < rowsProperty.arraySize; i++)
        {
            EditorGUILayout.BeginHorizontal();

            SerializedProperty rowProperty = rowsProperty.GetArrayElementAtIndex(i);

            // Колонка 1: Персонаж
            SerializedProperty characterProp = rowProperty.FindPropertyRelative("_character");
            string character = EditorGUILayout.TextField(characterProp.stringValue,
                GUILayout.Width(_columnWidths[0]));
            characterProp.stringValue = character;

            // Колонка 2: Скорость движения (диапазон)
            SerializedProperty movementSpeedProp = rowProperty.FindPropertyRelative("_movementSpeed");
            Vector2 movementSpeed = EditorGUILayout.Vector2Field("", movementSpeedProp.vector2Value,
                GUILayout.Width(_columnWidths[1]));
            movementSpeedProp.vector2Value = movementSpeed;

            // Колонка 3: Скорость удара (диапазон)
            SerializedProperty attackSpeedProp = rowProperty.FindPropertyRelative("_attackSpeed");
            Vector2 attackSpeed = EditorGUILayout.Vector2Field("", attackSpeedProp.vector2Value,
                GUILayout.Width(_columnWidths[2]));
            attackSpeedProp.vector2Value = attackSpeed;

            // Колонка 4: Урон ударом
            SerializedProperty meleeDamageProp = rowProperty.FindPropertyRelative("_meleeDamage");
            int meleeDamage = EditorGUILayout.IntField(meleeDamageProp.intValue,
                GUILayout.Width(_columnWidths[3]));
            meleeDamageProp.intValue = meleeDamage;

            // Колонка 5: Скорость выстрела (диапазон)
            SerializedProperty shootSpeedProp = rowProperty.FindPropertyRelative("_shootSpeed");
            Vector2 shootSpeed = EditorGUILayout.Vector2Field("", shootSpeedProp.vector2Value,
                GUILayout.Width(_columnWidths[4]));
            shootSpeedProp.vector2Value = shootSpeed;

            // Колонка 6: Урон выстрелом
            SerializedProperty shootDamageProp = rowProperty.FindPropertyRelative("_shootDamage");
            int shootDamage = EditorGUILayout.IntField(shootDamageProp.intValue,
                GUILayout.Width(_columnWidths[5]));
            shootDamageProp.intValue = shootDamage;

            // Колонка 7: Здоровье
            SerializedProperty healthProp = rowProperty.FindPropertyRelative("_health");
            int health = EditorGUILayout.IntField(healthProp.intValue,
                GUILayout.Width(_columnWidths[6]));
            healthProp.intValue = health;

            // Колонка 8: Броня
            SerializedProperty armorProp = rowProperty.FindPropertyRelative("_armor");
            int armor = EditorGUILayout.IntSlider(armorProp.intValue, 0, 100,
                GUILayout.Width(_columnWidths[7]));
            armorProp.intValue = armor;

            // Колонка действий
            EditorGUILayout.BeginVertical(GUILayout.Width(80));

            if (GUILayout.Button("Копировать", GUILayout.Height(20)))
            {
                DuplicateRow(i);
            }

            if (GUILayout.Button("Удалить", GUILayout.Height(20)))
            {
                DeleteRow(i);
                return; // Выходим, так как массив изменился
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            // Разделитель между строками
            if (i < rowsProperty.arraySize - 1)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.Space(2);
            }
        }
    }

    private void AddNewRow()
    {
        SerializedProperty rowsProperty = serializedObject.FindProperty("_enemyRows");
        rowsProperty.arraySize++;
        serializedObject.ApplyModifiedProperties();
    }

    private void AddMultipleRows(int count)
    {
        SerializedProperty rowsProperty = serializedObject.FindProperty("_enemyRows");
        int currentSize = rowsProperty.arraySize;
        rowsProperty.arraySize = currentSize + count;
        serializedObject.ApplyModifiedProperties();
    }

    private void DeleteRow(int index)
    {
        SerializedProperty rowsProperty = serializedObject.FindProperty("_enemyRows");
        if (index >= 0 && index < rowsProperty.arraySize)
        {
            rowsProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DuplicateRow(int index)
    {
        SerializedProperty rowsProperty = serializedObject.FindProperty("_enemyRows");
        if (index >= 0 && index < rowsProperty.arraySize)
        {
            rowsProperty.InsertArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void ClearTable()
    {
        SerializedProperty rowsProperty = serializedObject.FindProperty("_enemyRows");
        rowsProperty.ClearArray();
        serializedObject.ApplyModifiedProperties();
    }

    private void LoadExampleData()
    {
        SerializedProperty rowsProperty = serializedObject.FindProperty("_enemyRows");
        rowsProperty.ClearArray();

        // Кот базовый
        rowsProperty.arraySize = 6;

        // Строка 0: Кот базовый
        var row0 = rowsProperty.GetArrayElementAtIndex(0);
        row0.FindPropertyRelative("_character").stringValue = "Кот базовый";
        row0.FindPropertyRelative("_movementSpeed").vector2Value = new Vector2(1, 1);
        row0.FindPropertyRelative("_attackSpeed").vector2Value = Vector2.zero;
        row0.FindPropertyRelative("_meleeDamage").intValue = 0;
        row0.FindPropertyRelative("_shootSpeed").vector2Value = new Vector2(1, 1);
        row0.FindPropertyRelative("_shootDamage").intValue = 10;
        row0.FindPropertyRelative("_health").intValue = 200;
        row0.FindPropertyRelative("_armor").intValue = 0;

        // Строка 1: Зомби обычный
        var row1 = rowsProperty.GetArrayElementAtIndex(1);
        row1.FindPropertyRelative("_character").stringValue = "Зомби обычный";
        row1.FindPropertyRelative("_movementSpeed").vector2Value = new Vector2(0.4f, 0.7f);
        row1.FindPropertyRelative("_attackSpeed").vector2Value = new Vector2(1, 1);
        row1.FindPropertyRelative("_meleeDamage").intValue = 20;
        row1.FindPropertyRelative("_shootSpeed").vector2Value = Vector2.zero;
        row1.FindPropertyRelative("_shootDamage").intValue = 0;
        row1.FindPropertyRelative("_health").intValue = 40;
        row1.FindPropertyRelative("_armor").intValue = 0;

        // Строка 2: Зомби ползущий
        var row2 = rowsProperty.GetArrayElementAtIndex(2);
        row2.FindPropertyRelative("_character").stringValue = "Зомби ползущий";
        row2.FindPropertyRelative("_movementSpeed").vector2Value = new Vector2(0.25f, 0.4f);
        row2.FindPropertyRelative("_attackSpeed").vector2Value = new Vector2(0.4f, 0.7f);
        row2.FindPropertyRelative("_meleeDamage").intValue = 10;
        row2.FindPropertyRelative("_shootSpeed").vector2Value = Vector2.zero;
        row2.FindPropertyRelative("_shootDamage").intValue = 0;
        row2.FindPropertyRelative("_health").intValue = 20;
        row2.FindPropertyRelative("_armor").intValue = 0;

        // Строка 3: Зомби-камикадзе
        var row3 = rowsProperty.GetArrayElementAtIndex(3);
        row3.FindPropertyRelative("_character").stringValue = "Зомби-камикадзе";
        row3.FindPropertyRelative("_movementSpeed").vector2Value = new Vector2(0.3f, 3.0f);
        row3.FindPropertyRelative("_attackSpeed").vector2Value = new Vector2(1, 1);
        row3.FindPropertyRelative("_meleeDamage").intValue = 60;
        row3.FindPropertyRelative("_shootSpeed").vector2Value = new Vector2(1, 1);
        row3.FindPropertyRelative("_shootDamage").intValue = 0;
        row3.FindPropertyRelative("_health").intValue = 30;
        row3.FindPropertyRelative("_armor").intValue = 0;

        // Строка 4: Зомби-качок
        var row4 = rowsProperty.GetArrayElementAtIndex(4);
        row4.FindPropertyRelative("_character").stringValue = "Зомби-качок";
        row4.FindPropertyRelative("_movementSpeed").vector2Value = new Vector2(0.8f, 1.2f);
        row4.FindPropertyRelative("_attackSpeed").vector2Value = new Vector2(0.6f, 0.9f);
        row4.FindPropertyRelative("_meleeDamage").intValue = 30;
        row4.FindPropertyRelative("_shootSpeed").vector2Value = Vector2.zero;
        row4.FindPropertyRelative("_shootDamage").intValue = 0;
        row4.FindPropertyRelative("_health").intValue = 50;
        row4.FindPropertyRelative("_armor").intValue = 0;

        // Строка 5: Зомби-коп
        var row5 = rowsProperty.GetArrayElementAtIndex(5);
        row5.FindPropertyRelative("_character").stringValue = "Зомби-коп";
        row5.FindPropertyRelative("_movementSpeed").vector2Value = new Vector2(0.7f, 1.0f);
        row5.FindPropertyRelative("_attackSpeed").vector2Value = Vector2.zero;
        row5.FindPropertyRelative("_meleeDamage").intValue = 0;
        row5.FindPropertyRelative("_shootSpeed").vector2Value = new Vector2(0.5f, 0.5f);
        row5.FindPropertyRelative("_shootDamage").intValue = 40;
        row5.FindPropertyRelative("_health").intValue = 80;
        row5.FindPropertyRelative("_armor").intValue = 20;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(_enemiesData);

        Debug.Log("Пример данных загружен успешно!");
    }
}

// Вспомогательный класс для красивого отображения таблицы
public static class TableStyles
{
    private static GUIStyle _headerStyle;
    private static GUIStyle _cellStyle;

    public static GUIStyle HeaderStyle
    {
        get
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                _headerStyle.alignment = TextAnchor.MiddleCenter;
                _headerStyle.normal.textColor = Color.white;
                _headerStyle.padding = new RectOffset(5, 5, 5, 5);
            }
            return _headerStyle;
        }
    }

    public static GUIStyle CellStyle
    {
        get
        {
            if (_cellStyle == null)
            {
                _cellStyle = new GUIStyle(EditorStyles.textField);
                _cellStyle.alignment = TextAnchor.MiddleLeft;
                _cellStyle.padding = new RectOffset(5, 5, 3, 3);
            }
            return _cellStyle;
        }
    }
}
#endif
