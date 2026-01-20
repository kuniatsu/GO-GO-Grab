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

---

## 4. 開発ロードマップ (Development Phase)

AIへの指示出しはこのフェーズ順に行うこと。現在は **[Phase 1]** に着手中。

### [Phase 1] 走行可能なミニマップの構築
- [ ] Unityプロジェクトのセットアップ (URP推奨)。
- [ ] Cesium for Unityの導入とGoogle Maps APIキーの設定。
- [ ] ベンタイン市場（`10.7725, 106.6980`）の表示。
- [ ] エリア制限（3ブロック外に出られない透明な壁の設置）。

### [Phase 2] バイク走行の実装
- [ ] 仮のバイク（Cube）の配置。
- [ ] `BikeController.cs` の作成（加速、減速、旋回）。
- [ ] **重要:** Raycast Suspensionの実装（見た目のズレ吸収）。
- [ ] カメラ追従（Cinemachine または シンプルなFollow script）。

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
1.  **仕様遵守:** 上記の「Hybrid Map Strategy」を厳守すること。CesiumのColliderをそのまま使ってはいけない。
2.  **コード品質:** C#コードはUnityのベストプラクティスに従い、コメントを日本語で記述すること。
3.  **ステップ実行:** 一度に全て実装せず、上記のPhaseごとにタスクを完了させること。