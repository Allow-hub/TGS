using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TechC
{
    public abstract class PolymorphicListEditor<TTarget, TInterface> : Editor
        where TTarget : UnityEngine.Object
    {
        protected abstract string PropertyName { get; }

        private SerializedProperty listProperty;
        private static List<Type> concreteTypes;

        protected virtual void OnEnable()
        {
            listProperty = serializedObject.FindProperty(PropertyName);

            if (concreteTypes == null)
            {
                concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => asm.GetTypes())
                    .Where(t =>
                        typeof(TInterface).IsAssignableFrom(t) &&
                        !t.IsAbstract &&
                        t.IsClass &&
                        t.GetConstructor(Type.EmptyTypes) != null)
                    .ToList();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField($"{typeof(TInterface).Name} List", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(element, new GUIContent($"Element {i + 1}"), true);

                if (GUILayout.Button("Remove"))
                {
                    listProperty.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("+ Add Element"))
            {
                ShowAddMenu();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowAddMenu()
        {
            var menu = new GenericMenu();

            foreach (var type in concreteTypes)
            {
                menu.AddItem(new GUIContent(type.Name), false, () =>
                {
                    var instance = Activator.CreateInstance(type);
                    listProperty.arraySize++;
                    var element = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                    element.managedReferenceValue = instance;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }
    }
}
