# Tonjiru

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20170606/20170606214904.png)

開いてるウィンドウを全部閉じてデスクトップをキレイにするアプリです。

- シンプル（機能不足ともいう）
- Windows Store Apps に（一応）対応している
- 除外リスト機能を搭載している

のがウリです。

# ダウンロード

https://github.com/daruyanagi/Tonjiru/releases

# 更新情報

http://blog.daruyanagi.jp/archive/category/Tonjiru

# 使い方

## 1. UI レスでの利用（そのまま起動）

ダブルクリックで tonjiru.exe を起動すると、ウィンドウをすべて閉じて終了します（GUI なし）。タスクバーにピン留めして使うと便利かも。

exclusions.txt に書かれたプロセス（小文字）に属するウィンドウは無視されるので、閉じたくないウィンドウがあれば登録しておくといいです。

## 2. GUI を利用（［Shift］キーを押しながら起動）

［Shift］キーを押しながら起動すると、GUI が現れます。リストビューにウィンドウが列挙されるので、閉じたいウィンドウにチェックを入れて、［Close All Windows And Exit］ボタンを押すと、選択されたウィンドウを閉じて終了します。

![](https://cdn-ak.f.st-hatena.com/images/fotolife/d/daruyanagi/20170606/20170606214302.png)

exclusions.txt の編集も GUI で行えます（コンテキストメニューから追加・削除）。
