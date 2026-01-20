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

## 次のステップ

セットアップが完了したら、 **Phase 2** に進みます：

- [ ] 仮のバイク（Cube）の配置
- [ ] `BikeController.cs` の実装
- [ ] Raycast Suspension の実装
- [ ] カメラ追従スクリプト

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
