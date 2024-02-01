using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Fido2NetLib;

namespace EDP_Backend.Models
{
    public class SaveCredentialsOptionsRequest
    {
        public AuthenticatorAttestationRawResponse AttestationResponse { get; set; }
        public string Options { get; set; }
	}
}
