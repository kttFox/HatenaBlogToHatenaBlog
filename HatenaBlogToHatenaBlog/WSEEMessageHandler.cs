using System.Security.Cryptography;
using System.Text;

namespace HatenaBlogToHatenaBlog;

/// <summary>
/// WSSE認証用
/// </summary>
/// <param name="hatenaId"></param>
/// <param name="apiKey"></param>
public class WSEEMessageHandler( string hatenaId, string apiKey ) : HttpClientHandler {
	protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken ) {
		request.Headers.Add( "X-WSSE", CreateHeaderXWSSE() );
		return base.SendAsync( request, cancellationToken );
	}

	string CreateHeaderXWSSE() {
		var nonce = RandomNumberGenerator.GetBytes( 40 );
		var created = DateTime.UtcNow.ToString( "o" );
		var digest = SHA1.HashData( nonce.Concat( Encoding.UTF8.GetBytes( created + apiKey ) ).ToArray() );

		var credentials
			= "UsernameToken "
			+ $"Username=\"{hatenaId}\", "
			+ $"PasswordDigest=\"{Convert.ToBase64String( digest )}\", "
			+ $"Nonce=\"{Convert.ToBase64String( nonce )}\", "
			+ $"Created=\"{created}\"";

		return credentials;
	}
}
