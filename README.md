# Tonjiru

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20170606/20170606214904.png)

開いてるウィンドウを全部閉じてデスクトップをキレイにするアプリです。

- シンプル（機能不足ともいう）
- Windows Store Apps に（一応）対応している
- 除外リスト機能を搭載している

のがウリです。

# ダウンロード

https://github.com/daruyanagi/Tonjiru/releases

.NET Framework 4.6.1 が必要です（Windows 10 Creators Update を推奨）

# 更新情報

http://blog.daruyanagi.jp/archive/category/Tonjiru

# 使い方

## 1. UI レスでの利用（［Shift］キーを押しながら起動）

［Shift］キーを押しながら tonjiru.exe を起動すると、ウィンドウをすべて閉じて終了します（GUI なし）。コマンドラインで /s スイッチを付けて起動しても同じ効果になります。

exclusions.txt に書かれたプロセス（小文字）に属するウィンドウは無視されるので、閉じたくないウィンドウがあれば GUI モードであらかじめ登録しておくといいです。

## 2. GUI を利用（そのまま起動）

普通に起動すると GUI が現れます。リストビューにウィンドウが列挙されるので、閉じたいウィンドウにチェックを入れて、［Close All Windows And Exit］ボタンを押すと、選択されたウィンドウを閉じて終了します。

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20170606/20170606214302.png)

exclusions.txt の編集も GUI で行えます（コンテキストメニューから追加・削除）。UI-less モードのときに閉じたくないプロセスをここで指定しておけます。

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20170608/20170608192727.png)

<del>v1.2.0 からはジャンプリストや /g スイッチでも GUI が利用できるようになりました</del>

v1.4.0 からは以前と逆の挙動になりました。ジャンプリストはそのままですが、v1.5.0 で直すと思います
