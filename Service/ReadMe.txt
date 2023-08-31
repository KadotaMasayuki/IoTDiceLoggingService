■機能

MONO-WIRELESS製のTWELITE CUE ( https://mono-wireless.com/jp/products/twelite-cue/index.html ) を「6つの状態を持つ無線スイッチ」として、そのケース表面に示されているサイコロ目を受信・蓄積するための、Windows用プログラムです。
受信するためには、MONO-WIRELESS製TWELITE CUE ( https://mono-wireless.com/jp/products/twelite-cue/index.html ) をWindowsPCに接続する必要があります。
Windowsサービスとして動作します。
インストール時の指定によって、ログイン不要のシステムアカウント "LocalSystem" で動作させます。
設定ファイルを用いることで、受信データの保存先フォルダと、COMポート番号と、6面の名称、スイッチの名称、中継器があればその名称、を変更できます。
サービスを開始したタイミングで設定が読みなおされます。設定を変更したら、サービスを停止し再度開始すれば、新しい設定で動作します。
データを受信したタイミングで、TWELITE CUEのデータであれば、加速度センサ値を整形し、ログ保存します。
※蓄積したデータを分析・表示するプログラムは同梱しません。


■構成

Windows PC ( Windows10 Pro 22H2 にて動作確認済み )
　・・本プログラムを実行する
MONO-WIRELESS製TWELITE CUE ( https://mono-wireless.com/jp/products/twelite-cue/index.html )
　・・「6つの状態を持つ無線スイッチ」として使う
MONO-WIRELESS製MONOSTICK ( https://mono-wireless.com/jp/products/MoNoStick/index.html )
　・・Windows PCのUSBポートに接続し、TWELITE CUEからのデータを受信する
　・・必要があれば中継器のファームウェアを書き込んだMONOSTICKを追加して、距離を延ばすことができる


■コンパイル

コマンドラインから以下を実行します。開発環境のインストールは不要です。

> c:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild   IoTDiceLoggingService.csproj


以下のファイルが生成されます。

　bin/Debug/IoTDiceLoggingService.exe
　bin/Debug/Settings.txt


警告が３つ出ることがあります。

warning MSB3644: フレームワーク ".NETFramework,Version=v4.7.2" の参照アセンブリが見つかりませんでした。
→出来上がったファイルを実行する際に、DotNetFramework 4.7.2以上があれば良いです

warning MSB3270: 構築されているプロ ジェクトのプロセッサ アーキテクチャ "MSIL" と、参照 "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscorlib.dll" のプロセッサ アーキテクチャ "AMD64" の間には不一致がありました。
→問題ないようです

warning MSB3270: 構築されているプロ ジェクトのプロセッサ アーキテクチャ "MSIL" と、参照 "System.Data" のプロセッサ アーキテクチャ "AMD64" の間には不一致がありました。
→問題ないようです


■インストール方法

以下を配置してください。

　c:\IoTDice\bin\IotDiceLoggingService.exe
　c:\IoTDice\bin\Settings.txt

MONOSTICKをUSBポートに接続し、COMポートとして認識させてください。
その後、Settings.txtと、MONOSTICKのCOMポート番号を合わせてください。COMポートの速度やビット長などの設定は不要です。

Controlフォルダ内の「INSTALL_administrator.bat」を管理者権限で実行することで、インストールできます。
Controlフォルダ内の「CHECK.bat」を実行することで、インストールが成功したか、動作しているか、といった情報を確認できます。
状態が「START PENDING」の場合、1分程度待ってからもう一度「CHECK.bat」を実行してみてください。
RUNNINGにならない場合、COMポートの番号が違っている可能性があります。設定を確認し、正しい構成であることが確認できたら、「STOP_administrator.bat」を管理者権限で実行し、数秒待ってから「START_administrator.bat」を管理者権限で実行してください。その後、「CHECK.bat」を実行し、RUNNNINGになっていることを確認してください。
TWELITE_CUEに衝撃を加えるか、規定の時間が経過するたびに、ケースの上面の情報が c:/IoTDice/Log に保存されます。


■アンインストール方法

Controlフォルダ内の「STOP_administrator.bat」を管理者権限で実行してください。
Controlフォルダ内の「CHECK.bat」を実行し、STOPEDになっていたら、「INSTALL_administrator.bat」を管理者権限で実行することで、サービスをアンインストールできます。


■6つの状態の送信方法

TWELITE CUEの仕様のままであり、

・定期的にTWELITE CUEの状態を送信
・衝撃が与えられた時のTWELITE CUEの状態を送信

します。この時送信される加速度データを用いて、上を向いている面を本プログラムが推定します。
推定方法は単純で、X,Y,Zそれぞれ10個の加速度データが送信されるため、それぞれの値を平均して、もっとも絶対値の大きい値をもとに、上になっている面を推定します。


■ログの書式

BOMありUTF8、改行コードはCRLF、タブ区切り、で記述します。
ファイル名は、4桁の年、ハイフン、2桁の月、ハイフン、2桁の日、.csv です。（例　2023-08-15.csv）
その年月日に受信したデータはそのファイルの末尾に追記します。
例： 2023/5/30 19:08:30 に取得したデータは、2023-05-30.csv に記録される。
1行目は列ヘッダー。
2行目から実データ。
データの並びは、
    TWELITE_CUEの8桁のシリアルID
    このシリアルIDを持つTWELITE_CUEの呼び名（設定ファイルより）
    中継器のシリアルID。中継していなければ 80000000 。
    中継器の呼び名（設定ファイルより）
    このTWELITE_CUEの電池電圧[mV]。
    加速度X（静止状態で、上向きなら+1000付近、下向きなら-1000付近、横向きなら0付近の値になる）
    加速度Y
    加速度Z
    上を向いている面番号
    上を向いている面の名前（設定ファイルより）
    LQI
    受信時点のPCの年月日時分秒（例：2023/07/26 12:32:44）


■設定ファイルの書式

BOMありUTF8、タブ区切り、改行コードはCRLF、で記述します。
ファイル名は "Settings.txt"。
ファイルの場所は、本プログラムと同じフォルダ。

'[' と ']' で囲んだキーワード行から、次のキーワード行までの間に、そのキーワードの設定を記述します。

[log]
ログを記録するフォルダのパス（例：C:\test\log）

[com]
COMポート番号（例：COM6）、

[relay]
中継器のシリアルIDとその呼び名をタブ区切りで
1行に1個、必要分だけ記載する

[face]
TWELITE CUEの8桁のID、その呼び名、面１の呼び名、面２の、、、面６の呼び名　をタブ区切りで）
1行に1個、必要分だけ記載する

※以下は未実装
ログフォルダの代わりに、データベースサーバーに記録することもできるようになると良いな
[database]
サーバーアドレス
アカウント
パスワード
その他、必要な情報


■TWELITE CUEの設定

TWELITE CUEは購入時のまま未設定でも使えます。
電池を長もちさせるために、送信間隔を長くする設定（↓）を行うと良いです。
※必要とするログの記録間隔に合わせて調整してください

TWE-Programmer ( https://tweprogrammer.twelite.info/ )（例：TWE-Programmer_0_3_7_5.zip）を用い、MONOSTICKにAPP_CUE用のOTAファームウェア
（例：App_CUE_OTA_BLUE_L1304_V1-0-2.bin）を書き込む。
TWE-ProgrammerでMONOSTICKのインタラクティブターミナルを表示する。
送信間隔 't' を、集計間隔より30秒程度短くする（例：5分間隔で集計するなら、4分30秒＝270秒にする。送信間隔をできるだけ長くすることで電池の持ちが良くなる）。
設定を保存する 'S' 。これで、このMONOSTICKからこの設定を複数のTWELITE CUEに連続して書き込むことができる。

設定後のMONOSTICKの状態
--- CONFIG/App_CUE V1-01-1/SID=0x810aaaaa/LID=0x01 ---
 a: set Application ID (0x67720102)
 i: set Device ID (--)
 c: set Channels (18)
 x: set Tx Power (13)
 b: set UART baud (38400)
 B: set UART option (8N1)
 k: set Enc Key (0xA5A5A5A5)
 o: set Option Bits (0x00000001)
 t: set Transmission Interval (270)
 p: set Senser Parameter (0x00000000)
---
 S: save Configuration
 R: reset to Defaults
 *** POWER ON TWELITE CUE NEAR THIS CONFIGURATOR ***

このまま、TWELITE_CUEをMONOSTICKの上に置き、TWELITE_CUEに磁石を近づけたり遠ざけたりを5回以上繰り返すと、TWELITE_CUEのLEDとMONOSTICKのLEDが点滅し、OTA設定が完了する。MONOSTICKのターミナルに「SUCCESS」と表示されれば成功。失敗は
「FAILURE」。ゆっくり磁石を操作すると成功しやすい。
設定を終えたTWELITE_CUEをMONOSTICKから遠ざけ、次のTWELITE_CUEをMONOSTICKの上に置き、同じように磁石を操作することで、同一の設定ができる。
使用するTWELITE_CUE全てにこの操作を行い設定する。


■サービスインストール方法の解説（PowerShellの場合：PowerShellバージョンが古いとサービス削除コマンドがないため、PowerShellでの操作はお勧めしない）

PowerShell (バージョン5.1で可能なことを確認済み) 管理者モードでインストールする
https://learn.microsoft.com/ja-jp/powershell/module/microsoft.powershell.management/new-service?view=powershell-7.3
PowerShellのエスケープ文字はバッククオート「`」だそうなので、ディレクトリ区切り文字「\」はそのまま使える。
※LocalSystemアカウントで動くようにインストールしたい。-credentialオプションかな？　わからん。
  なにも指定なしでやるとLocalSystemになるので、まあ良いか。
 new- service - binarypathname "C:\xxxx\yyyyy\IoTDiceLoggingService.exe" - startuptype Automatic - description "logging data from IoT Dices" - displayname "IoT Dice Logger" - name "IoTDiceLoggingService"

◇開始
コマンドラインまたはPowerShell 管理者モードでサービスを始動する
 net start IoTDiceLoggingService

◇停止
コマンドラインまたはPowerShell 管理者モードでサービスを停止する
 net stop IoTDiceLoggingService

◇アンインストール
PowerShell (バージョン6以上が必要) 管理者モードでアンインストールする
https://learn.microsoft.com/ja-jp/powershell/module/microsoft.powershell.management/remove-service?view=powershell-7.3
 remove-service -name IoTDiceLoggingService


■インストール方法の解説（コマンドラインでSCコマンド。オススメ。「Control」フォルダ内のバッチファイルはこの方法で実施）

◇インストール
sc create  -  https://windows.command-ref.com/cmd-sc-create.html
  <管理者プロンプト>  sc  create  ServiceName  type= own  start= auto  error= normal  binpath= "c:\xxx\xxx\xxx.exe"  displayname= "Display Name"  obj= "LocalSystem"
  ※ start= auto のように、=の後ろに半角スペースが必要

◇削除
sc delete  -  https://windows.command-ref.com/cmd-sc-delete.html
  <管理者プロンプト>  sc  delete  ServiceName
  ※ 削除できなかったときは、再起動時に削除されるようマーキングされる

◇開始
sc start  -  https://windows.command-ref.com/cmd-sc-start.html
  <通常プロンプト>  sc  start  ServiceName
  ※ 起動し切る前にコマンドが終わるので、ちょっと待ってからサービス使用しなければ起動していない可能性がある。
  services.msc か sc query で確認すると良い

◇停止
sc stop  -  https://windows.command-ref.com/cmd-sc-stop.html
  <通常プロンプト>  sc  stop  ServiceName
  ※ 停止し切る前にコマンドが終わるので、ちょっと待たないとサービス停止していない可能性がある。
  services.msc か sc query で確認すると良い

◇状態確認
sc query  -  https://windows.command-ref.com/cmd-sc-query.html
  <通常プロンプト>  sc  query  ServiceName
  サービスが動いていれば
     STATE : 4 RUNNING
  と表示される
  sc queryex や、 sc qc もいろいろ情報が出る。


■送信データの書式（TWELITE_CUE のファームウェア app_cue からのデータ）

    データ位置[1文字目:1文字目＋文字数]	データ例	意味
    [0:0+1] :   開始文字
    [1:1+8]	80000000	中継機のシリアルID
    [9:9+2]	AE	LQI
    [11:11+4]	0098	データ送信のたびに＋１される続き番号
    [15:15+8]	810B6492	送信元のシリアルID
    [23:23+2]	01	送信元の論理デバイスID
    [25:25+2]	80	センサー種別(80で固定)
    [27:27+2]	05	PAL基板バージョンとPAL基板ID(TWELITE_CUEは05)
    [69:69+4]	0D34	電池電圧[mV]
    [93:93+2]	80	磁気センサー（下表）
    [103:103+12]	FFF00010FC18	加速度データ１ FFF0(X軸)/0010(Y軸)/FC18(Z軸)
    [123:123+12]	FFF00018FC18	加速度データ２ FFF0(X軸)/0018(Y軸)/FC18(Z軸)
    [143:143+12]	FFF00010FC00	...
    [163:163+12]	FFF80000FC10	
    [183:183+12]	FFF00010FC18	
    [203:203+12]	FFE00018FBF8	
    [223:223+12]	FFE00018FBF8	
    [243:243+12]	FFE80010FBF8	
    [263:263+12]	FFE80010FC08	
    [283:283+12]	FFE80010FC08	加速度データ10


■送信データの中の、磁気センサーのデータ[93:93+2]の意味

    00	：　磁石が遠ざかった。
    01	：　磁石のN極が近づいた。
    02	：　磁石のS極が近づいた。
    80	：　磁石が近くにない。(タイマーによる定期送信)
    81	：　磁石のN極が近くにある。(タイマーによる定期送信)
    82	：　磁石がS極が近くにある。(タイマーによる定期送信)


■送信データの中の、加速度データ[103:103+12]...の意味（XYZ軸の重力加速度）
    0010FFF10123　：　X=0x0010(=16mg), Y=0xFFF1(=-15mg), Z=0x0123(=291mg)
    静止状態では +1024 ～ -1048[mg] 程度の範囲を示す


以上
