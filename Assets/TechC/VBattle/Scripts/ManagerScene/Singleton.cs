using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TechC
{
    /// <summary>
    /// 初期化フェーズの定義
    /// </summary>
    public enum InitPhase
    {
        Early = 0,       // もっとも早い（GameManagerや設定初期化など）
        MiddleEarly = 1, 
        MiddleLate = 2, 
        Late = 3        
    }

    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        /// <summary>
        /// 初期化フェーズを指定（派生クラスでオーバーライド）
        /// </summary>
        protected virtual InitPhase GetInitPhase() => InitPhase.MiddleEarly;

        /// <summary>
        /// 派生クラスでこの値を変更して、DontDestroyOnLoad を使うかどうかを制御する
        /// </summary>
        protected virtual bool UseDontDestroyOnLoad => true;

        /// <summary>
        /// 重複時に GameObject ごと破壊するか（false だとこのコンポーネントだけ破壊）
        /// </summary>
        protected virtual bool DestroyTargetGameObject => false;

        public static T I { get; private set; } = null;
        public static bool IsValid() => I != null;

        // 全シングルトンの初期化管理用
        private static readonly Dictionary<InitPhase, List<Singleton<T>>> _phaseGroups = 
            new Dictionary<InitPhase, List<Singleton<T>>>();
        private static bool _isInitializing = false;
        private static InitPhase _currentPhase = InitPhase.Early;

        private void Awake()
        {
            if (I == null)
            {
                I = this as T;

                if (UseDontDestroyOnLoad)
                {
                    DontDestroyOnLoad(this.gameObject);
                }

                // フェーズ別初期化システムに登録
                RegisterForPhaseInit();
            }
            else
            {
                if (DestroyTargetGameObject)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(this);
                }
            }
        }

        private void RegisterForPhaseInit()
        {
            var phase = GetInitPhase();
            
            if (!_phaseGroups.ContainsKey(phase))
            {
                _phaseGroups[phase] = new List<Singleton<T>>();
            }

            _phaseGroups[phase].Add(this);

            // 初期化プロセスが開始されていない場合は開始
            if (!_isInitializing)
            {
                StartCoroutine(ProcessPhaseInitialization());
            }
        }

        private static IEnumerator ProcessPhaseInitialization()
        {
            _isInitializing = true;

            // 全てのシングルトンのAwakeが完了するまで待機
            yield return new WaitForEndOfFrame();

            // 各フェーズを順番に処理
            var phases = System.Enum.GetValues(typeof(InitPhase)).Cast<InitPhase>().OrderBy(p => (int)p);

            foreach (var phase in phases)
            {
                if (_phaseGroups.ContainsKey(phase))
                {
                    _currentPhase = phase;
                    Debug.Log($"=== Starting {phase} Phase Initialization ===");

                    var singletons = _phaseGroups[phase];
                    
                    // 同一フェーズ内のシングルトンを並行して初期化開始
                    foreach (var singleton in singletons)
                    {
                        if (singleton != null)
                        {
                            singleton.StartInit();
                        }
                    }

                    // 同一フェーズ内の全ての初期化が完了するまで待機
                    yield return new WaitUntil(() => 
                        singletons.All(s => s == null || s.IsInitialized()));

                    Debug.Log($"=== {phase} Phase Initialization Complete ===");

                    // 次のフェーズに進む前に1フレーム待機
                    yield return null;
                }
            }

            Debug.Log("=== All Singleton Initialization Complete ===");
            _isInitializing = false;

            // 全初期化完了後のコールバック
            NotifyAllInitializationComplete();
        }

        private static void NotifyAllInitializationComplete()
        {
            foreach (var phaseGroup in _phaseGroups.Values)
            {
                foreach (var singleton in phaseGroup)
                {
                    singleton?.OnAllSingletonsInitialized();
                }
            }
        }

        private void OnDestroy()
        {
            if (I == this)
            {
                I = null;
                OnRelease();
            }
        }

        // ISingletonInit インターフェースの実装
        private bool _isInitialized = false;
        private Coroutine _initCoroutine = null;

        public bool IsInitialized() => _isInitialized;

        public void StartInit()
        {
            if (!_isInitialized && _initCoroutine == null)
            {
                Debug.Log($"Initializing {typeof(T).Name} ({GetInitPhase()} phase)");
                _initCoroutine = StartCoroutine(InitCoroutine());
            }
        }

        private IEnumerator InitCoroutine()
        {
            // 非同期初期化が必要な場合のために、コルーチンとして実装
            yield return StartCoroutine(InitAsync());
            
            _isInitialized = true;
            Debug.Log($"{typeof(T).Name} initialization completed");
        }

        public void OnAllSingletonsInitialized()
        {
            OnPostInit();
        }

        /// <summary>
        /// 派生クラス用の初期化メソッド（同期）
        /// </summary>
        protected virtual void Init() { }

        /// <summary>
        /// 派生クラス用の非同期初期化メソッド
        /// デフォルトでは Init() を呼び出すだけ
        /// </summary>
        protected virtual IEnumerator InitAsync()
        {
            Init();
            yield return null;
        }

        /// <summary>
        /// 全シングルトンの初期化完了後に呼ばれる
        /// </summary>
        protected virtual void OnPostInit() { }

        /// <summary>
        /// 派生クラス用の破棄処理
        /// </summary>
        protected virtual void OnRelease() { }

        /// <summary>
        /// 現在の初期化フェーズを取得
        /// </summary>
        public static InitPhase GetCurrentPhase() => _currentPhase;

        /// <summary>
        /// 指定したフェーズが完了しているかチェック
        /// </summary>
        public static bool IsPhaseComplete(InitPhase phase)
        {
            return (int)_currentPhase > (int)phase || 
                   (_currentPhase == phase && _phaseGroups.ContainsKey(phase) && 
                    _phaseGroups[phase].All(s => s?.IsInitialized() ?? true));
        }
    }
}