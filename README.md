# HatenaBlogToHatenaBlog
はてなブログAtomPubを使用して、はてなブログからはてなブログへ移行します。   
記事、画像が対象です。（**コメントは移行されません**）  
記法(見たまま/はてな記法/Markdown)の変更は`はてなブログAtomPub`の仕様上できないので、事前に変更する必要があります。  
カスタムURLに関しても初期値はブログの設定になるようです。  
画像は移行先のはてなフォトにアップロードされます。  

はてなブログAtomPub   
https://developer.hatena.ne.jp/ja/documents/blog/apis/atom

# 準備
移行先の設定を事前に変更します。  
・記法（基本設定-編集モード）  
・カスタムURL（詳細設定-フォーマット）  

# 使い方
移行元のはてなID、ブログID、APIキー  
移行先のはてなID、ブログID、APIキー を指定してください。

[実行例]  
![image](https://github.com/kttFox/HatenaBlogToHatenaBlog/assets/22765277/c64c0bd1-9c2a-4547-91af-c1cd535ed9ff)

# オプション
```
-o,  --oldHatenID   <oldHatenID>   (必須) 移行元のはてなID  
-ob, --oldBlogID    <oldBlogID>    (必須) 移行元のブログID (xxxxx.hatenablog.jp)  
-oa, --oldHatenAPI  <oldHatenAPI>  (必須) 移行元のはてなAPIキー  
-n,  --newHatenID   <newHatenID>   (必須) 移行先のはてなID  
-nb, --newBlogID    <newBlogID>    (必須) 移行先のブログID (yyyyy.hatenablog.jp)  
-na, --newHatenAPI  <newHatenAPI>  (必須) 移行先のはてなAPIキー  
-nf, --newSubFolder <newSubFolder>        移行先のはてなフォトのサブフォルダ [default: Hatena Blog Import]  
```
# おすすめ
はてなIDが変更できないので、新しいはてなIDを取得して移行するために作成しました。  
元の記事内にはてなIDが含まれている可能性があるので、ソースコードから記事内の置換を追記して実行がおすすめです。
