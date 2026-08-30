# CustomCosmetics

Among Us 自定义装饰品加载器。从远程仓库下载并加载自定义**帽子 / 面饰 / 名牌**，支持多仓库、包分组、增量下载。

## 安装

1. 将 `CosmeticsManager.dll` 放入 `BepInEx/plugins/`
2. 启动游戏一次，生成配置文件 `BepInEx/config/com.mxyx.cosmetics.cfg`
3. 编辑配置后重启游戏生效

## 配置 `BepInEx/config/com.mxyx.cosmetics.cfg`

| 配置项 | 默认值 | 说明 |
|---|---|---|
| `EnableHats` | `true` | 启用帽子加载 |
| `EnableVisors` | `false` | 启用面饰加载 |
| `EnableNamePlates` | `false` | 启用名牌加载 |
| `Repositories` | `https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master\|hat` | 仓库列表 |

`Repositories` 格式：`url|flags;url|flags`，`flags` 为 `hat` / `visor` / `plate`（不写默认 `hat`），多个仓库用 `;` 分隔。

装饰品缓存目录：`{persistentDataPath}/CustomCosmetics`（Android 为 `Android/data/<包名>/files/CustomCosmetics`），子目录 `CustomHats/ CustomVisors/ CustomNamePlates/`。

## 仓库配置格式

每个仓库根目录放三个配置文件，资源文件放对应子目录（`{url}/{resDir}/{文件名}`）。

### `CustomHats.json`

```json
{
  "packages": [                                        // 包分组定义（可选）
    { "package": "HatsPack",                           // 包 ID
      "displayName": "My Hats",                        // 界面显示的包名
      "priority": 50 }                                 // 排序权重（越大越靠后）
  ],
  "hats": [
    {
      "name": "Name",                                  // 装扮名
      "author": "Author",                              // 作者
      "package": "HatsPack",                           // 所属包ID
      "resource": "example.png",                       // 主图
      "climbresource": "example_climb.png",            // 爬梯动画图（可选）
      "backresource": "example_back.png",              // 后层图（可选）
      "flipresource": "example_flip.png",              // 翻转图（可选）
      "backflipresource": "example_back_flip.png",     // 背后翻转图（可选）
      "adaptive": false,                               // 自适应颜色
      "bounce": false,                                 // 弹跳动画
      "behind": false,                                 // 渲染在人物背后
      "autoscale": true,                               // 自动缩放至 300px 基准（默认开）
      "reshasha": "",                                  // 主图 MD5
      "reshashb": "",                                  // back MD5
      "reshashc": "",                                  // climb MD5
      "reshashf": "",                                  // flip MD5
      "reshashbf": ""                                  // back_flip MD5
    }
  ]
}
```

### `CustomVisors.json`

```json
{
  "packages": [                                        // 包分组定义（可选）
    { "package": "VisorPack", "displayName": "My Visors", "priority": 50 }
  ],
  "visors": [
    {
      "name": "Name",                                  // 装扮名
      "author": "Author",                              // 作者
      "package": "VisorPack",                          // 所属包ID
      "resource": "visor.png",                         // 主图
      "flipresource": "visor_flip.png",                // 翻转图（可选）
      "behindHats": false,                             // 渲染在帽子后面
      "adaptive": false,                               // 自适应颜色
      "autoscale": true,                               // 自动缩放至 300px 基准（默认开）
      "reshasha": "",                                  // 主图 MD5
      "reshashf": ""                                   // flip MD5
    }
  ]
}
```

### `CustomNamePlates.json`

```json
{
  "packages": [                                        // 包分组定义（可选）
    { "package": "PlatePack", "displayName": "My NamePlates", "priority": 50 }
  ],
  "nameplates": [
    {
      "name": "Name",                                  // 装扮名
      "author": "Author",                              // 作者
      "package": "PlatePack",                          // 所属包ID
      "resource": "plate.png",                         // 主图
      "reshasha": ""                                   // 主图 MD5
    }
  ]
}
```

## 图片规范

| 类型 | 基准尺寸 | 缩放 |
|---|---|---|
| 帽子 | 300×375 | `autoscale` 默认开启，大图自动缩放到基准显示 |
| 面饰 | 300×375 | `autoscale` 默认开启，大图自动缩放到基准显示 |
| 名牌 | 275×68 | 固定自动缩放 |

## 常见问题

**每次启动都全量下载？**

配置文件里的 `reshasha` 等 MD5 字段与文件实际 MD5 不匹配（或缺失）时会判定为"需要下载"。补真实 MD5 后只有变更的文件才会下载。

## 构建

.NET 6.0 + BepInEx IL2CPP
