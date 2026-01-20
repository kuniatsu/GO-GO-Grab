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

### G. バイクコントローラー仕様 (Bike Controller System)
* **入力スキーム:**
  * **前進:** W または ↑ キー (速度 = maxSpeed に向けて加速)
  * **後退:** S または ↓ キー (速度 = -maxSpeed × 0.5)
  * **左旋回:** A または ← キー (現在の速度に応じて旋回)
  * **右旋回:** D または → キー (同上)
* **移動パラメータ（デフォルト値）:**
  * `maxSpeed = 30 m/s`: 最大速度
  * `acceleration = 50 m/s²`: 加速度（速度到達時間 ≈ 0.6秒）
  * `deceleration = 30 m/s²`: 減速度
  * `turnSpeed = 180 度/s`: 旋回速度（一周 = 2秒）
  * `driftFriction = 0.95`: ドリフト時の摩擦係数（将来実装用）
* **物理パラメータ:**
  * `mass = 1000 kg`: バイク質量
  * `Rigidbody.drag = 0.1`: 空気抵抗
  * `Rigidbody.angularDrag = 2`: 回転抵抗
  * `FreezeRotationZ`: Z軸回転を制限（転倒防止）
* **公開API:**
  * `float GetCurrentSpeed()`: 現在の移動速度を取得
  * `Vector3 GetVelocity()`: Rigidbody の速度ベクトルを取得
  * `bool IsMoving()`: 速度 > 0.5 m/s かを判定
* **統合ポイント:**
  * BoundaryManager との自動結合（`Start()` で自動取得）
  * MapConfig の座標系を透過的に使用

### H. サスペンションシステム仕様 (Raycast Suspension)
* **物理モデル:** スプリング・ダンパー系
  * サスペンション力 = `(distanceError × suspensionForce) - (heightVelocity × suspensionDamping)`
  * `distanceError = suspensionHeight - raycastAverageDistance`
* **Raycast 設定：**
  * **rayCount = 4**: バイク下部の4点から Ray を放射
  * **raycastRadius = 0.5m**: Ray 配置の半径（バイク幅に合わせて調整）
  * **raycastDistance = 2m**: Ray の最大距離（床探索範囲）
  * **路面検知:** Road レイヤーのみを検索
* **サスペンションパラメータ（デフォルト値）:**
  * `suspensionHeight = 0.5m`: 床からの目標高さ
  * `suspensionForce = 30N`: スプリング定数（値が大きい = 硬い）
  * `suspensionDamping = 2`: ダンパー係数（値が大きい = 揺れが少ない）
* **調整ガイド:**
  * **揺れが大きい場合:** suspensionDamping を増加（2 → 3-4）
  * **沈み込みが大きい場合:** suspensionForce を増加（30 → 40-50）
  * **応答が遅い場合:** suspensionHeight を減少（0.5 → 0.3-0.4）
* **デバッグ方法:**
  * Inspector で `raycastRadius` を変更して Ray 配置を調整
  * Gizmos ビジュアライズで Ray ヒット状況を確認（黄色 = Ray 位置、緑 = ヒット、赤 = ミス）
  * Spacebar キー長押しで Console にデバッグ情報出力

### I. カメラシステム仕様 (Camera Follower)
* **追従方式:**
  * **基本:** バイクの後ろ上からの追従カメラ（LateUpdate で更新）
  * **オフセット:** バイクの向きに基づいて回転（`TransformDirection` を使用）
* **カメラパラメータ（デフォルト値）:**
  * `cameraOffset = (0, 4, -8)`: バイクからの相対位置
    * Y = 4m（高さ）→ ボンネット上方視点
    * Z = -8m（後方） → 後ろから見る距離
    * X = 0（中央） → 中央揃え
  * `followSpeed = 5`: 追従速度（スムージング強度）
    * 値が大きい = リアルタイム追従
    * 値が小さい = 遅延した追従（カメラ酔い軽減）
* **Look Ahead 機能:**
  * `enableLookAhead = true`: ON で有効
  * `lookAheadDistance = 5m`: バイクの前方を見る距離
  * 進行方向の状況を事前に把握可能
* **カメラ揺れ効果（オプション）:**
  * `enableCameraShake = false`: デフォルト OFF（必要に応じて ON）
  * `cameraShakeAmount = 0.2`: 揺れの大きさ（Perlin Noise ベース）
  * 移動中の動感を演出
* **公開API:**
  * `void SetCameraOffset(Vector3 offset)`: カメラオフセットを動的に変更
  * `void SetFollowSpeed(float speed)`: 追従速度を変更
  * `void SetLookAheadEnabled(bool enabled)`: Look Ahead ON/OFF
  * `void SetCameraShakeEnabled(bool enabled)`: カメラ揺れ ON/OFF

---

## 4. 開発ロードマップ (Development Phase)

AIへの指示出しはこのフェーズ順に行うこと。現在は **[Phase 3]** に着手準備中。

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
- [ ] ベンタイン市場周辺の実際の道路形状をマッピング。
- [ ] 複数の Plane を配置し、InvisibleRoad システムの完成。
- [ ] Road レイヤー設定と Collider 行列の確認。
- [ ] Raycast Suspension デバッグ（Gizmos 可視化）。
- [ ] Cesium 3D Tiles との整合性テスト（貫通・ズレなし）。
- [ ] **テスト実行:** WASD でバイク走行、曲線道路対応確認。

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
  * WASD 入力の反応性をテスト（acceleration / deceleration パラメータで調整）。
* Raycast Suspension 実装時：
  * 透明 Road Plane との距離をRaycastで計測。
  * 車体の浮遊高さを Y 座標で調整。
  * raycastRadius は バイク Collider サイズに合わせて調整（デフォルト 0.5m）。
* カメラ追従：
  * CameraFollower.cs で LateUpdate() での追従を実装。
  * cameraOffset は ゲームプレイ性に応じてチューニング。
  * Look Ahead により、進行方向の先読みを実現。

### 5.5 Phase 3 開発時の注意点
* **道路メッシュ敷設時：**
  * InvisibleRoad (Plane) を Road レイヤーに割り当て。
  * 複数の Plane を組み合わせる場合、隙間がないか確認（バイクが落ちる可能性）。
  * 曲線道路の場合、複数の小さな Plane で段階的に表現。
* **Raycast Suspension の動作確認：**
  * Gizmos ビジュアライズで Ray が Road に正しく当たっているか確認。
  * Red Ray（ミス） が多い場合：raycastDistance を増加。
  * raycastRadius を Road Plane 幅に合わせて調整。
* **Cesium との整合性：**
  * Visual Layer (Cesium 3D Tiles) は VisualOnly レイヤーに配置。
  * Bike ↔ VisualOnly 衝突が無効か確認。
  * InvisibleRoad が Cesium Geometry と一致しているか視認テスト。
* **パフォーマンス最適化：**
  * Road Plane のメッシュ面数を抑える（Plane は最小限）。
  * Raycast 数が増えた場合、rayCount を制限（4-8 推奨）。
  * Physics.Raycast の非同期化（複数フレームに分散）は後続 Phase で検討。
* **テスト項目（Phase 3 完了条件）：**
  * [ ] バイクが Road Plane 上で安定して走行
  * [ ] Cesium 3D Tiles を通り抜ける（衝突しない）
  * [ ] エリア制限が機能する（300m 外に出られない）
  * [ ] カメラが滑らかに追従
  * [ ] 曲線道路でのサスペンション応答テスト