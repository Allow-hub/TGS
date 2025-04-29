using UnityEngine;
using UnityEngine.InputSystem;


namespace  TechC
{
    #if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
    public class SnapDirectionProcessor : InputProcessor<Vector2>
{
    // 入力値の大きさを正規化するかどうか
    public bool digitalNormalized;

    private const string ProcessorName = "SnapDirection";

#if UNITY_EDITOR
    static SnapDirectionProcessor() => Initialize();
#endif

    // Processorの登録処理
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // 重複登録すると、Input ActionのProcessor一覧に正しく表示されない事があるため、
        // 重複チェックを行う
        if (InputSystem.TryGetProcessor(ProcessorName) == null)
            InputSystem.RegisterProcessor<SnapDirectionProcessor>(ProcessorName);
    }

    // 独自のProcessorの処理定義
    public override Vector2 Process(Vector2 value, InputControl control)
    {
        // 入力値の角度を取得
        var angle = Mathf.Atan2(value.y, value.x) * Mathf.Rad2Deg;

        // 角度を8方向にスナップさせる
        angle = Mathf.Round(angle / 45) * 45;

        // ベクトルの大きさを決定する
        var magnitude = value.magnitude;
        
        if (digitalNormalized)
            magnitude = magnitude > InputSystem.settings.defaultButtonPressPoint ? 1 : 0;

        // 角度と大きさからベクトルを作成
        return new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * magnitude;
    }
}
}
