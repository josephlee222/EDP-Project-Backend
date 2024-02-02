using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Fido2NetLib;

namespace EDP_Backend.Models
{
    public class LoginWithPasskeyRequest
	{
        public AuthenticatorAssertionRawResponse AttestationResponse { get; set; }
        public string Options { get; set; }
	}
}
