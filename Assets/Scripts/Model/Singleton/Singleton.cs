using UnityEngine;

/// <summary>
/// 汎用的なシングルトン基底クラス
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    
    // アプリケーション終了中かどうかを判定するフラグ
    private static bool _applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            // アプリケーション終了中なら、新しく作らずにnullを返す（ゴースト生成対策）
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[Singleton] {typeof(T)} はアプリケーション終了により破棄されました。新しいインスタンスは生成しません。");
                return null;
            }

            if (_instance == null)
            {
                // 既存のインスタンスを検索
                _instance = FindFirstObjectByType<T>();

                // 見つからなかった場合
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name + " (Singleton)");
                    _instance = obj.AddComponent<T>();
                    Debug.Log("シングルトンが生成されました。");
                }
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// 初期化処理。派生クラスで必要に応じてオーバーライドする。
    /// Bootstrapperから明示的に生成タイミングを制御するために使用。
    /// </summary>
    public virtual void Initialize()
    {
        Debug.Log("シングルトンが明示的に初期化されました。");
    }

    protected virtual void Awake()
    {
        // まだインスタンスが登録されていなければ、自分自身を登録
        if (_instance == null)
        {
            _instance = this as T;
            
            // DontDestroyOnLoadはルートのオブジェクトのみ有効→親を解除
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }
        // 既に別のインスタンスが存在している場合
        else if (_instance != this)
        {
            Debug.LogWarning($"[Singleton] {typeof(T)} が複数存在するため、重複している {gameObject.name} を破棄します。");
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }
}