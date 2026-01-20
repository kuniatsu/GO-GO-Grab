# Project: Crazy Grab (MVP) - SE-Asia Hustle

## 1. プロジェクト概要
『クレイジータクシー』のゲーム性をベースに、東南アジア（ベトナム・ホーチミン）のGrabバイク事情を再現したドライビングアクションゲーム。
プレイヤーはGrabドライバーとなり、カオスな交通事情の中で客を拾い、目的地まで爆走する。

### MVPのゴール
* **ロケーション:** ホーチミン市ベンタイン市場周辺（3ブロック限定）
* **規模:** 客（乗客ターゲット）は2名のみ。
* **プラットフォーム:** まずはmacOS (Unity Editor) での動作を完璧にし、将来的にiOS (iPhone 13 mini) へ移植する。

---

## 2. 技術スタック (Tech Stack)
* **Engine:** Unity 2022.3 LTS (Silicon Native推奨)
* **Language:** C#
* **Map System:**
    * **Visual:** Cesium for Unity (Google Maps Platform 3D Tiles)
    * **Physics:** Invisible Plane / ProBuilder (透明な衝突判定用道路)
* **Asset Management:** UPM (Unity Package Manager)
* **IDE:** Cursor / VS Code

---

## 3. 実装仕様 (Core Specifications)

### A. マップ構築方針 (Hybrid Map Strategy)
リアルな見た目と快適な走行を両立させるため、以下のハイブリッド方式を採用する。
1.  **Visual Layer (Cesium):**
    * Google Mapsの3D Tilesを描画用として使用。
    * **重要:** CesiumのMesh Colliderは無効化（またはBikeレイヤーと干渉させない）。
2.  **Physics Layer (Invisible Road):**
    * 道路部分に平らなPlane（透明なメッシュ）を手動または簡易生成で配置。
    * バイクはこの「透明な床」の上を走行する（ガタガタ防止）。
3.  **座標:**
    * **Center:** Ben Thanh Market (Lat: `10.7725`, Long: `106.6980`)
    * **Scope:** 上記を中心とした半径約300m（3ブロック分）。外側は見えない壁で封鎖。

### B. バイク挙動 (Bike Physics)
* **Type:** Arcade Style (リアルシミュレーターではない)
* **Controller:**
    * Rigidbodyベース。
    * **Raycast Suspension:** 車体下部にRayを飛ばし、透明道路からの距離で車体を浮遊させる（見た目のズレ吸収）。
    * 転倒しない（Z軸回転を制限し、起き上がり処理を入れる）。
* **Input:**
    * WASD / Arrow Keys (PC)
    * Touch UI (iOS - 将来実装)

### C. ゲームループ (Game Loop)
1.  **Idle / Cruising:** 街を流している状態。
2.  **Notification:** スマホ風UIに通知が来る。「Passenger」または「Food」。
3.  **Pickup:** 指定された座標（客の場所）へ向かう。
4.  **Delivery:** 制限時間内に目的地へ向かう。
5.  **Result:** 到着タイムに応じたスコア獲得。

### D. 座標系とGIS統合 (Coordinate System & WGS84 Integration)
* **座標基準:** WGS84（世界測地系）を採用。ベンタイン市場を原点 (0, 0) とする。
* **変換方式:**
  * WGS84 緯度・経度 → Unity ローカル座標（メートル単位）
  * 計算式：
    * `latitudeOffset (m) = (latitude - CENTER_LATITUDE) × 111319.5`
    * `longitudeOffset (m) = (longitude - CENTER_LONGITUDE) × 40075017/360 × cos(CENTER_LATITUDE)`
  * Unity座標系：X軸は東方向（経度）、Z軸は北方向（緯度）、Y軸は高さ。
* **実装:** `MapConfig.cs` で静的メソッドを提供。
  * `GeoToLocalPosition()`: WGS84 → ローカル座標に変換
  * `LocalPositionToGeo()`: ローカル座標 → WGS84 に逆変換
  * `IsWithinBoundary()`: 座標がエリア内かを判定
  * `ClampToBoundary()`: エリア外の座標をクランプ

### E. エリア制限システム (Boundary Management)
* **制限形状:** 半径300m（ベンタイン市場を中心）の円形エリア。
* **実装方式:**
  * `BoundaryManager.cs` が制御。
  * 衝突判定は円形（2D平面 X-Z）で実行。
  * バイクがエリア外に出ようとしたら、`HandleBoundaryCollision()` で安全な位置に調整。
* **可視化モード:**
  * デバッグ用に透明な円柱型ウォール（64セグメント）を描画可能。
  * `visualizeBoundary` フラグで ON/OFF 可能。
* **使用方法:**
  * BikeController から `BoundaryManager.HandleBoundaryCollision(currentPos, nextPos, bikeRadius)` を呼び出し。
  * 戻り値は境界内での安全な位置。

### F. コリジョンレイヤー設定 (Collision Layers & Physics)
* **レイヤー構成:**
  * `Bike`: バイクが属するレイヤー。Cesium Colliderと干渉しない。
  * `Road`: 透明な床（Invisible Plane）が属するレイヤー。バイクと衝突。
  * `VisualOnly`: Cesium の3D Tilesが属するレイヤー。物理演算に関与しない。
* **コリジョン行列設定:**
  * Bike ↔ Road: 有効（バイクは道路の上を走行）。
  * Bike ↔ VisualOnly: 無効（Cesium Mesh を通り抜ける）。
  * Road ↔ VisualOnly: 無効。
* **設定手順（Unity Editor）:**
  1. Edit → Project Settings → Physics
  2. Layers を確認：Bike, Road, VisualOnly が定義されているか。
  3. Collision Matrix を編集し、不要な組み合わせをチェック解除。

---

## 4. 開発ロードマップ (Development Phase)

AIへの指示出しはこのフェーズ順に行うこと。現在は **[Phase 2]** に着手準備中。

### [Phase 1] 走行可能なミニマップの構築 ✅
- [x] Unityプロジェクトのセットアップ (URP推奨)。
- [x] UPM packages.json で Cesium for Unity の依存関係を定義。
- [x] `MapConfig.cs` で座標変換システム実装（WGS84 ↔ ローカル座標）。
- [x] `BoundaryManager.cs` でエリア制限ロジック実装（半径300m、円形）。
- [ ] **次ステップ:** Cesium Ion Access Token 取得 & Unity Editor での Cesium セットアップ（手動）。
- [ ] ベンタイン市場（`10.7725, 106.6980`）の表示確認。

### [Phase 2] バイク走行の実装 ✅
- [x] 仮のバイク（Cube）の配置（SETUP_GUIDE参照）。
- [x] `BikeController.cs` の作成（加速、減速、旋回、入力処理）。
- [x] **重要:** `RaycastSuspension.cs` の実装（見た目のズレ吸収）。
- [x] `CameraFollower.cs` でカメラ追従を実装。
- [ ] **次ステップ:** Unity Editor でシーン構築・コンポーネント接続・テスト実行。

### [Phase 3] 物理道路（透明床）の敷設
- [ ] 道路に沿ってPlaneを配置し、Physics Layerを設定。
- [ ] Cesiumの表示と、物理走行の整合性テスト。

### [Phase 4] Grabシステムのロジック実装
- [ ] UI作成（スマホ画面風HUD）。
- [ ] `GameManager.cs` の作成（ステート管理）。
- [ ] 客（Cylinderなど仮素材）の配置と検知ロジック。

### [Phase 5] ポリッシュ & iOSビルド準備
- [ ] バイク・キャラクターモデルの差し替え。
- [ ] iOS向けビルド設定と最適化（軽量化）。

---

## 5. AIアシスタントへの指示ルール (Instructions for AI)
このリポジトリを読み込んだAI（Claude Codeなど）は以下のルールに従うこと：

### 5.1 設計・仕様ルール
1.  **Hybrid Map Strategy の厳守:**
    * Cesium のMesh Collider は無効化し、Physics Layerで独立した透明Plane を使用。
    * CesiumはVisual Onlyレイヤーに配置。
2.  **座標系の一貫性:**
    * すべての座標変換は `MapConfig.cs` のメソッドを使用。
    * WGS84 緯度・経度とローカルメートル単位の混同を避ける。
3.  **コリジョン設定:**
    * Bike, Road, VisualOnly の3レイヤーを厳密に区別。
    * Bike ↔ Road のみ衝突判定を有効にする。

### 5.2 コード品質ルール
1.  **C#ベストプラクティス:**
    * Unity の命名規約に従う：`PascalCase` for クラス/メソッド、`camelCase` for 変数。
    * `[SerializeField]` / `[HideInInspector]` を適切に使用（Editor調整対応）。
    * XML コメント（`/// <summary>`）で公開APIを記述。
2.  **コメント言語:**
    * コード内のコメントはすべて日本語で記述。
    * 複雑なロジックには説明コメントを必須。
3.  **パフォーマンス:**
    * `Update()` での `GetComponent` 呼び出しは避ける（キャッシュ推奨）。
    * 物理演算は `FixedUpdate()` で実行。

### 5.3 ステップ実行ルール
1.  **Phase ごとのタスク完結:**
    * 一度に全実装を行わない。各Phaseを完全に完了させる。
    * 各Phase完了時に機能確認・テスト可能な状態にする。
2.  **コミットメッセージ:**
    * 形式: `Phase N: <feature> - <brief description>`
    * 日本語での説明と英語でのコミットメッセージを混在させない。
3.  **ドキュメント更新:**
    * コード追加時に README.md / SETUP_GUIDE.md も同時更新。
    * API 追加時は「D. 座標系」「E. エリア制限」などの該当セクションを更新。

### 5.4 Phase 2 開発時の注意点
* `BikeController.cs` 作成時：
  * `BoundaryManager.HandleBoundaryCollision()` を必ず統合。
  * `MapConfig.IsWithinBoundary()` で位置チェック。
* Raycast Suspension 実装時：
  * 透明 Road Plane との距離をRaycastで計測。
  * 車体の浮遊高さを Y 座標で調整。
* カメラ追従：
  * Cinemachine を使用する場合、Virtual Camera の Follow/LookAt を設定。
  * シンプル実装の場合、`LateUpdate()` で追従。