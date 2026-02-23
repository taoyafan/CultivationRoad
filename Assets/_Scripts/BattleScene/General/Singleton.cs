using System.Reflection;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();
                        
                        if (singletonObject.transform.parent != null)
                        {
                            singletonObject.transform.parent = null;
                        }
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = this as T;
                
                if (transform.parent != null)
                {
                    transform.parent = null;
                }

                AutoFindSerializedFields();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    private void AutoFindSerializedFields()
    {
        BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        FieldInfo[] fields = GetType().GetFields(bindingFlags);

        foreach (FieldInfo field in fields)
        {
            if (field.IsDefined(typeof(SerializeField), false))
            {
                object currentValue = field.GetValue(this);
                if (currentValue == null)
                {
                    System.Type fieldType = field.FieldType;
                    
                    if (typeof(Component).IsAssignableFrom(fieldType))
                    {
                        UnityEngine.Object foundObject = FindObjectOfType(fieldType, true);
                        if (foundObject != null && foundObject is Component component)
                        {
                            field.SetValue(this, component);
                            Debug.Log($"[Singleton] Auto-found {field.Name} ({fieldType.Name}) for {GetType().Name}");
                        }
                    }
                    else if (fieldType.IsClass)
                    {
                        UnityEngine.Object foundObject = FindObjectOfType(fieldType, true);
                        if (foundObject != null)
                        {
                            field.SetValue(this, foundObject);
                            Debug.Log($"[Singleton] Auto-found {field.Name} ({fieldType.Name}) for {GetType().Name}");
                        }
                    }
                }
            }
        }
    }
}
