# Crazy Grab (MVP) - Setup Guide

このガイドに従って、ローカル環境で Crazy Grab プロジェクトをセットアップしてください。

---

## 前提条件

- **Unity:** 2022.3 LTS (Silicon Native推奨 - macOS M1/M2)
- **IDE:** Cursor / VS Code
- **OS:** macOS (iOS へのビルドは後続フェーズ)

---

## セットアップステップ

### Step 1: Unity プロジェクトの初期化

1. **Unity Hub** を起動
2. **New Project** を選択
3. **Editor Version:** `2022.3 LTS` を選択
4. **Template:** `Universal 3D` を選択
5. **Project Location:** `/home/user/GO-GO-Grab` を指定
6. **Create** をクリック

> **注意:** このリポジトリには `Assets/` 、`ProjectSettings/` ディレクトリが既に存在します。Unity Editor が起動時に自動的にこれらを認識します。

### Step 2: UPM 依存パッケージのインストール

Unity Editor が起動したら、以下の手順で UPM パッケージをインストール：

1. **Window** → **Package Manager** を開く
2. 左上の **+** ボタン → **Add package from git URL** を選択
3. 以下を順番に追加：
   - `com.cesium.unity` (Cesium Registry から)
   - `com.unity.cinemachine` (Unity Registry から)
   - `com.unity.inputsystem` (Unity Registry から)
   - `com.unity.render-pipelines.universal` (既にインストール済みの可能性)

**または** Terminal から以下を実行：

```bash
cd /home/user/GO-GO-Grab
cat Packages/manifest.json
```

> **Packages/manifest.json** に依存パッケージが定義されています。
> Unity Editor 起動時に自動的に解決されます。

### Step 3: Cesium for Unity のセットアップ

1. **Window** → **Cesium** → **Cesium Ion** を開く
2. **Cesium Ion Account** を作成 / ログイン
3. **Google Maps 3D Tiles** アセットを検索・導入
4. **Access Token** を取得して保存

> **重要:** Access Token は `ProjectSettings/` 配下に別途保存してください（`.gitignore` に含める）

### Step 4: シーンの初期セットアップ

1. **Assets/Scenes** 内に新規シーンを作成：`MainGameScene.unity`
2. Hierarchy に以下のゲームオブジェクトを作成：
   - **CesiumGeoreference** (Cesium for Unity)
   - **BoundaryManager** (エリア制限用)
   - **GameManager** (ゲームロジック用 - Phase 4)

3. **CesiumGeoreference** の設定：
   - Latitude: `10.7725`
   - Longitude: `106.6980`
   - Height: `50.0` (メートル)

### Step 5: マップの確認

1. Sceneビュー上で **Play** を実行
2. ベンタイン市場周辺が表示されているか確認
3. **BoundaryManager** の可視化 ON で、透明な壁が見えるか確認

---

## 注意事項

### Hybrid Map Strategy（重要）

このプロジェクトは以下の2層構造を採用しています：

- **Visual Layer (Cesium):** 見た目用の3D Tiles (Google Maps)
- **Physics Layer (Invisible Road):** 走行用の透明な床 (Plane メッシュ)

**CesiumのMesh Collider は無効化** してください。
バイクの物理演算は透明な床（Invisible Plane）の上でのみ行われます。

### ディレクトリ構造

```
GO-GO-Grab/
├── Assets/
│   ├── Scripts/
│   │   ├── MapConfig.cs          # 座標管理
│   │   ├── BoundaryManager.cs    # エリア制限
│   │   ├── BikeController.cs     # (Phase 2) バイク操作
│   │   └── GameManager.cs        # (Phase 4) ゲームロジック
│   ├── Scenes/
│   │   └── MainGameScene.unity   # メインシーン (作成予定)
│   ├── Materials/
│   ├── Prefabs/
│   └── Resources/
├── Packages/
│   └── manifest.json             # UPM 依存パッケージ
├── ProjectSettings/
│   └── ProjectSettings.yaml      # プロジェクト設定
└── README.md                      # プロジェクト概要
```

---

## Phase 2: バイク走行の実装

### Step 6: バイク用ゲームオブジェクトの作成

1. **Hierarchy** で右クリック → **3D Object** → **Cube** を選択
2. 以下の設定を行う：
   - **名前:** `Bike`
   - **Position:** (0, 1, 0) - CesiumGeoreference の中心上
   - **Scale:** (1, 0.5, 2) - バイクっぽい形に調整

### Step 7: Rigidbody とコライダー設定

**Bike** ゲームオブジェクトに以下を設定：

1. **Rigidbody** コンポーネント：
   - Add Component → Physics → Rigidbody
   - **Mass:** 1000
   - **Drag:** 0.1
   - **Angular Drag:** 2
   - **Constraints:** Freeze Rotation Z (Z軸回転を制限)

2. **Box Collider** の確認：
   - Size: (1, 0.5, 2)
   - Layer を `Bike` に設定（新規作成が必要）

### Step 8: スクリプトの追加

**Bike** ゲームオブジェクトに以下のスクリプトを Add Component で追加：

1. **BikeController.cs**
   - BoundaryManager: (Scene の BoundaryManager オブジェクトをドラッグ)
   - Max Speed: 30
   - Acceleration: 50
   - Deceleration: 30
   - Turn Speed: 180
   - Bike Radius: 1

2. **RaycastSuspension.cs**
   - Road Layer: `Road` を選択
   - Suspension Height: 0.5
   - Suspension Force: 30
   - Suspension Damping: 2
   - Raycast Distance: 2
   - Ray Count: 4

### Step 9: 透明な Road Plane の作成

**Hybrid Map Strategy** に従い、バイクが走行する透明な床を作成：

1. **Hierarchy** で右クリック → **3D Object** → **Plane** を選択
2. 設定：
   - **名前:** `InvisibleRoad`
   - **Position:** (0, 0, 0)
   - **Scale:** (100, 1, 100) - 十分な広さ
   - **Layer:** `Road` に設定（新規作成）

3. **Mesh Renderer** の Material を透明にする（オプション）：
   - または Material で Transparent モードに設定

4. **Physics Settings** で Cesium との干渉を避ける：
   - Road layer の Collider は `Road` layer に属する

### Step 10: コリジョンレイヤーの設定（重要）

Unity Editor で以下の手順を実行：

1. **Edit** → **Project Settings** → **Physics**
2. **Layers** セクションで新規レイヤーを作成：
   - Layer 8: `Bike`
   - Layer 9: `Road`
   - Layer 10: `VisualOnly`

3. **Collision Matrix** を設定：
   - `Bike` ↔ `Road`: ✓ 有効（衝突判定あり）
   - `Bike` ↔ `VisualOnly`: ✗ 無効（Cesium を通り抜ける）
   - その他の組み合わせ: 無視してOK

### Step 11: カメラのセットアップ

1. **Main Camera** を選択（通常は自動作成されている）
2. **Add Component** → CameraFollower.cs を追加
3. 設定：
   - **Bike Transform:** Scene の Bike オブジェクトをドラッグ
   - **Camera Offset:** (0, 4, -8) - 後ろ上から見る角度
   - **Follow Speed:** 5
   - **Look Ahead Distance:** 5
   - **Enable Look Ahead:** ✓ チェック

### Step 12: ゲーム実行とテスト

1. **Play** ボタンを押す
2. 以下をテスト：
   - **WASD / Arrow Keys** でバイクを操作
   - **前進:** W or ↑
   - **後退:** S or ↓
   - **左旋回:** A or ←
   - **右旋回:** D or →
3. エリア制限テスト：
   - バイクが300m 円形エリアを超えて出られないか確認
   - BoundaryManager の **visualizeBoundary** を ON にすると、透明な壁が可視化される

---

## Phase 2 実装内容

| スクリプト | 機能 |
|-----------|------|
| **BikeController.cs** | Arcade Style の移動・回転、入力処理、エリア制限統合 |
| **RaycastSuspension.cs** | 透明 Road Plane との距離計測、車体の浮遊高さ調整 |
| **CameraFollower.cs** | バイク追従カメラ、スムーズな視点管理 |

---

## Phase 3: 物理道路（透明床）の敷設

### Step 13: 道路メッシュ（Invisible Road Plane）の配置確認

Phase 2 の Step 9 で作成した **InvisibleRoad** Plane が正しく配置されているか確認：

1. **Hierarchy** で `InvisibleRoad` を選択
2. 以下を確認：
   - **Position:** (0, 0, 0) - ベンタイン市場中心
   - **Scale:** (100, 1, 100) - 十分な広さ
   - **Layer:** `Road` に設定済みか確認
3. **Gizmos** で可視化（Scene ビュー上部）：
   - Gizmos ボタンをクリック → Colliders にチェック
   - Plane の Collider が緑色で表示されることを確認

### Step 14: Raycast Suspension デバッグ

**RaycastSuspension** が Road Plane を正しく検知しているか確認：

1. **Play** ボタンを押してゲーム実行
2. **Scene ビュー** に切り替え（Game ビューの隣）
3. 以下を観察：
   - **黄色のドット（4個）:** Raycast 発射地点（バイク下部）
   - **緑色の線:** Road に正常にヒット
   - **赤色の線:** Road に当たらない（エラー）
4. **Spacebar** を長押し → Console でデバッグ情報表示：
   ```
   [Suspension] Distance: 0.50m, Error: 0.00m, Force: 0.0N
   ```

**問題が発生した場合:**
- Red Ray が多い → `raycastDistance` を増加（2 → 3-4m）
- Ray が Road Plane 外 → `raycastRadius` を調整（0.5 → バイク幅に合わせる）

### Step 15: Cesium との整合性テスト

Cesium 3D Tiles（Visual Layer）とバイク走行（Physics Layer）の整合性を確認：

1. **Play** で実行中にバイクを移動（WASD キー）
2. **以下を確認:**
   - ✅ バイクが Cesium の建物を通り抜ける（衝突しない）
   - ✅ バイクが InvisibleRoad 上で安定して走行
   - ✅ カメラがスムーズに追従
   - ✅ 斜面・曲線でのサスペンション応答が滑らか
3. **Cesium Geometry とのズレ確認:**
   - Visual（見た目） と Physics（走行面） が大きくズレていないか
   - ズレが大きい場合 → InvisibleRoad の Position/Scale を調整

### Step 16: 曲線道路への対応（オプション）

ホーチミン市の実際の道路に合わせて曲線道路を敷設：

1. 複数の小さな **Plane** を曲線状に配置
   - 各 Plane を 45-90 度ずつ回転
   - Y 軸方向（上下）も調整して高さの変化に対応

2. コリジョン設定を確認：
   - すべての Plane が **Road レイヤー** に属しているか
   - 隙間がないか確認（バイクが落ちる可能性）

3. テスト実行：
   - WASD でバイクを曲線道路に沿って移動
   - Gizmos で Ray がすべての Plane にヒットしているか確認

---

## Phase 3 完了チェックリスト

- [ ] InvisibleRoad Plane が Road レイヤーに設定済み
- [ ] Gizmos で Plane Collider が緑色で表示される
- [ ] Scene ビューで Raycast ヒット状況が確認できる（緑 Ray）
- [ ] バイクが InvisibleRoad 上で安定して走行
- [ ] Cesium 3D Tiles を通り抜ける（衝突しない）
- [ ] カメラが滑らかに追従
- [ ] エリア制限（300m 境界）が機能
- [ ] 曲線道路でのサスペンション応答テスト完了

**Phase 3 完了後は Phase 4「Grabシステムのロジック実装」に進みます。**

---

## トラブルシューティング

### Cesium for Unity がインストールできない

→ Package Manager が Cesium Registry を認識していない可能性があります。
手動で `Packages/manifest.json` に registry 設定を追加してください：

```json
"scopedRegistries": [
  {
    "name": "Cesium",
    "url": "https://cesium.com/packages/cesium-npm/",
    "scopes": ["com.cesium"]
  }
]
```

### バイクが空を飛ぶ / 地面を貫通する

→ BoundaryManager と物理層（Invisible Plane）の設定を確認してください。
Raycast Suspension が正しく実装されているか確認しましょう（Phase 2）。

### Google Maps が表示されない

→ Cesium Ion の Access Token が正しく設定されているか確認
インターネット接続を確認

---

**質問や問題が発生したら、README.md の「AIアシスタントへの指示ルール」を参照してください。**
