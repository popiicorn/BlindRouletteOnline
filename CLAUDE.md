# CLAUDE.md

Unity 製ゲーム「BlindRouletteOnline」のプロジェクト。プレイヤーが部屋を移動してお宝を運び、仕掛け人が選んだ部屋が爆発するターン制ゲーム。

## 環境

- Unity **6000.0.47f1**（Unity 6）/ URP 17.0.4
- 主要パッケージ: uGUI + TextMeshPro, Input System 1.19（`activeInputHandler: 2` = 旧 Input Manager と併用）, AI Navigation, Timeline, Test Framework
- IDE: Visual Studio / Rider（`.csproj` / `.sln` は Unity が自動生成するので手で編集しない）
- Windows 環境。パスやコマンドは Windows 前提で考える

## ディレクトリ構成

- `Assets/Scripts/` — **自作スクリプトはすべてここ**（asmdef なし = `Assembly-CSharp`）
- `Assets/Scenes/` — 自作シーン
- それ以外の `Assets/` 配下（`JMO Assets`, `CartoonVFX9x`, `BTM_Assets`, `PolyOne`, `PinkTea`, `PolyKebap`, `MONQO ...`, `TutorialInfo`, `TextMesh Pro` など）は **アセットストア由来のサードパーティ素材。編集しない**
- `Library/`, `Temp/`, `Obj/`, `Logs/`, `UserSettings/` は自動生成物。読まない・触らない

## シーン遷移（Build Settings 順）

`TitleScene` → `LobbyScene` → `SelectionScene` → `GameScene`（ターン数ぶん再ロード）→ `ResultScene` → `TitleScene`

- シーン名は文字列で `SceneManager.LoadScene("...")` している。シーン名を変える場合は全スクリプトを grep して合わせる
- 新しいシーンを追加したら `ProjectSettings/EditorBuildSettings.asset` への登録が必要（Unity エディタの Build Profiles から行うようユーザーに伝える）

## 主要スクリプトと状態管理

- `GameManager` — ターン進行・タイマー・お宝生成・爆発処理の中心。GameScene をターンごとに再ロードする設計
- シーンをまたぐ状態は **`GameManager` の static フィールド**（`hostMoney`, `playerTotalMoney`, `currentTurn`）で保持。リセットは `SceneChanger.LoadSceneAndResetData`
- 永続設定は `PlayerPrefs`（キー: `"SettingMaxTurns"`, `"PlayerColorIndex"`）。キー名を変えるときは読み書き両方を直す
- `TreasureData` — お宝/爆弾の `ScriptableObject`（`CreateAssetMenu: Item/ItemData`）
- 参照の取得は `GameObject.Find("名前")` / `FindGameObjectsWithTag` / `FindObjectOfType` に依存している箇所が多い。**シーン内のオブジェクト名・タグ（`Player`, `Door`）を変えると壊れる**ので、名前で検索している箇所を変更する際は注意喚起する
- 入力は現状 旧 Input Manager（`Input.GetAxisRaw`, `Input.GetKeyDown(KeyCode.Space)`）。勝手に Input System へ移行しない

## コーディング規約（既存コードに合わせる）

- クラス名・メソッド名・public フィールドは PascalCase / camelCase（Unity 標準）。`namespace` は使っていない
- インスペクタ項目は `[Header("日本語の説明")]` でグループ化する
- **コメントは日本語**。既存の `★` マーカー（変更点の目印）はそのまま残してよい
- UI テキストは `TextMeshProUGUI` を使い、文字列補間で日本語表示（例: `$"残り時間: {n}秒"`、金額は `ToString("#,0")`）
- null チェック（`if (x != null)` / `?.`）を入れてから UI を更新する既存スタイルに合わせる
- 1 ファイル 1 MonoBehaviour、ファイル名 = クラス名（一致しないと Unity がアタッチできない）

## ファイル編集時の注意（重要）

- `Assets/Scripts/` の `.cs` はすべて **UTF-8 (BOM 付き) + CRLF** に統一済み。新規ファイルも同じ形式で作成する（Visual Studio の既定 Shift-JIS で保存されていないか、追加時に確認する）
- `.cs` を新規作成・削除・リネームしたら `.meta` ファイルも対応させる（新規作成時の `.meta` は Unity が自動生成するので作らなくてよい。リネーム／移動時は `.meta` も一緒に動かして GUID を保つ）
- public フィールド名を変更するとインスペクタで設定した値が失われる。必要なら `[FormerlySerializedAs("旧名")]` を付ける
- `.unity` / `.prefab` / `.asset` は YAML だが手編集は原則しない。シーン上の設定変更が必要な場合はエディタでの手順をユーザーに説明する

## Unity 開発での一般的な注意

- `Update()` 内での `GetComponent` / `Find` 系呼び出しや毎フレームの文字列生成・アロケーションは避け、`Start()` / `Awake()` でキャッシュする
- 物理処理は `FixedUpdate()`、カメラ追従は `LateUpdate()`
- タグ比較は `CompareTag("Player")` を使う（`tag ==` は使わない）
- `FindObjectOfType` は Unity 6 で非推奨。新規コードでは `FindFirstObjectByType` / `FindAnyObjectByType` を使う（既存コードの一括置換は頼まれたときだけ）
- Unity の Object は `?.` / `??` だと破棄済み判定が正しく働かない。`!= null` で比較する
- コルーチンを使うときは `StopAllCoroutines` やシーン遷移による中断を考慮する

## ビルド・確認

- コンパイル・動作確認は Unity エディタで行う（CLI からのビルド手順は未整備）。変更後はユーザーにエディタでの確認を依頼し、Console のエラー内容を共有してもらう
- テストは現状なし（Test Framework は導入済み）

## Git

- コミットメッセージは日本語の短い要約（例: 「リザルトに金額反映」）
- 大きなバイナリ（モデル・テクスチャ・フォントアセット）の差分は意図せずコミットしないよう確認する
