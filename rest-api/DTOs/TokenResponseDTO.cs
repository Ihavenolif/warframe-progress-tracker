namespace rest_api.DTO
{
    public class TokenResponseDTO
    {
        /// <summary>
        /// The JWT token issued upon successful authentication.
        /// </summary>
        public string Token { get; set; }
    }
}