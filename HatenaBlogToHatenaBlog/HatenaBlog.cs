using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace HatenaBlogToHatenaBlog;

public class HatenaBlog( string Id, string BlogId, string APIKey ) : IDisposable {
	public string Id { get; } = Id;
	public string BlogId { get; } = BlogId;
	public string APIKey { get; } = APIKey;

	public HttpClient Client { get; } = new HttpClient( new WSEEMessageHandler( Id, APIKey ) );

	/// <summary>
	/// はてなブログの記事一覧を取得します。
	/// </summary>
	/// <returns></returns>
	public async IAsyncEnumerable<XElement> EnumerateEntriesAsync() {

		var nextUri = $"https://blog.hatena.ne.jp/{Id}/{BlogId}/atom/entry";

		while( nextUri is not null ) {
			var stream = await Client.GetStreamAsync( nextUri );
			var doc = await XDocument.LoadAsync( stream, LoadOptions.None, CancellationToken.None );

			var entries = doc.Element( AtomPub.Namespaces.Atom + "feed" )!
								.Elements( AtomPub.Namespaces.Atom + "entry" );

			foreach( var entry in entries ) {
				yield return entry;
			}

			nextUri = doc.Element( AtomPub.Namespaces.Atom + "feed" )!
											.Elements( AtomPub.Namespaces.Atom + "link" )!
											.FirstOrDefault( e => e.Attribute( "rel" )?.Value == "next" )?
											.Attribute( "href" )!.Value;
		}
	}

	/// <summary>
	/// はてなフォトから指定した写真IDの画像をカレントフォルダにダウンロードします。
	/// </summary>
	/// <param name="photoId"></param>
	/// <returns>ダウンロードしたファイルパス、画像タイプ</returns>
	public async Task<(string path, string contentType)> PhotoDownloadAsync( string photoId ) {

		var stream = await Client.GetStreamAsync( $"http://f.hatena.ne.jp/atom/edit/{photoId}" );
		var doc = await XDocument.LoadAsync( stream, LoadOptions.None, CancellationToken.None );
		var url = doc.Element( AtomPub.Namespaces.Root + "entry" )!
						.Element( AtomPub.Namespaces.Hatena + "imageurl" )!.Value;

		var path = @"./" + Path.GetFileName( url );
		var image = await Client.GetAsync( url );

		using( var w = File.OpenWrite( path ) ) {
			await ( await image.Content.ReadAsStreamAsync() ).CopyToAsync( w );
		}

		return (path, image.Content.Headers.ContentType!.ToString());
	}

	/// <summary>
	/// はてなフォトに画像をアップロードします。
	/// </summary>
	/// <param name="path">アップロードする画像ファイルのパス</param>
	/// <param name="contentType">アップロードする画像タイプ</param>
	/// <param name="subFolder">はてなフォトのアップロード先のサブフォルダ</param>
	/// <returns>アップロード後のURL</returns>
	public async Task<string> PhotoUploadAsync( string path, string contentType, string? subFolder = null ) {
		var xml = new XElement( AtomPub.Namespaces.Root + "entry" );
		xml.Add( new XElement( "title", $"{Path.GetFileNameWithoutExtension( path )}" ) );
		xml.Add( new XElement( "content", new XAttribute( "mode", "base64" ), new XAttribute( "type", contentType ), Convert.ToBase64String( File.ReadAllBytes( path ) ) ) );
		if( subFolder is not null ) {
			xml.Add( new XElement( "{http://purl.org/dc/elements/1.1/}subject", new XAttribute( XNamespace.Xmlns + "dc", "http://purl.org/dc/elements/1.1/" ), subFolder ) );
		}

		var content = new StringContent( xml.ToString( SaveOptions.DisableFormatting ), Encoding.UTF8, contentType );
		var re = await Client.PutAsync( "http://f.hatena.ne.jp/atom/post", content );

		var doc = XDocument.Load( re.Content.ReadAsStream() );
		var uploadPath = doc.Element( AtomPub.Namespaces.Root + "entry" )!
							.Element( AtomPub.Namespaces.Root + "link" )!.Attribute( "href" )!.Value;

		return uploadPath;
	}

	/// <summary>
	/// はてなブログに記事を投稿します。
	/// </summary>
	/// <param name="entry"></param>
	/// <param name="customURL"></param>
	/// <returns></returns>
	public async Task PostEntry( XElement entry, string? customURL = null ) {
		if( customURL is not null ) {
			entry.AddFirst( new XElement( "{http://www.hatena.ne.jp/info/xmlns#hatenablog}" + "custom-url",
								new XAttribute( XNamespace.Xmlns + "hatenablog", "http://www.hatena.ne.jp/info/xmlns#hatenablog" ),
								customURL )
							);
		}

		var content = new StringContent( entry.ToString( SaveOptions.DisableFormatting ), Encoding.UTF8, "application/xml" );
		await Client.PostAsync( $"https://blog.hatena.ne.jp/{Id}/{BlogId}/atom/entry", content );
	}


	public void Dispose() {
		Client?.Dispose();
	}
}
