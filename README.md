# CustomCosmetics

Among Us 自定义装饰品加载器。从远程仓库下载并加载自定义**帽子 / 面饰 / 名牌**，支持多仓库、包分组、增量下载。

## 安装

1. 将 `CosmeticsManager.dll` 放入 `BepInEx/plugins/`
2. 启动游戏一次，生成配置文件 `Cosmetics/config.yml`（游戏目录下）
3. 编辑配置后重启游戏生效

## 配置文件 `Cosmetics/config.yml`

```yaml
cosmetics:
  # 解锁所有装饰品（含原版商店锁定项）
  unlocker: false
  # 仅本地模式：不进行任何网络下载，只读取本地缓存
  local: false

hats:
  enabled: true       # 启用帽子加载

visors:
  enabled: false      # 启用面饰加载

nameplates:
  enabled: false      # 启用名牌加载

repositories:
  # 仓库 URL
  - url: "https://raw.githubusercontent.com/TheOtherRolesAU/TheOtherHats/master"
    # 本地缓存目录名（可选）
    alias: "TheOtherHats"
    # 启用该仓库的哪些类型
    hats: true
    visors: false
    nameplates: false
    # 自定义配置文件名（可选，默认 CustomHats.json / CustomVisors.json / CustomNamePlates.json）
    hatsFile: "MyHats.json"
    visorsFile: "MyVisors.json"
    platesFile: "MyPlates.json"
    # 自定义资源子目录（可选，默认 hats/ visors/ nameplates/）
    hatsDir: "hats"
    visorsDir: "visors"
    platesDir: "nameplates"
```

多仓库可配置多个；同一类型会合并加载（按包分组显示）。

## 仓库配置格式

每个仓库根目录放三个配置文件，资源文件放对应子目录（`{url}/{resDir}/{文件名}`）。

### `CustomHats.json`

```json
{
  "packages": [                                        // 包分组定义（可选）
    { "package": "HatsPack",                           // 包 ID（条目里的 package 引用它）
      "displayName": "My Hats",                        // 界面显示的包名
      "priority": 50 }                                 // 排序权重（越大越靠前）
  ],
  "hats": [
    {
      "name": "Name",                                  // 装扮名
      "author": "Author",                              // 作者
      "package": "HatsPack",                           // 所属包
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
      "package": "VisorPack",                          // 所属包
      "resource": "visor.png",                         // 主图
      "flipresource": "visor_flip.png",                // 翻转图（可选）
      "behindHats": false,                             // 渲染在帽子后面
      "adaptive": false,                               // 自适应颜色
      "autoscale": false,                              // 自动缩放至 300px 基准（默认关
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
      "package": "PlatePack",                          // 所属包 
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
| 面饰 | 任意（默认原尺寸） | `autoscale` 默认关闭，开启后按 300 基准缩放 |
| 名牌 | 275×68 | 固定自动缩放 |

## 常见问题

**每次启动都全量下载？**

配置文件里的 `reshasha` 等 MD5 字段与文件实际 MD5 不匹配（或缺失）时会判定为"需要下载"。补真实 MD5 后只有变更的文件才会下载。

## 构建

.NET 6.0 + BepInEx IL2CPP，依赖 YamlDotNet。
