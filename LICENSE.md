# License

このリポジトリには、性質の異なる複数の構成要素が含まれています。
それぞれ適用されるライセンスが異なりますので、利用前に必ず本ファイル全体をご確認ください。

| 構成要素 | 範囲 | ライセンス |
|---|---|---|
| ソースコード | `Assets/Scripts/` 配下、および `.cs` ファイル全般 | MIT License |
| 自作アセット | 上記以外の自作ファイル(画像・音声・楽曲等) | All Rights Reserved |
| 第三者ライブラリ・アセット | DOTween, R3, Yarn Spinner, MaruMonica フォント 等 | 各ライブラリ元のライセンス |

---

## 1. ソースコード(MIT License)

`Assets/Scripts/` 配下のスクリプトおよびその他の自作 `.cs` ファイルは、以下の MIT License の下で利用を許諾します。

```
MIT License

Copyright (c) 2026 haharman

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

※ 本セクションはソースコードのみに適用されます。第三者ライブラリ(後述)、および画像・音声等の自作アセットには適用されません。

---

## 2. 自作アセット(All Rights Reserved)

`Assets/Scripts/` を除く、本リポジトリに含まれる自作の画像・スプライト・サウンド・音楽・その他クリエイティブ素材の著作権は、すべて著作者(haharman)に帰属します。

Copyright (c) 2026 haharman. All Rights Reserved.

- 閲覧・参考(学習目的での観察、技術的な参考)は自由に行っていただけます。
- 上記アセットそのものの**再利用・再配布・改変・商用/非商用での二次利用**は、著作者の事前の許可なく行うことを禁止します。
- 利用許可に関するお問い合わせは、リポジトリのIssue等を通じてご連絡ください。

---

## 3. Third-Party Notices

本プロジェクトは以下の第三者ライブラリ・アセットを利用しています。これらはそれぞれの配布元が定めるライセンスの下で提供されており、本ライセンスの対象外です。利用にあたっては各ライブラリの公式ライセンス条項をご確認ください。

### 3.1 ライブラリ・プラグイン

- **DOTween** — Demigiant (Daniele Giardini)。トゥイーンアニメーションライブラリ。
  `Assets/Plugins/Demigiant/DOTween/`。http://dotween.demigiant.com/license.php
- **R3** — Cysharp。リアクティブプログラミングライブラリ(Reactive Extensions for .NET / Unity)。MIT License。
  UPM (`com.cysharp.r3`) および NuGet (`R3` v1.3.0) 経由で導入。
- **Yarn Spinner for Unity** — Yarn Spinner / Secret Lab。ダイアログ・ナラティブツール。MIT License。
  UPM (`dev.yarnspinner.unity`) 経由で導入。
- **NuGetForUnity** — GlitchEnzo。Unity 向け NuGet パッケージマネージャ。MIT License。
  UPM (`com.github-glitchenzo.nugetforunity`) 経由で導入。

### 3.2 フォント

- **x12y16pxMaruMonica(まるもんじゃ)** — ピクセル日本語フォント。`Assets/Fonts/maru_monica.asset`(TextMesh Pro フォントアセット)。
  配布元のフォントライセンスに準拠。
- **Liberation Sans** — TextMesh Pro 同梱のデフォルトフォント(SIL Open Font License)。
  `Assets/TextMesh Pro/`。

(上記は主要なものの一覧であり、その他 Unity Package Manager / NuGet 経由で導入したパッケージ等にもそれぞれのライセンスが適用されます。正確な依存パッケージ一覧は `Packages/manifest.json` および `Assets/packages.config` を参照してください。)

---

*Last updated: 2026-06-22*
