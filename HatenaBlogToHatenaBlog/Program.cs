using System;
using System.CommandLine;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace HatenaBlogToHatenaBlog;

internal class Program {
	static async Task Main( string[] args ) {
		var op1 = new Option<string>( new[] { "-o", "--oldHatenID" }, "移行元のはてなID" ) { IsRequired = true };
		var op2 = new Option<string>( new[] { "-ob", "--oldBlogID" }, "移行元のブログID(xxxxx.hatenablog.jp)" ) { IsRequired = true };
		var op3 = new Option<string>( new[] { "-oa", "--oldHatenAPI" }, "移行元のはてなAPIキー" ) { IsRequired = true };
		var op4 = new Option<string>( new[] { "-n", "--newHatenID" }, "移行先のはてなID" ) { IsRequired = true };
		var op5 = new Option<string>( new[] { "-nb", "--newBlogID" }, "移行先のブログID(yyyyy.hatenablog.jp)" ) { IsRequired = true };
		var op6 = new Option<string>( new[] { "-na", "--newHatenAPI" }, "移行先のはてなAPIキー" ) { IsRequired = true };
		var op7 = new Option<string>( new[] { "-nf", "--newSubFolder" }, () => "Hatena Blog Import", "移行先のはてなフォトのサブフォルダ" ) { AllowMultipleArgumentsPerToken = true };
		var rootCommand = new RootCommand( """
			はてなブログからはてなブログへ移行します。
			記事、画像が対象です。（コメントは移行されません）
			記法(見たまま/はてな記法/Markdown)の変更は[はてなブログAtomPub]の仕様上できないので、事前に変更する必要があります。
			""" ) {
			op1 ,op2 ,op3 ,op4 ,op5 ,op6 ,op7
		};

		rootCommand.SetHandler( Run, op1, op2, op3, op4, op5, op6, op7 );

		await rootCommand.InvokeAsync( args );
	}

	public static async Task Run(
			string oldHatenaID,
			string oldHatenaBlogID,
			string oldHatenaAPI,
			string newHatenaID,
			string newHatenaBlogID,
			string newHatenaAPI,
			string newSubFolder = "Hatena Blog Import"
		) {
		Console.WriteLine( $"{oldHatenaID} [{oldHatenaBlogID}] から\n{newHatenaID} [{newHatenaBlogID}]へ移行します。" );

		using var oldHatenaBlog = new HatenaBlog( Id: oldHatenaID, BlogId: oldHatenaBlogID, APIKey: oldHatenaAPI );
		using var newHatenaBlog = new HatenaBlog( Id: newHatenaID, BlogId: newHatenaBlogID, APIKey: newHatenaAPI );

		var entries = oldHatenaBlog.EnumerateEntriesAsync().ToBlockingEnumerable().ToArray();

		Console.WriteLine( $"全 {entries.Length} の記事が移行されます。よろしいですか？(Y/N)(default:N)" );
		if( Console.ReadLine()?.ToString().ToLower() != "y" ) return;

		foreach( var (entry, index) in entries.OrderBy( x => DateTime.Parse( x.Updated()!.Value ) ).Select( ( x, i ) => (x, i) ) ) {
			await Console.Out.WriteLineAsync( $"[{index + 1}/{entries.Length}] {entry.Title()!.Value}" );

			var content = entry.Content()!;

			// 古いブログの画像を新しいブログにアップロードして、記事内の画像IDを差し替える
			content.Value = Regex.Replace( content.Value, $@"\[f:id:{oldHatenaBlog.Id}:([0-9]+).*?]", m => {
				var oldPhotoId = m.Groups[1].Value;
				Console.Out.Write( "\t Image Download: " + oldPhotoId );
				var (path, contentType) = oldHatenaBlog.PhotoDownloadAsync( oldPhotoId ).Result;
				Console.Out.WriteLine( " [Complate]" );

				Console.Out.Write( "\t         Upload: " + path );
				var newPath = newHatenaBlog.PhotoUploadAsync( path, contentType, newSubFolder ).Result;
				var newPhotoId = Path.GetFileNameWithoutExtension( newPath );
				Console.Out.WriteLine( $" => {newPhotoId} [Complate]" );

				return m.Value.Replace( oldPhotoId, newPhotoId )
							.Replace( oldHatenaBlog.Id, newHatenaBlog.Id );
			} );

			// 記事内に古いIDがある場合に、新しいIDがに置換する
			//content.Value = content.Value.Replace( oldHatenaBlog.Id, newHatenaBlog.Id );

			// カスタムURL
			//var alternateURL = entry.Elements(AtomPub.Namespaces.Atom + "link")!.First(x => x.Attribute("rel")?.Value == "alternate" ).Attribute("href")!.Value;
			//var customURL = alternateURL[ ($"https://{oldHatenaBlog.BlogId}/entry/").Length ..];
			//await newHatenaBlog.PostEntry( entry, customURL );

			// 新しいブログに投稿
			await newHatenaBlog.PostEntry( entry );
		}

		await Console.Out.WriteLineAsync( "[Finished]" );
	}
}


file static class Extentions {

	public static XElement? Entry( this XElement element ) {
		return element.Element( AtomPub.Namespaces.Atom + "entry" );
	}

	public static XElement? Id( this XElement element ) {
		return element.Element( AtomPub.Namespaces.Atom + "id" );
	}

	public static XElement? Title( this XElement element ) {
		return element.Element( AtomPub.Namespaces.Atom + "title" );
	}

	public static XElement? Content( this XElement element ) {
		return element.Element( AtomPub.Namespaces.Atom + "content" );
	}

	public static XElement? Updated( this XElement element ) {
		return element.Element( AtomPub.Namespaces.Atom + "updated" );
	}

}